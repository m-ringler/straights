// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const CACHE_NAME = 'v0.6.17';
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

async function fetchFresh(url) {
    console.debug("Fetching ", url)
    result = await fetch(url, { cache: 'no-store' })
    console.debug("Response:", result.status, result.statusText)
    return result
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

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request, { ignoreSearch: true }).then(response => {
            if (response) {
                return response
            }

            // Only cache http(s) requests
            console.debug("Not found in cache, fetching from network: ", event.request.url)
            if (event.request.url.startsWith('http://') || event.request.url.startsWith('https://')) {
                return fetch(event.request).then(networkResponse => {
                    return caches.open(CACHE_NAME).then(cache => {
                        cache.put(event.request, networkResponse.clone())
                        return networkResponse;
                    });
                });
            } else {
                return fetch(event.request)
            }
        })
    )
})

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
