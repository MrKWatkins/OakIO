import { describe, expect, it } from 'vitest';
import { getRuntimeUrl } from './oakio';

describe('getRuntimeUrl', () => {
  it('resolves the runtime inside the web root during Serve', () => {
    expect(getRuntimeUrl('http://localhost:5173/src/oakio.ts'))
      .toBe('http://localhost:5173/dotnet/dotnet.js');
  });

  it('resolves the runtime beside the production assets directory', () => {
    expect(getRuntimeUrl('https://example.com/assets/index.js'))
      .toBe('https://example.com/dotnet/dotnet.js');
  });

  it('preserves the nested converter deployment prefix', () => {
    expect(getRuntimeUrl('https://example.com/OakIO/converter-app/assets/index.js'))
      .toBe('https://example.com/OakIO/converter-app/dotnet/dotnet.js');
  });
});
