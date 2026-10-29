export async function fetchDssMapData(scenId, relativeEfficiencyThreshold = 0) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = { "Content-Type": "application/json" };
    if (token) {
        headers.RequestVerificationToken = token;
    }

    const response = await fetch("/api/app/dss-map/generate-map", {
        method: "POST",
        headers,
        credentials: "same-origin",
        body: JSON.stringify({ scenId, relativeEfficiencyThreshold }),
    });

    if (!response.ok) {
        let message = `Failed to load DSS scenario ${scenId}`;
        const text = await response.text();
        try {
            const errorBody = JSON.parse(text);
            message = errorBody?.error?.message ?? errorBody?.message ?? message;
        } catch {
            if (text) message = text;
        }
        throw new Error(message);
    }

    return response.json();
}
