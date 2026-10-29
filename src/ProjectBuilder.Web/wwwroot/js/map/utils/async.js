import { QUERY_TIMEOUT_MS } from "../config.js";

export function sleep(ms) {
    return new Promise((resolve) => window.setTimeout(resolve, ms));
}

export function chunkArray(items, size) {
    const chunks = [];
    for (let i = 0; i < items.length; i += size) {
        chunks.push(items.slice(i, i + size));
    }
    return chunks;
}

export async function runWithConcurrency(items, concurrency, worker) {
    if (items.length === 0) return [];

    const results = new Array(items.length);
    let nextIndex = 0;

    async function runWorker() {
        while (nextIndex < items.length) {
            const currentIndex = nextIndex;
            nextIndex += 1;
            results[currentIndex] = await worker(items[currentIndex], currentIndex);
        }
    }

    const workers = Array.from(
        { length: Math.min(concurrency, items.length) },
        () => runWorker(),
    );
    await Promise.all(workers);
    return results;
}

export async function withTimeout(promise, label) {
    let timeoutId;
    const timeoutPromise = new Promise((_, reject) => {
        timeoutId = window.setTimeout(() => {
            reject(new Error(`${label} timed out after ${QUERY_TIMEOUT_MS / 1000}s`));
        }, QUERY_TIMEOUT_MS);
    });

    try {
        return await Promise.race([promise, timeoutPromise]);
    } finally {
        window.clearTimeout(timeoutId);
    }
}
