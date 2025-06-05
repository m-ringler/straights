const CACHE_NAME = 'v0.6.12';
const urlsToCache = [
    './',
    './index.html',
    './LICENSE',
    './str8ts.css',
    './jquery-3.7.1.min.js',
    './generate-str8ts.js',
    './generate-worker.js',
    './str8ts.js',
    './Straights.Web.js',
    './Straights.Web.wasm',
    './favicon.ico',
    './favicon/favicon-16x16.png',
    './favicon/favicon-32x32.png',
    './favicon/favicon-48x48.png',
    './favicon/favicon-64x64.png',
    './favicon/favicon-128x128.png',
    './favicon/favicon-256x256.png',
    './favicon/android-chrome-192x192.png',
    './site.webmanifest',
    'https://fonts.googleapis.com/css2?family=Nunito:wght@400;600&display=swap'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return cache.addAll(urlsToCache);
        })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            if (response) {
                return response;
            }

            return fetch(event.request).then(networkResponse => {
                return caches.open(CACHE_NAME).then(cache => {
                    cache.put(event.request, networkResponse.clone());
                    return networkResponse;
                });
            });
        })
    );
});

self.addEventListener('activate', event => {
    const cacheWhitelist = [CACHE_NAME];
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (!cacheWhitelist.includes(cacheName)) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});
