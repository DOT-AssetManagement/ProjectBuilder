export async function fetchScenarioData(scenarioId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = { "Content-Type": "application/json" };
    if (token) {
        headers.RequestVerificationToken = token;
    }

    const response = await fetch("/api/app/map/generate-map", {
        method: "POST",
        headers,
        credentials: "same-origin",
        body: JSON.stringify({ scenId: scenarioId }),
    });

    if (!response.ok) {
        let message = `Failed to load scenario ${scenarioId}`;
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
