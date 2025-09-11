// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const CACHE_NAME = 'v0.6.15-final';
const urlsToCache = [
  './',
  './favicon.ico',
  './game.js',
  './gameHistory.js',
  './generate-str8ts.js',
  './generate-worker.js',
  './index.html',
  './jquery-3.7.1.min.js',
  './service-worker.js',
  './LICENSE',
  './str8ts.css',
  './str8ts.js',
  './Straights.Web.js',
  './Straights.Web.wasm',
  './undoStack.js',
  './favicon/favicon-16x16.png',
  './favicon/favicon-32x32.png',
  './favicon/favicon-48x48.png',
  './favicon/favicon-64x64.png',
  './favicon/favicon-128x128.png',
  './favicon/favicon-256x256.png',
  './favicon/maskable_icon_x192.png',
  './favicon/maskable_icon_x512.png',
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

      // Only cache http(s) requests
      if (event.request.url.startsWith('http://') || event.request.url.startsWith('https://')) {
        return fetch(event.request).then(networkResponse => {
          return caches.open(CACHE_NAME).then(cache => {
            cache.put(event.request, networkResponse.clone());
            return networkResponse;
          });
        });
      } else {
        return fetch(event.request);
      }
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
