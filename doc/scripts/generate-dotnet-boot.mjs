#!/usr/bin/env node
// Generates docs/dotnet/dotnet.boot.js - the boot manifest required by dotnet.js.

import { readdirSync, writeFileSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const dotnetDir = join(__dirname, '..', 'docs', 'dotnet');

if (!existsSync(dotnetDir)) {
    console.error(`Directory not found: ${dotnetDir}`);
    process.exit(1);
}

const files = readdirSync(dotnetDir);

const assemblies = files
    .filter(f => f.endsWith('.dll'))
    .map(name => ({ name, virtualPath: name }));

const icuFiles = files
    .filter(f => f.endsWith('.dat') && f.startsWith('icudt'))
    .map(name => ({ name, virtualPath: name }));

const config = {
    mainAssemblyName: 'MrKWatkins.OakIO.Wasm.dll',
    resources: {
        jsModuleNative: [{ name: 'dotnet.native.js' }],
        jsModuleRuntime: [{ name: 'dotnet.runtime.js' }],
        jsModuleWorker: [{ name: 'dotnet.native.worker.mjs' }],
        wasmNative: [{ name: 'dotnet.native.wasm' }],
        assembly: assemblies,
        icu: icuFiles,
    },
};

const content = `export const config = ${JSON.stringify(config, null, 2)};\n`;
writeFileSync(join(dotnetDir, 'dotnet.boot.js'), content);
console.log(`Generated dotnet.boot.js (${assemblies.length} assemblies, ${icuFiles.length} ICU files)`);
