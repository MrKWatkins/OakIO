import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ContentsTab } from './ContentsTab';
import type { FileInfo } from './types';

const baseInfo: FileInfo = {
  format: 'TAP',
  fileExtension: 'tap',
  type: 'Tape',
  convertibleTo: [],
  sections: [],
};

describe('ContentsTab', () => {
  it('shows empty note when there are no content sections', () => {
    render(<ContentsTab info={baseInfo} />);
    expect(screen.getByText('No content sections available.')).toBeInTheDocument();
  });

  it('hides the Blocks section title for tapes', () => {
    const info: FileInfo = {
      ...baseInfo,
      sections: [{ title: 'Blocks', category: 'content', items: [{ title: 'Data Block' }] }],
    };
    render(<ContentsTab info={info} />);
    expect(screen.queryByText('Blocks')).not.toBeInTheDocument();
    expect(screen.getByText('Data Block')).toBeInTheDocument();
  });

  it('shows titles for sections with items (non-Blocks)', () => {
    const info: FileInfo = {
      ...baseInfo,
      sections: [{ title: 'Program', category: 'content', items: [{ title: 'Header' }] }],
    };
    render(<ContentsTab info={info} />);
    expect(screen.getByText('Program')).toBeInTheDocument();
  });

  it('renders snapshot sections as grid without headers', () => {
    const info: FileInfo = {
      ...baseInfo,
      sections: [
        { title: 'Hardware', category: 'content', properties: [{ name: 'Border', value: 'Blue' }] },
        { title: 'Registers', category: 'content', properties: [{ name: 'AF', value: '0x1234' }] },
        { title: 'Shadow Registers', category: 'content', properties: [{ name: "AF'", value: '0x5678' }] },
      ],
    };
    render(<ContentsTab info={info} />);
    // No section titles rendered
    expect(screen.queryByText('Registers')).not.toBeInTheDocument();
    expect(screen.queryByText('Shadow Registers')).not.toBeInTheDocument();
    expect(screen.queryByText('Hardware')).not.toBeInTheDocument();
    // Properties are rendered
    expect(screen.getByText('AF')).toBeInTheDocument();
    expect(screen.getByText("AF'")).toBeInTheDocument();
    expect(screen.getByText('Border')).toBeInTheDocument();
  });

  it('renders Registers before Shadow Registers in snapshot layout', () => {
    const info: FileInfo = {
      ...baseInfo,
      sections: [
        { title: 'Shadow Registers', category: 'content', properties: [{ name: "AF'", value: '0x5678' }] },
        { title: 'Registers', category: 'content', properties: [{ name: 'AF', value: '0x1234' }] },
        { title: 'Hardware', category: 'content', properties: [{ name: 'Border', value: 'Blue' }] },
      ],
    };
    render(<ContentsTab info={info} />);
    const cells = screen.getAllByRole('cell');
    const borderIndex = cells.findIndex(c => c.textContent === 'Border');
    const afIndex = cells.findIndex(c => c.textContent === 'AF');
    const afPrimeIndex = cells.findIndex(c => c.textContent === "AF'");
    // Order: Hardware, Registers, Shadow Registers
    expect(borderIndex).toBeLessThan(afIndex);
    expect(afIndex).toBeLessThan(afPrimeIndex);
  });
});
