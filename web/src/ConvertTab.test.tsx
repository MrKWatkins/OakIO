import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ConvertTab } from './ConvertTab';
import type { ConvertibleFormat } from './types';

describe('ConvertTab', () => {
  it('passes the extension when a labelled conversion button is clicked', () => {
    const onConvert = vi.fn();
    render(<ConvertTab formats={[{ name: 'ZX Spectrum Screen', extension: 'scr' }]}
      compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={onConvert} converting={null} error={null} />);
    fireEvent.click(screen.getByRole('button', { name: 'ZX Spectrum Screen (.scr)' }));
    expect(onConvert).toHaveBeenCalledWith('scr');
  });

  it('shows empty note when there are no formats', () => {
    render(<ConvertTab formats={[]} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting={null} error={null} />);
    expect(screen.getByText('No conversion options available for this format.')).toBeInTheDocument();
  });

  it('renders a button for each format', () => {
    const formats: ConvertibleFormat[] = [
      { name: 'WAV Audio', extension: 'wav' },
      { name: 'TZX', extension: 'tzx' },
    ];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting={null} error={null} />);
    expect(screen.getByRole('button', { name: 'WAV Audio (.wav)' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'TZX (.tzx)' })).toBeInTheDocument();
  });

  it('disables buttons while converting', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting="wav" error={null} />);
    expect(screen.getByRole('button', { name: 'WAV Audio (.wav)' })).toBeDisabled();
  });

  it('shows converting message while converting', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting="wav" error={null} />);
    expect(screen.getByText('Converting…')).toBeInTheDocument();
  });

  it('shows error message when error is set', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting={null} error="Block type not supported." />);
    expect(screen.getByText('Block type not supported.')).toBeInTheDocument();
  });

  it('shows a compression dropdown defaulting to None', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting={null} error={null} />);
    expect(screen.getByLabelText('Compression:')).toHaveValue('None');
  });

  it('offers None, Zip and GZip but not Brotli or Zstandard', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting={null} error={null} />);
    const options = screen.getAllByRole('option').map(o => o.textContent);
    expect(options).toEqual(['None', 'Zip', 'GZip']);
  });

  it('calls onCompressionFormatChange when a different format is chosen', () => {
    const onCompressionFormatChange = vi.fn();
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={onCompressionFormatChange} onConvert={() => {}} converting={null} error={null} />);

    fireEvent.change(screen.getByLabelText('Compression:'), { target: { value: 'GZip' } });

    expect(onCompressionFormatChange).toHaveBeenCalledWith('GZip');
  });

  it('disables the compression dropdown while converting', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} compressionFormat="None" onCompressionFormatChange={() => {}} onConvert={() => {}} converting="wav" error={null} />);
    expect(screen.getByLabelText('Compression:')).toBeDisabled();
  });
});
