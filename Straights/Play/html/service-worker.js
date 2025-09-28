// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const CACHE_NAME = 'v0.6.18';
const urlsToCache = [
    './',
    './favicon.ico',
    './game.js',
    './gameHistory.js',
    './generate-str8ts.js',
    './generate-worker.js',
    './index.html',
    './jquery-3.7.1.min.js',
    './LICENSE',
    './str8ts.css',
    './str8ts.js',
    './Straights.Web.js',
    './Straights.Web.wasm',
    './undoStack.js',
    './favicon/android-chrome-192x192.png',
    './favicon/android-chrome-512x512.png',
    './favicon/favicon-16x16.png',
    './favicon/favicon-32x32.png',
    './favicon/favicon-48x48.png',
    './favicon/favicon-64x64.png',
    './favicon/favicon-128x128.png',
    './favicon/favicon-256x256.png',
    './favicon/maskable_icon_x192.png',
    './favicon/maskable_icon_x512.png',
    './site.webmanifest',
    'https://fonts.googleapis.com/css2?family=Nunito:wght@400;600&display=swap',
    'https://fonts.gstatic.com/s/nunito/v31/XRXV3I6Li01BKofINeaB.woff2',
    'https://fonts.gstatic.com/s/nunito/v31/XRXV3I6Li01BKofINeaBTMnFcQ.woff2'
];

const apiEndPoints = new Set([
    '/generate',
    '/hint'
])

async function fetchFresh(url) {
    console.debug("Fetching ", url)
    result = await fetch(url, { cache: 'no-store' })
    console.debug("Response:", result.status, result.statusText)
    return result
}

async function _fetch(request) {
    const requestUrl = new URL(request.url);

    if (apiEndPoints.has(requestUrl.pathname)) {
        // Always fetch from network for local API endpoints
        return await fetch(request);
    }

    // Try to match the request in the cache
    const cachedResponse = await caches.match(
        request,
        { ignoreSearch: true })
    if (cachedResponse) {
        // Return cached response if found
        return cachedResponse
    }

    // HTTP(S) requests: fetch and cache
    console.debug("Not found in cache, fetching from network: ", request.url);
    if (request.url.startsWith('http://') || request.url.startsWith('https://')) {
        const networkResponse = await fetch(request)
        const cache = await caches.open(CACHE_NAME)
        await cache.put(request, networkResponse.clone())
        return networkResponse
    }

    // Non-HTTP(S) requests: just fetch
    return await fetch(request)
}

self.addEventListener('install', event => {
    console.info("Installing service worker", CACHE_NAME)
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return Promise.all(
                urlsToCache.map(url =>
                    fetchFresh(url)
                        .then(response => cache.put(url, response))
                        .catch(error => console.warn(`Failed to fetch ${url}:`, error))
                )
            );
        })
    );
});

self.addEventListener('fetch', event => event.respondWith(_fetch(event.request)));

self.addEventListener('activate', event => {
    const cacheWhitelist = [CACHE_NAME]
    console.info("Activating service worker", CACHE_NAME)

    event.waitUntil((async () => {
        const cacheNames = await caches.keys();
        await Promise.all(
            cacheNames.map(cacheName => {
                if (!cacheWhitelist.includes(cacheName)) {
                    console.info("Deleting old cache", cacheName)
                    return caches.delete(cacheName)
                }
            })
        )
        await self.clients.claim()
        const clients = await self.clients.matchAll()
        clients.forEach(client => client.postMessage('reload'))
    })())
})
