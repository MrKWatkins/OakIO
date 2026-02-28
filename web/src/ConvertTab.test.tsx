import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ConvertTab } from './ConvertTab';
import type { ConvertibleFormat } from './types';

describe('ConvertTab', () => {
  it('shows empty note when there are no formats', () => {
    render(<ConvertTab formats={[]} onConvert={() => {}} converting={null} error={null} />);
    expect(screen.getByText('No conversion options available for this format.')).toBeInTheDocument();
  });

  it('renders a button for each format', () => {
    const formats: ConvertibleFormat[] = [
      { name: 'WAV Audio', extension: 'wav' },
      { name: 'TZX', extension: 'tzx' },
    ];
    render(<ConvertTab formats={formats} onConvert={() => {}} converting={null} error={null} />);
    expect(screen.getByText('WAV Audio')).toBeInTheDocument();
    expect(screen.getByText('TZX')).toBeInTheDocument();
  });

  it('disables buttons while converting', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} onConvert={() => {}} converting="wav" error={null} />);
    expect(screen.getByText('WAV Audio')).toBeDisabled();
  });

  it('shows converting message while converting', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} onConvert={() => {}} converting="wav" error={null} />);
    expect(screen.getByText('Converting…')).toBeInTheDocument();
  });

  it('shows error message when error is set', () => {
    const formats: ConvertibleFormat[] = [{ name: 'WAV Audio', extension: 'wav' }];
    render(<ConvertTab formats={formats} onConvert={() => {}} converting={null} error="Block type not supported." />);
    expect(screen.getByText('Block type not supported.')).toBeInTheDocument();
  });
});
