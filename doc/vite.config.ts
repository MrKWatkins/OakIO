import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
    plugins: [react()],
    resolve: {
        // Force all React imports (including those from web/src/ files outside this
        // root) to resolve to a single copy, preventing "duplicate React" errors.
        alias: {
            'react': resolve(__dirname, 'node_modules/react'),
            'react-dom': resolve(__dirname, 'node_modules/react-dom'),
            'react/jsx-runtime': resolve(__dirname, 'node_modules/react/jsx-runtime'),
            'react-dom/client': resolve(__dirname, 'node_modules/react-dom/client'),
        },
    },
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
