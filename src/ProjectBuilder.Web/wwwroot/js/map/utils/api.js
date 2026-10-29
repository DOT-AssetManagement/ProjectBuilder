export async function getJson(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Request failed (${response.status})`);
    return response.json();
}

export async function postJson(url, body, token) {
    const response = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json", RequestVerificationToken: token },
        body: JSON.stringify(body),
    });
    if (!response.ok) throw new Error(`Request failed (${response.status})`);
    return response.json();
}
