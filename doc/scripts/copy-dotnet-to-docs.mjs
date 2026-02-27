#!/usr/bin/env node
// Copies the WASM publish output into docs/dotnet/, keeping only runtime assets.

import { readdirSync, copyFileSync, mkdirSync, rmSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const srcDir = '/tmp/oakio-wasm';
const destDir = join(__dirname, '..', 'docs', 'dotnet');

if (!existsSync(srcDir)) {
    console.error(`Publish output not found: ${srcDir}`);
    process.exit(1);
}

if (existsSync(destDir)) {
    rmSync(destDir, { recursive: true });
}
mkdirSync(destDir, { recursive: true });

const includedExtensions = new Set(['.js', '.wasm', '.dll', '.dat', '.json', '.mjs']);
const files = readdirSync(srcDir).filter(f => {
    const dot = f.lastIndexOf('.');
    return dot !== -1 && includedExtensions.has(f.slice(dot)) && !f.endsWith('.map');
});

for (const file of files) {
    copyFileSync(join(srcDir, file), join(destDir, file));
}

console.log(`Copied ${files.length} dotnet assets to docs/dotnet/`);
