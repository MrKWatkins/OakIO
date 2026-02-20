import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import type { Plugin } from 'vite'
import { readFileSync } from 'node:fs'

// Vite plugin for .NET WASM runtime files.
// dotnet.js uses /*! webpackIgnore: true */ on its internal dynamic imports.
// Replacing this with /* @vite-ignore */ suppresses Vite's "cannot be analyzed"
// warning without affecting runtime behaviour.
function dotnetWasmPlugin(): Plugin {
  return {
    name: 'dotnet-wasm',
    load(id) {
      if (id.endsWith('/dotnet/dotnet.js')) {
        const code = readFileSync(id, 'utf-8')
        return code.replace(/import\(\/\*! webpackIgnore: true \*\//g, 'import(/* @vite-ignore */')
      }
    },
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), dotnetWasmPlugin()],
  server: {
    headers: {
      'Cross-Origin-Embedder-Policy': 'require-corp',
      'Cross-Origin-Opener-Policy': 'same-origin',
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test-setup.ts',
  },
})
