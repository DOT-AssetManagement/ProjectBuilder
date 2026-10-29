/**
 * PBTableState — sessionStorage-backed persistence for Tabulator sort/page state and
 * page-defined filter selections, keyed per page instance so grid state survives a
 * location.reload() and normal same-tab navigation away and back.
 */
var PBTableState = (function(){
  function storageKey(key){ return 'pbTableState:' + key; }

  function load(key){
    try {
      var raw = sessionStorage.getItem(storageKey(key));
      return raw ? JSON.parse(raw) : null;
    } catch (e) { return null; }
  }

  function save(key, patch){
    try {
      var merged = Object.assign({}, load(key) || {}, patch);
      sessionStorage.setItem(storageKey(key), JSON.stringify(merged));
    } catch (e) { /* storage unavailable or quota exceeded — state just won't persist */ }
  }

  function clear(key){
    try { sessionStorage.removeItem(storageKey(key)); } catch (e) { }
  }

  return { load: load, save: save, clear: clear };
})();

window.addEventListener('pageshow', function (event) {
  if (!event.persisted) return;
  var overlay = document.getElementById('overlay');
  if (overlay) overlay.style.display = 'none';
});

/**
 * createTable(selector, config)
 *
 * config = {
 *   data            : [],           // row data
 *   columns         : [],           // column definitions (Actions column is auto-appended)
 *   actions         : [             // expand-panel action buttons
 *     { label, icon, key?, type?, onClick? }
 *     // type:'delete' → red styling
 *     // onClick(rowData, row) → called on click
 *   ],
 *   maxHeightEl     : '.table-wrap',// CSS selector of container to measure height
 *   paginationSize  : 10,
 *   layout          : 'fitColumns', // Tabulator layout mode. Default shrinks/grows columns to
 *                                   // exactly fill the width — fine for a handful of columns,
 *                                   // but truncates headers/data once there are many. Pass
 *                                   // 'fitDataFill' for wide tables: columns size to their own
 *                                   // content/header and the table scrolls horizontally instead.
 *   stateKey        : undefined     // sessionStorage key for sort/page/filter persistence;
 *                                   // defaults to pathname+search, pass `false` to disable
 *   remote          : false,        // true: server-paginated via ajaxURL/ajaxParams instead of local `data`
 *   ajaxURL         : undefined,    // required when remote:true
 *   ajaxParams      : undefined,    // function returning extra query params; called with `this` bound to the table
 * }
 *
 * Returns the Tabulator instance. When state persistence is active (the default), the
 * instance also exposes:
 *   table.pbSavedFilters       // filters object restored from sessionStorage, or null
 *   table.pbSaveFilters(obj)   // persist the page's current filter-state object
 *
 * Sort and pagination page are persisted automatically; restoring the page-defined filter
 * dropdowns/state and re-applying them is up to the calling page (read pbSavedFilters once
 * at startup, apply it, and call pbSaveFilters(...) whenever the page's own filter state
 * changes — e.g. at the end of its applyAllFilters()/reset handlers).
 */
