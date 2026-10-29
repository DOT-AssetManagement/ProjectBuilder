// Reads the worksheet names out of a picked Excel file and offers them as Tab Name choices.
(function (window, document) {
    'use strict';

    var SHEETJS_URL = 'https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js';
    // Parsing happens in the browser, so skip huge files rather than freeze the tab.
    var MAX_READ_BYTES = 25 * 1024 * 1024;

    var loader = null;

    function loadSheetJs() {
        if (window.XLSX) return Promise.resolve(window.XLSX);
        if (!loader) {
            loader = new Promise(function (resolve, reject) {
                var script = document.createElement('script');
                script.src = SHEETJS_URL;
                script.onload = function () {
                    if (window.XLSX) resolve(window.XLSX);
                    else reject(new Error('Excel reader loaded but was unavailable.'));
                };
                script.onerror = function () {
                    loader = null;
                    reject(new Error('Could not load the Excel reader.'));
                };
                document.head.appendChild(script);
            });
        }
        return loader;
    }

    function readSheetNames(file) {
        return loadSheetJs().then(function (XLSX) {
            return new Promise(function (resolve, reject) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    try {
                        // bookSheets skips cell data, so only the workbook index is parsed.
                        var wb = XLSX.read(new Uint8Array(e.target.result), { type: 'array', bookSheets: true });
                        resolve(wb.SheetNames || []);
                    } catch (err) {
                        reject(err);
                    }
                };
                reader.onerror = function () { reject(new Error('Could not read the selected file.')); };
                reader.readAsArrayBuffer(file);
            });
        });
    }

    function findMatch(names, wanted) {
        if (!wanted) return null;
        var target = String(wanted).trim().toLowerCase();
        if (!target) return null;
        for (var i = 0; i < names.length; i++) {
            if (String(names[i]).trim().toLowerCase() === target) return names[i];
        }
        return null;
    }

    function byId(value) {
        return typeof value === 'string' ? document.getElementById(value) : value;
    }

    // opts: { fileInput, tabInput, tabSelect, preferred, onStatus }
    function attach(opts) {
        var fileInput = byId(opts.fileInput);
        var tabInput = byId(opts.tabInput);
        var tabSelect = byId(opts.tabSelect);
        if (!fileInput || !tabInput || !tabSelect) return { reset: function () { } };

        var preferred = opts.preferred || [];
        var status = typeof opts.onStatus === 'function' ? opts.onStatus : function () { };

        // Only the active control carries a name, so the form posts one tabname value.
        function showTyping() {
            tabSelect.innerHTML = '';
            tabSelect.hidden = true;
            tabSelect.disabled = true;
            tabInput.hidden = false;
            tabInput.disabled = false;
        }

        function showChoices(names, chosen) {
            tabSelect.innerHTML = '';
            names.forEach(function (name) {
                var option = document.createElement('option');
                option.value = name;
                option.textContent = name;
                if (name === chosen) option.selected = true;
                tabSelect.appendChild(option);
            });
            tabInput.hidden = true;
            tabInput.disabled = true;
            tabSelect.hidden = false;
            tabSelect.disabled = false;
        }

        fileInput.addEventListener('change', function () {
            var file = fileInput.files && fileInput.files[0];
            showTyping();
            if (!file) { status(''); return; }

            if (file.size > MAX_READ_BYTES) {
                status('File is too large to list tab names - please type the tab name.');
                return;
            }

            status('Reading tab names...');
            readSheetNames(file).then(function (names) {
                if (!names.length) {
                    status('No worksheets found in this file.');
                    return;
                }

                // Keep whatever the user typed if that tab exists, else prefer a known name.
                var chosen = findMatch(names, tabInput.value);
                for (var i = 0; !chosen && i < preferred.length; i++) {
                    chosen = findMatch(names, preferred[i]);
                }

                showChoices(names, chosen || names[0]);
                status(names.length === 1
                    ? 'This file has one tab.'
                    : 'Pick which of the ' + names.length + ' tabs to import.');
            }).catch(function () {
                // Silent fallback: the field stays free-text exactly as before.
                showTyping();
                status('');
            });
        });

        return {
            reset: function () {
                showTyping();
                status('');
            }
        };
    }

    window.pbTabNameAutofill = { attach: attach };
})(window, document);
