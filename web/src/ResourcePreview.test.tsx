import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ResourcePreview } from './ResourcePreview';
import type { FileInfo } from './types';

const info: FileInfo = { format: 'Test', fileExtension: 'bmp', type: 'image', convertibleTo: [], sections: [] };

describe('ResourcePreview', () => {
  it('renders palette colours in index order with alpha', () => {
    render(<ResourcePreview info={{ ...info, palette: [
      { red: 255, green: 0, blue: 0, alpha: 255 },
      { red: 0, green: 255, blue: 0, alpha: 128 },
    ] }} />);
    expect(screen.getByText('Palette (2 colours)')).toBeInTheDocument();
    expect(screen.getAllByRole('img').map(element => element.getAttribute('aria-label')))
      .toEqual(['Index 0: #FF0000FF', 'Index 1: #00FF0080']);
  });

  it('renders RGBA pixels and zooms without changing the pixel buffer', () => {
    const data = { data: new Uint8ClampedArray(8) };
    const putImageData = vi.fn();
    const getContext = vi.spyOn(HTMLCanvasElement.prototype, 'getContext')
      .mockReturnValue({ createImageData: vi.fn(() => data), putImageData } as unknown as CanvasRenderingContext2D);
    render(<ResourcePreview info={{ ...info, image: { width: 2, height: 1, pixels: btoa('\xff\x00\x00\xff\x00\xff\x00\xff') } }} />);
    const canvas = screen.getByLabelText('2 × 1 image preview');
    expect([...data.data]).toEqual([255, 0, 0, 255, 0, 255, 0, 255]);
    expect(putImageData).toHaveBeenCalledWith(data, 0, 0);
    fireEvent.change(screen.getByLabelText('Zoom'), { target: { value: '4' } });
    expect(canvas).toHaveStyle({ width: '8px', height: '4px', imageRendering: 'pixelated' });
    expect(canvas).toHaveAttribute('width', '2');
    expect(putImageData).toHaveBeenCalledTimes(1);
    getContext.mockRestore();
  });

  it('does not render a preview for non-resource files', () => {
    const { container } = render(<ResourcePreview info={info} />);
    expect(container.querySelector('canvas')).toBeNull();
    expect(screen.queryByRole('heading')).toBeNull();
  });

  it('handles an unavailable canvas context', () => {
    const getContext = vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(null);
    render(<ResourcePreview info={{ ...info, image: { width: 1, height: 1, pixels: btoa('\x00\x00\x00\xff') } }} />);
    expect(screen.getByLabelText('1 × 1 image preview')).toHaveAttribute('height', '1');
    getContext.mockRestore();
  });
});
