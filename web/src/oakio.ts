import type * as DotnetModule from '../dotnet/dotnet.js';
import type { FileInfo } from './types';

interface OakIOExports {
  MrKWatkins: {
    OakIO: {
      Wasm: {
        OakIOInterop: {
          GetInfo: (inputFilename: string, inputData: Uint8Array) => Promise<string>;
          Convert: (inputFilename: string, inputData: Uint8Array, outputFilename: string) => Promise<string>;
        };
      };
    };
  };
}

let exportsPromise: Promise<OakIOExports> | null = null;

async function initRuntime(): Promise<OakIOExports> {
  const dotnetUrl = '/dotnet/dotnet.js';
  const { dotnet } = await import(/* @vite-ignore */ dotnetUrl) as typeof DotnetModule;
  const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .withConfig({ globalizationMode: 'invariant' })
    .create();

  const config = getConfig();
  const exports = await getAssemblyExports(config.mainAssemblyName!);
  return exports as OakIOExports;
}

function getExports(): Promise<OakIOExports> {
  if (!exportsPromise) {
    exportsPromise = initRuntime().catch(err => {
      exportsPromise = null;
      throw err;
    });
  }
  return exportsPromise;
}

export async function getInfo(inputFilename: string, inputData: Uint8Array): Promise<FileInfo> {
  const exports = await getExports();
  const json = await exports.MrKWatkins.OakIO.Wasm.OakIOInterop.GetInfo(inputFilename, inputData);
  return JSON.parse(json) as FileInfo;
}

export async function convert(inputFilename: string, inputData: Uint8Array, outputFilename: string): Promise<Uint8Array> {
  const exports = await getExports();
  const base64 = await exports.MrKWatkins.OakIO.Wasm.OakIOInterop.Convert(inputFilename, inputData, outputFilename);
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}
