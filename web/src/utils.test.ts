import { describe, expect, it } from 'vitest';
import { formatErrorMessage } from './utils';

describe('formatErrorMessage', () => {
  it.each([
    'An SCR image must be 256 by 192 pixels. Arg_ParamName_Name, image',
    'An SCR image must be 256 by 192 pixels.\nArg_ParamName_Name, image',
    "An SCR image must be 256 by 192 pixels. (Parameter 'image')",
  ])('removes the parameter diagnostic from %s', message => {
    expect(formatErrorMessage(new Error(message))).toBe('An SCR image must be 256 by 192 pixels.');
  });

  it('preserves other error details', () => {
    const message = 'Invalid image, dimensions 256×384.\nExpected 256×192.';
    expect(formatErrorMessage(new Error(message))).toBe(message);
  });

  it('preserves non-error values', () => {
    expect(formatErrorMessage('Unable to convert.')).toBe('Unable to convert.');
    expect(formatErrorMessage(null)).toBe('null');
  });
});