function createTable(selector, config){
  config = config || {};
  var actions      = config.actions      || [];
  var maxHeightEl  = config.maxHeightEl  || '.table-wrap';
  var paginationSize = config.paginationSize || 10;
  var stateKey     = config.stateKey === false ? null : (config.stateKey || (location.pathname + location.search));
  var savedState   = stateKey ? PBTableState.load(stateKey) : null;

  function getHeight(){
    var el = document.querySelector(maxHeightEl);
    return el ? el.offsetHeight : 400;
  }

  // The expand panel is a full-row-width child, so its centered content would sit off-screen
  // to the right when the table scrolls horizontally. Pin it (position:sticky;left:0 in CSS)
  // and size it to the visible scroll area so the action buttons stay centered in view.
  function sizeExpandPanels(){
    var holder = document.querySelector(selector + ' .tabulator-tableholder');
    if(!holder) return;
    var w = holder.clientWidth;
    document.querySelectorAll(selector + ' .row-expand-panel').forEach(function(p){
      p.style.width = w + 'px';
    });
  }

  function colMenu(e, column){
    var subMenu = [];
    column.getTable().getColumns().forEach(function(col){
      var title = col.getDefinition().title;
      if(!title) return;
      var visible = col.isVisible();
      subMenu.push({
        label: '<span class="col-menu-check"><input type="checkbox" ' + (visible ? 'checked' : '') + '/>' + title + '</span>',
        action: function(e){
          e.stopPropagation();
          var span = e.target.closest ? e.target.closest('.col-menu-check') : null;
          var cb = span ? span.querySelector('input[type=checkbox]') : null;
          if(cb) {
            if(e.target.tagName !== 'INPUT') cb.checked = !cb.checked;
            if(cb.checked) col.show(); else col.hide();
          } else {
            col.toggle();
          }
        }
      });
    });
    return [
      { label: '<i class="fa-solid fa-arrow-up-wide-short"></i> Sort Ascending',   action: function(){ column.getTable().setSort(column.getField(), 'asc');  } },
      { label: '<i class="fa-solid fa-arrow-down-wide-short"></i> Sort Descending', action: function(){ column.getTable().setSort(column.getField(), 'desc'); } },
      { separator: true },
      { label: '<i class="fa-solid fa-table-columns"></i> Columns', menu: subMenu }
    ];
  }

  function buildExpandPanel(row){
    var panel = document.createElement('div');
    panel.className = 'row-expand-panel';
    var data = row.getData();
    var btns = actions.map(function(a){
      var isDisabled = typeof a.disabled === 'function' ? a.disabled(data) : false;
      var cls = 'action-item' + (a.type === 'delete' ? ' action-delete' : '') + (isDisabled ? ' action-disabled' : '');
      var key = a.key || a.label;
      return '<button class="' + cls + '" data-action="' + key + '"' + (isDisabled ? ' disabled' : '') + '><i class="' + a.icon + '"></i> ' + a.label + '</button>';
    }).join('');
    panel.innerHTML = '<div class="action-bar">' + btns + '</div>';

    panel.querySelectorAll('.action-item').forEach(function(btn){
      var key = btn.getAttribute('data-action');
      var action = null;
      for(var i = 0; i < actions.length; i++){
        if((actions[i].key || actions[i].label) === key){ action = actions[i]; break; }
      }
      if(action && action.onClick){
        btn.addEventListener('click', function(e){
          e.stopPropagation();
          action.onClick(row.getData(), row);
        });
      }
    });

    return panel;
  }

  var userColumns = (config.columns || []).map(function(col){
    if(col.title) col.headerMenu = col.headerMenu || colMenu;
    return col;
  });

  if (actions.length > 0) userColumns.push({
    title: 'Actions', field: 'id', width: 110, hozAlign: 'center', headerHozAlign: 'center',
    headerSort: false, frozen: true,
    formatter: function(){
      return '<button class="btn-actions-toggle"><i class="fa-solid fa-chevron-down" style="font-size:10px"></i></button>';
    },
    cellClick: function(e, cell){
      var row = cell.getRow();
      var panel = row.getElement().querySelector('.row-expand-panel');
      if(!panel) return;
      var isOpen = panel.style.display === 'block';

      table.getRows().forEach(function(r){
        if(r !== row){
          var p = r.getElement().querySelector('.row-expand-panel');
          if(p && p.style.display === 'block'){
            p.style.display = 'none';
            var b = r.getElement().querySelector('.btn-actions-toggle');
            if(b){ b.classList.remove('open'); var ic=b.querySelector('i'); if(ic){ic.className='fa-solid fa-chevron-down';ic.style.fontSize='10px';} }
            r.normalizeHeight();
          }
        }
      });

      panel.style.display = isOpen ? 'none' : 'block';
      if(!isOpen) sizeExpandPanels();
      var btn  = cell.getElement().querySelector('.btn-actions-toggle');
      var icon = btn ? btn.querySelector('i') : null;
      if(icon){ icon.className = isOpen ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'; icon.style.fontSize='10px'; }
      if(btn)  btn.classList.toggle('open', !isOpen);
      row.normalizeHeight();
    }
  });

  var tabulatorOptions = {
    layout:             config.layout || 'fitColumns',
    // "pagination" only enables pagination at all (boolean); local vs remote is the separate "paginationMode" option below.
    pagination:         true,
    paginationMode:     config.remote ? 'remote' : 'local',
    paginationSize:     paginationSize,
    paginationSizeSelector: [5, 10, 15, 20],
    paginationCounter:  'rows',
    tooltip:            false,
    movableColumns:     true,
    // Disable native click-to-sort arrows on column headers; sorting is available
    // via the header (⋮) menu's Sort Ascending / Sort Descending items instead.
    // (headerSort is a per-column option in Tabulator 5.x — apply it via columnDefaults.)
    columnDefaults:     { headerSort: false },
    virtualDom:         false,
    maxHeight:          getHeight(),
    rowFormatter: function(row){
      var el = row.getElement();
      if(el.querySelector('.row-expand-panel')) return;
      el.appendChild(buildExpandPanel(row));
    },
    columns: userColumns
  };

  // ajaxParams is re-evaluated per request, called with `this` bound to the table (so it can read this.getSorters()).
  if (config.remote) {
    tabulatorOptions.ajaxURL    = config.ajaxURL;
    tabulatorOptions.ajaxParams = config.ajaxParams;
    tabulatorOptions.sortMode   = 'remote';
  } else {
    tabulatorOptions.data = config.data || [];
  }
  // Baking the filter into the Tabulator config (rather than calling setFilter
  // after construction) avoids a race on pages that need rows hidden from the
  // very first paint: a post-build setFilter can run after the caller's first
  // render if `tableBuilt` already fired synchronously (local data builds are
  // synchronous, so any handler attached after `new Tabulator(...)` returns is
  // too late to catch it), leaving every row visible until the next filter change.
  if (config.initialFilter) tabulatorOptions.initialFilter = config.initialFilter;
  if (savedState && savedState.sort && savedState.sort.length) tabulatorOptions.initialSort = savedState.sort;
  if (savedState && savedState.page) tabulatorOptions.paginationInitialPage = savedState.page;

  var table = new Tabulator(selector, tabulatorOptions);

  // Tabulator 6.x has setMaxHeight(); the 5.5.2 build several pages pin does not (it only reads
  // options.maxHeight / the element's max-height), so fall back to setting those directly.
  function applyMaxHeight(){
    var h = getHeight();
    if (typeof table.setMaxHeight === 'function') { table.setMaxHeight(h); return; }
    table.options.maxHeight = h + 'px';
    table.element.style.maxHeight = h + 'px';
    table.redraw();
  }

  table.on('tableBuilt', function(){ applyMaxHeight(); sizeExpandPanels(); });
  window.addEventListener('resize', function(){ applyMaxHeight(); sizeExpandPanels(); });

  // Tabulator known issue: rows with variableHeight columns (e.g. wrapped status text)
  // don't recalculate their height/position correctly after switching pages or page
  // size with local pagination, leaving stale gaps/overlaps until something else forces
  // a reflow. A hard redraw on these events recalculates row heights properly.
  // https://github.com/olifolkerd/tabulator/issues/4622
  table.on('pageLoaded', function(){ table.redraw(true); });
  table.on('pageSizeChanged', function(){ table.redraw(true); });

  if (stateKey) {
    table.pbSavedFilters = savedState ? savedState.filters : null;
    table.pbSaveFilters = function(filters){ PBTableState.save(stateKey, { filters: filters }); };

    table.on('dataSorted', function(sorters){
      PBTableState.save(stateKey, { sort: (sorters || []).map(function(s){ return { column: s.field, dir: s.dir }; }) });
    });
    table.on('pageLoaded', function(pageno){
      PBTableState.save(stateKey, { page: pageno });
    });
  }

  return table;
}
