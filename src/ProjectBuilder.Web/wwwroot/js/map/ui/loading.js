export function setLoading(visible, message, percent) {
    const overlay = document.getElementById("map-loading");
    const messageEl = document.getElementById("map-loading-message");
    const percentEl = document.getElementById("map-loading-percent");
    const progressBar = document.getElementById("map-loading-progress-bar");

    if (!overlay) return;
    if (message && messageEl) messageEl.textContent = message;
    if (typeof percent === "number") {
        const safePercent = Math.max(0, Math.min(100, Math.round(percent)));
        if (percentEl) percentEl.textContent = `${safePercent}%`;
        if (progressBar) progressBar.style.width = `${safePercent}%`;
    }
    overlay.hidden = !visible;
}

export function createMapLoadProgress(updateLoading) {
    const FETCH_MESSAGE = "Fetching scenario data...";
    const MAP_MESSAGE = "Preparing the map...";
    const FETCH_START_PERCENT = 1;
    const FETCH_END_PERCENT = 60;
    const MAP_END_PERCENT = 100;

    let scenarioTimer = null;
    let scenarioPercent = FETCH_START_PERCENT;

    function startFetchingScenario() {
        scenarioPercent = FETCH_START_PERCENT;
        updateLoading(true, FETCH_MESSAGE, scenarioPercent);

        scenarioTimer = window.setInterval(() => {
            if (scenarioPercent < FETCH_END_PERCENT - 1) {
                scenarioPercent += 1;
                updateLoading(true, FETCH_MESSAGE, scenarioPercent);
            }
        }, 2000);
    }

    function finishFetchingScenario() {
        if (scenarioTimer) {
            window.clearInterval(scenarioTimer);
            scenarioTimer = null;
        }
        updateLoading(true, FETCH_MESSAGE, FETCH_END_PERCENT);
    }

    function reportMapPreparation(fraction) {
        const safeFraction = Math.max(0, Math.min(1, fraction));
        const percent = FETCH_END_PERCENT + Math.round(safeFraction * (MAP_END_PERCENT - FETCH_END_PERCENT));
        updateLoading(true, MAP_MESSAGE, percent);
    }

    function complete() {
        updateLoading(true, MAP_MESSAGE, MAP_END_PERCENT);
    }

    function stop() {
        if (scenarioTimer) {
            window.clearInterval(scenarioTimer);
            scenarioTimer = null;
        }
        updateLoading(false);
    }

    return {
        startFetchingScenario,
        finishFetchingScenario,
        reportMapPreparation,
        complete,
        stop,
    };
}
