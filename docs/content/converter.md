---
sidebar: false
---

# Converter

Load a ZX Spectrum file, palette or image to inspect its contents and convert it between supported formats. All inspection and conversion is done in your browser; files are not uploaded.

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue';
import { withBase } from 'vitepress';

const ready = ref(false);
const error = ref('');
let resizeObserver;

function fitConverter(event) {
  resizeObserver?.disconnect();
  const frame = event.target;
  const body = frame.contentDocument.body;
  resizeObserver = new ResizeObserver(() => {
    frame.style.height = `${Math.ceil(body.getBoundingClientRect().height)}px`;
  });
  resizeObserver.observe(body);
}

onBeforeUnmount(() => resizeObserver?.disconnect());

onMounted(async () => {
  if (window.crossOriginIsolated) {
    ready.value = true;
    return;
  }
  try {
    if (!window.isSecureContext || !('serviceWorker' in navigator)) {
      throw new Error('The converter requires HTTPS or localhost and a browser with service worker support.');
    }
    await navigator.serviceWorker.register(withBase('/coi-serviceworker.js'));
    await navigator.serviceWorker.ready;
    window.location.reload();
  } catch (cause) {
    error.value = cause.message;
  }
});
</script>

<p v-if="error" role="alert">{{ error }}</p>
<p v-else-if="!ready">Loading converter…</p>
<iframe v-else :src="withBase('/converter-app/index.html?embedded')" title="OakIO file converter"
  @load="fitConverter" allow="cross-origin-isolated" style="width: 100%; border: 0; display: block;"></iframe>
