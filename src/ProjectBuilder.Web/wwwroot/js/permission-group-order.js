// ABP's permission management modal always lists permission groups alphabetically by
// display name. Product wants Scenarios, Treatment Pool Libraries and Candidates +
// pinned right after Admin instead, so we reorder the tab list client-side
// once the modal (and its groups) have been rendered.
(function ($) {
    var GROUP_ORDER = ['Admin', 'Scenarios', 'Treatment Pool Libraries', 'Candidates +'];

    function tabGroupName($navLink) {
        var parts = $navLink.text().trim().split(' ');
        if (parts.length && /^\(\d+\)$/.test(parts[parts.length - 1])) {
            parts.pop();
        }
        return parts.join(' ').trim();
    }

    function reorderPermissionTabs($modal) {
        var $tabs = $modal.find('#PermissionsTabs');
        if (!$tabs.length) {
            return;
        }

        var $items = $tabs.children('li').toArray();
        $items.sort(function (a, b) {
            var aIndex = GROUP_ORDER.indexOf(tabGroupName($(a).find('.nav-link').first()));
            var bIndex = GROUP_ORDER.indexOf(tabGroupName($(b).find('.nav-link').first()));
            if (aIndex === -1) aIndex = GROUP_ORDER.length;
            if (bIndex === -1) bIndex = GROUP_ORDER.length;
            return aIndex - bIndex;
        });

        $.each($items, function (_, li) {
            $tabs.append(li);
        });
    }

    $(document).on('shown.bs.modal', function (e) {
        reorderPermissionTabs($(e.target));
    });
})(jQuery);
