import { fireEvent, render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import App from './App';
import { getInfo, convert, getCompressedFilename } from './oakio';

vi.mock('./oakio', () => ({ getInfo: vi.fn(), convert: vi.fn(), getCompressedFilename: vi.fn() }));

describe('App', () => {
  it('displays conversion errors without the parameter diagnostic', async () => {
    vi.mocked(getInfo).mockResolvedValueOnce({
      format: 'Windows Bitmap', fileExtension: 'bmp', type: 'image',
      convertibleTo: [{ name: 'ZX Spectrum Screen', extension: 'scr' }], sections: [],
    });
    vi.mocked(convert).mockRejectedValueOnce(new Error('An SCR image must be 256 by 192 pixels. Arg_ParamName_Name, image'));
    vi.mocked(getCompressedFilename).mockResolvedValueOnce('mona.scr');
    render(<App />);
    const file = new File(['bitmap'], 'mona.bmp');
    Object.defineProperty(file, 'arrayBuffer', { value: async () => new ArrayBuffer(1) });
    fireEvent.change(screen.getByLabelText('File:'), { target: { files: [file] } });
    await screen.findByText('mona.bmp');
    fireEvent.click(screen.getByRole('button', { name: 'Convert' }));
    fireEvent.click(screen.getByRole('button', { name: 'ZX Spectrum Screen (.scr)' }));
    expect(await screen.findByText('An SCR image must be 256 by 192 pixels.')).toBeInTheDocument();
    expect(screen.queryByText(/Arg_ParamName_Name/)).not.toBeInTheDocument();
  });

  it('downloads a conversion using an asynchronous filename result', async () => {
    vi.mocked(getInfo).mockResolvedValueOnce({
      format: 'Windows Bitmap', fileExtension: 'bmp', type: 'image',
      convertibleTo: [{ name: 'ZX Spectrum Next Image', extension: 'nxi' }], sections: [],
    });
    vi.mocked(convert).mockResolvedValueOnce(new Uint8Array([1, 2, 3]));
    vi.mocked(getCompressedFilename).mockResolvedValueOnce('screen.nxi');
    const createObjectURL = vi.fn(() => 'blob:converted');
    const revokeObjectURL = vi.fn();
    const NativeURL = URL;
    vi.stubGlobal('URL', class extends NativeURL {
      static createObjectURL = createObjectURL;
      static revokeObjectURL = revokeObjectURL;
    });
    const downloads: { filename: string; href: string }[] = [];
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function (this: HTMLAnchorElement) {
      downloads.push({ filename: this.download, href: this.href });
    });
    try {
      render(<App />);
      const file = new File(['bitmap'], 'screen.bmp');
      Object.defineProperty(file, 'arrayBuffer', { value: async () => new ArrayBuffer(1) });
      fireEvent.change(screen.getByLabelText('File:'), { target: { files: [file] } });
      await screen.findByText('screen.bmp');
      fireEvent.click(screen.getByRole('button', { name: 'Convert' }));
      fireEvent.click(screen.getByRole('button', { name: 'ZX Spectrum Next Image (.nxi)' }));
      await vi.waitFor(() => expect(downloads).toEqual([{ filename: 'screen.nxi', href: 'blob:converted' }]));
      expect(convert).toHaveBeenCalledWith('screen.bmp', new Uint8Array(1), 'screen.nxi', 'None');
      expect(getCompressedFilename).toHaveBeenCalledWith('screen.nxi', 'None');
      expect(revokeObjectURL).toHaveBeenCalledWith('blob:converted');
    } finally {
      click.mockRestore();
      vi.unstubAllGlobals();
    }
  });

  it('loads a palette and displays its metadata and colours', async () => {
    vi.mocked(getInfo).mockResolvedValueOnce({
      format: 'JASC palette', fileExtension: 'pal', type: 'palette', convertibleTo: [], sections: [],
      palette: [{ red: 255, green: 0, blue: 0, alpha: 255 }],
    });
    render(<App />);
    const file = new File(['palette'], 'sample.pal');
    Object.defineProperty(file, 'arrayBuffer', { value: async () => new ArrayBuffer(1) });
    fireEvent.change(screen.getByLabelText('File:'), { target: { files: [file] } });
    expect(await screen.findByText('sample.pal')).toBeInTheDocument();
    expect(screen.queryByText('Palette (1 colours)')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Contents' }));
    expect(screen.getByText('Palette (1 colours)')).toBeInTheDocument();
    expect(screen.getByRole('img', { name: 'Index 0: #FF0000FF' })).toBeInTheDocument();
    expect(getInfo).toHaveBeenCalledWith('sample.pal', new Uint8Array(1));
    expect(screen.queryByText('No content sections available.')).not.toBeInTheDocument();
  });

  it('renders the OakIO heading', () => {
    render(<App />);
    expect(screen.getByText('OakIO')).toBeInTheDocument();
  });

  it('renders the subtitle', () => {
    render(<App />);
    expect(screen.getByText('ZX Spectrum file tools')).toBeInTheDocument();
  });

  it('renders the file picker label', () => {
    render(<App />);
    expect(screen.getByText('File:')).toBeInTheDocument();
  });

  it('has a file input accepting ZX Spectrum formats', () => {
    render(<App />);
    const input = document.querySelector('input[type="file"]');
    expect(input).toBeInTheDocument();
    expect(input?.getAttribute('accept')).toBe('.tap,.tzx,.pzx,.z80,.sna,.nex,.rzx,.gpl,.pal,.txt,.nxp,.bmp,.scr,.nxi,.zip,.gz');
  });

  it('shows tabs even before a file is loaded', () => {
    render(<App />);
    expect(screen.getByText('File')).toBeInTheDocument();
    expect(screen.getByText('Contents')).toBeInTheDocument();
    expect(screen.getByText('Convert')).toBeInTheDocument();
  });

  it('shows placeholder text before a file is loaded', () => {
    render(<App />);
    expect(screen.getByText('Load a file to get started.')).toBeInTheDocument();
  });
});
