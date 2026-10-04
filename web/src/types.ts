export interface FileInfo {
  format: string;
  fileExtension: string;
  type: string;
  convertibleTo: ConvertibleFormat[];
  sections: Section[];
  palette?: Colour[];
  image?: { width: number; height: number; pixels: string };
}

export interface Colour {
  red: number;
  green: number;
  blue: number;
  alpha: number;
}

export interface ConvertibleFormat {
  name: string;
  extension: string;
}

// Zstandard and Brotli are deliberately omitted: neither works under WASM (Zstandard needs unmanaged
// code via ZstdSharp; Brotli relies on a native library that isn't available in the browser runtime).
export const compressionFormats = ['None', 'Zip', 'GZip'] as const;

export type CompressionFormat = (typeof compressionFormats)[number];

export interface Section {
  title: string;
  category: string;
  properties?: Property[];
  items?: Item[];
}

export interface Property {
  name: string;
  value: string;
  format?: string;
}

export interface Item {
  title: string;
  properties?: Property[];
  details?: Record<string, string>;
  sections?: Section[];
}
