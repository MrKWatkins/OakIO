#!/usr/bin/env node
// Generates public/dotnet/dotnet.boot.js - the boot manifest required by dotnet.js.
// The manifest tells the .NET runtime which assemblies and assets to load.
// This script is run automatically as part of the dotnet:publish npm script.

import { readdirSync, writeFileSync, existsSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const outputDir = join(__dirname, '..', 'dotnet');

if (!existsSync(outputDir)) {
    console.error(`Directory not found: ${outputDir}`);
    process.exit(1);
}

const files = readdirSync(outputDir);

const assemblies = files
    .filter(f => f.endsWith('.dll'))
    .map(name => ({ name, virtualPath: name }));

const icuFiles = files
    .filter(f => f.endsWith('.dat') && f.startsWith('icudt'))
    .map(name => ({ name, virtualPath: name }));

// dotnet.boot.js is imported by dotnet.js as:
//   r = (await import(url)).config
// So 'config' must be a named export on the module namespace, not wrapped in 'default'.
// BootModule type in dotnet.d.ts: { config: MonoConfig } (the namespace object itself).
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
writeFileSync(join(outputDir, 'dotnet.boot.js'), content);
console.log(`Generated dotnet.boot.js (${assemblies.length} assemblies, ${icuFiles.length} ICU files)`);
