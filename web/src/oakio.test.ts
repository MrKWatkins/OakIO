import { describe, expect, it } from 'vitest';
import { getRuntimeUrl } from './oakio';

describe('getRuntimeUrl', () => {
  it('resolves the runtime inside the web root during Serve', () => {
    expect(getRuntimeUrl('http://localhost:5173/src/oakio.ts', true))
      .toBe('http://localhost:5173/dotnet/dotnet.js');
  });

  it('resolves the runtime beside the production assets directory', () => {
    expect(getRuntimeUrl('https://example.com/assets/index.js', false))
      .toBe('https://example.com/dotnet/dotnet.js');
  });

  it('preserves the deployment prefix for the documentation website', () => {
    expect(getRuntimeUrl('https://example.com/OakIO/assets/javascripts/converter.js', false))
      .toBe('https://example.com/OakIO/dotnet/dotnet.js');
  });
});
