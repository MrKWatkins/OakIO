/* coi-serviceworker — enables cross-origin isolation via a service worker.
 * Required for SharedArrayBuffer used by the .NET WASM threaded runtime.
 * When loaded as a <script> in a page, it registers itself as a service worker
 * and reloads the page so the SW can intercept requests and add COOP/COEP headers.
 */
"use strict";

(() => {
  // ---- Service worker context ----
  if (typeof window === 'undefined') {
    self.addEventListener('install', () => self.skipWaiting());
    self.addEventListener('activate', e => e.waitUntil(self.clients.claim()));
    self.addEventListener('fetch', e => {
      // Guard against empty or unparseable URLs (e.g. from src="" attributes).
      let url;
      try {
        url = new URL(e.request.url);
      } catch {
        return;
      }

      // Only patch same-origin requests. Cross-origin requests (fonts, badges,
      // CDN assets) are passed through unchanged so they continue to load normally
      // under COEP: credentialless.
      if (url.origin !== self.location.origin) return;

      // Skip cache-only requests that would fail a real fetch.
      if (e.request.cache === 'only-if-cached' && e.request.mode !== 'same-origin') return;

      // On localhost, bypass HTTP cache for sub-resources so dev rebuilds are
      // always visible. Navigate requests can't be reconstructed this way.
      const isLocal = url.hostname === 'localhost' || url.hostname === '127.0.0.1';
      const fetchRequest = (isLocal && e.request.mode !== 'navigate')
        ? new Request(e.request, { cache: 'no-cache' })
        : e.request;

      e.respondWith(
        fetch(fetchRequest)
          .then(response => {
            const headers = new Headers(response.headers);
            // credentialless allows cross-origin subresources that lack CORP headers
            // while still achieving crossOriginIsolated (required for SharedArrayBuffer).
            headers.set('Cross-Origin-Embedder-Policy', 'credentialless');
            headers.set('Cross-Origin-Opener-Policy', 'same-origin');
            return new Response(response.body, {
              status: response.status,
              statusText: response.statusText,
              headers,
            });
          })
          .catch(err => {
            console.error('coi-serviceworker fetch error:', err);
            return new Response('Service worker fetch failed', { status: 503 });
          })
      );
    });
    return;
  }

  // ---- Page context ----
  if (window.crossOriginIsolated) return;

  if (!window.isSecureContext) {
    console.log('coi-serviceworker: requires a secure context (HTTPS or localhost).');
    return;
  }

  if (!('serviceWorker' in navigator)) {
    console.log('coi-serviceworker: service workers not supported.');
    return;
  }

  // Use the script's own URL so the scope is always correct, regardless of
  // whether the site is at / (local) or /OakIO/ (GitHub Pages).
  const swUrl = document.currentScript?.src ?? new URL('coi-serviceworker.js', location.href).href;

  navigator.serviceWorker.register(swUrl).then(registration => {
    // If a controller is already active the page was loaded under the SW —
    // nothing to do (the headers are already set).
    if (navigator.serviceWorker.controller) return;

    const reload = () => window.location.reload();

    if (registration.active) {
      // SW is active but not yet controlling this page: reload to let it take over.
      reload();
      return;
    }

    // Wait for the SW to become active then reload.
    const pending = registration.installing ?? registration.waiting;
    if (pending) {
      pending.addEventListener('statechange', function () {
        if (this.state === 'activated') reload();
      });
    }

    navigator.serviceWorker.addEventListener('controllerchange', reload);
  }).catch(err => console.error('coi-serviceworker registration failed:', err));
})();
