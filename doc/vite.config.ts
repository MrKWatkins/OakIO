import { defineConfig, createLogger } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

// dotnet.js is loaded via new URL('../../dotnet/dotnet.js', import.meta.url) in
// src/oakio.ts, resolved at runtime after "npm run dotnet:publish" generates it.
// Vite statically detects that pattern and warns that the file doesn't exist at
// build time, even though leaving it unresolved for runtime is exactly what we
// want. @vite-ignore doesn't suppress it (tested against vite 8.1.3 / rolldown -
// the comment gets normalized away before the warning check runs, regardless of
// where it's placed), so filter the one known-benign message here instead.
const logger = createLogger();
const { warnOnce } = logger;
logger.warnOnce = (message, options) => {
    if (message.includes("dotnet/dotnet.js")) {
        return;
    }
    warnOnce.call(logger, message, options);
};

export default defineConfig({
    customLogger: logger,
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
        // false, not true: this directory only ever holds this build's own fixed-named output
        // (converter.css/converter.js), so there's nothing stale to clean up between rebuilds.
        // Emptying it first (the default) means every rebuild briefly deletes both files before
        // recreating them; if mkdocs' one-shot startup file scan lands in that window (a real
        // race when "vite build --watch" and "mkdocs serve" start concurrently), the files are
        // permanently missing from that mkdocs session with no later rescan to recover them.
        emptyOutDir: false,
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
