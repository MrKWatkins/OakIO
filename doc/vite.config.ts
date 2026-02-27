import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
    plugins: [react()],
    build: {
        outDir: resolve(__dirname, 'docs/assets/javascripts'),
        emptyOutDir: true,
        sourcemap: false,
        rollupOptions: {
            input: resolve(__dirname, 'src/converter-entry.tsx'),
            output: {
                entryFileNames: 'converter.js',
                chunkFileNames: 'chunks/[name].js',
                assetFileNames: assetInfo =>
                    assetInfo.names?.some(n => n.endsWith('.css')) ? 'converter.css' : '[name][extname]',
            },
        },
    },
});
