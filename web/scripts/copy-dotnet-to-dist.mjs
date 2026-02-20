#!/usr/bin/env node
// Copies the required dotnet runtime files into dist/dotnet/ after the Vite
// production build. Because dotnet.js is loaded via a vite-ignored dynamic
// import, Vite does not bundle it or copy it automatically.

import { readdirSync, copyFileSync, mkdirSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const dotnetDir = join(__dirname, '..', 'dotnet');
const distDotnetDir = join(__dirname, '..', 'dist', 'dotnet');

if (!existsSync(distDotnetDir)) {
    mkdirSync(distDotnetDir, { recursive: true });
}

const includedExtensions = new Set(['.js', '.wasm', '.dll', '.dat', '.json']);
const files = readdirSync(dotnetDir).filter(f => {
    const dot = f.lastIndexOf('.');
    return dot !== -1 && includedExtensions.has(f.slice(dot)) && !f.endsWith('.map');
});

for (const file of files) {
    copyFileSync(join(dotnetDir, file), join(distDotnetDir, file));
}

console.log(`Copied ${files.length} dotnet assets to dist/dotnet/`);
