export const zxColours: Record<string, string> = {
  'Black': '#000000',
  'Blue': '#0000D7',
  'Red': '#D70000',
  'Magenta': '#D700D7',
  'Green': '#00D700',
  'Cyan': '#00D7D7',
  'Yellow': '#D7D700',
  'White': '#D7D7D7',
};

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} bytes`;
  }
  const kb = bytes / 1024;
  if (kb < 1024) {
    return `${kb.toFixed(1)} KB`;
  }
  const mb = kb / 1024;
  return `${mb.toFixed(1)} MB`;
}
