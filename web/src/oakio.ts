import { dotnet } from '../dotnet/dotnet.js';

interface OakIOExports {
  MrKWatkins: {
    OakIO: {
      Wasm: {
        OakIOInterop: {
          GetInfo: (inputFilename: string, inputData: Uint8Array) => string;
          Convert: (inputFilename: string, inputData: Uint8Array, outputFilename: string) => Uint8Array;
        };
      };
    };
  };
}

let exportsPromise: Promise<OakIOExports> | null = null;

async function initRuntime(): Promise<OakIOExports> {
  const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

  const config = getConfig();
  const exports = await getAssemblyExports(config.mainAssemblyName!);
  return exports as OakIOExports;
}

function getExports(): Promise<OakIOExports> {
  if (!exportsPromise) {
    exportsPromise = initRuntime();
  }
  return exportsPromise;
}

export async function getInfo(inputFilename: string, inputData: Uint8Array): Promise<string> {
  const exports = await getExports();
  return exports.MrKWatkins.OakIO.Wasm.OakIOInterop.GetInfo(inputFilename, inputData);
}

export async function convert(inputFilename: string, inputData: Uint8Array, outputFilename: string): Promise<Uint8Array> {
  const exports = await getExports();
  return exports.MrKWatkins.OakIO.Wasm.OakIOInterop.Convert(inputFilename, inputData, outputFilename);
}
