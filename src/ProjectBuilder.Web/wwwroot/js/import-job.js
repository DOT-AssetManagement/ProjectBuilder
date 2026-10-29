// Generic background-import form handler.
// Wires a <form> that uploads a file to a Razor Pages handler which enqueues a Hangfire job
// and returns { jobId }, then polls "?handler=JobStatus" (see TreatmentPoolLibraries/Index.cshtml.cs
// for the same pattern) until the job succeeds or fails, showing progress in a .job-status-banner.
function initBackgroundImportForm(options) {
    var form = document.getElementById(options.formId);
    var banner = document.getElementById(options.bannerId);
    if (!form || !banner) return;

    var spinner = banner.querySelector('.run-status-spinner');
    var messageEl = banner.querySelector('.job-status-message');
    var closeBtn = banner.querySelector('.run-status-close');
    var submitBtn = form.querySelector('[type=submit]');
    var pollTimer = null;

    function showStatus(state, message) {
        banner.className = 'job-status-banner run-status-' + state;
        messageEl.textContent = message;
        spinner.style.display = (state === 'running') ? 'block' : 'none';
        banner.style.display = 'flex';
    }

    function parseImportError(result) {
        if (!result) return 'Import failed. Please try again.';
        if (typeof result === 'string') return result;
        if (result.error) return result.error;
        if (Array.isArray(result)) return result.join('<br>');
        if (result.value && Array.isArray(result.value)) return result.value.join('<br>');
        return 'Import failed. Please try again.';
    }

    function poll(jobId) {
        if (pollTimer) clearInterval(pollTimer);
        pollTimer = setInterval(function () {
            $.get('?handler=JobStatus', { jobId: jobId }, function (res) {
                if (res.status === 'Succeeded') {
                    clearInterval(pollTimer); pollTimer = null;
                    sessionStorage.removeItem(options.storageKey);
                    showStatus('success', options.successMessage || 'Import completed successfully.');
                    if (submitBtn) submitBtn.disabled = false;
                    form.reset();
                } else if (res.status === 'Failed') {
                    clearInterval(pollTimer); pollTimer = null;
                    sessionStorage.removeItem(options.storageKey);
                    showStatus('error', 'Import failed: ' + (res.message || 'Unknown error'));
                    if (submitBtn) submitBtn.disabled = false;
                }
                // Enqueued / Processing — keep polling
            });
        }, 3000);
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            banner.style.display = 'none';
        });
    }

    form.addEventListener('submit', async function (event) {
        event.preventDefault();

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        if (options.beforeSubmit) options.beforeSubmit();

        var formData = new FormData(form);
        if (submitBtn) submitBtn.disabled = true;

        try {
            var response = await fetch(form.action, { method: 'POST', body: formData });
            var result = null;
            try { result = await response.json(); } catch (e) { /* non-json body */ }

            if (response.ok && result && result.jobId) {
                sessionStorage.setItem(options.storageKey, result.jobId);
                showStatus('running', options.runningMessage || 'Import running in the background…');
                poll(result.jobId);
            } else {
                showStatus('error', parseImportError(result));
                if (submitBtn) submitBtn.disabled = false;
            }
        } catch (error) {
            showStatus('error', 'Unexpected error. Please try again.');
            if (submitBtn) submitBtn.disabled = false;
        }
    });

    // Resume polling if a job was still running before a manual page refresh.
    var pendingJobId = sessionStorage.getItem(options.storageKey);
    if (pendingJobId) {
        if (submitBtn) submitBtn.disabled = true;
        showStatus('running', 'Import is still running in the background…');
        poll(pendingJobId);
    }
}
