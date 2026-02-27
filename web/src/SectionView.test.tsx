import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { SectionView } from './SectionView';
import type { Section } from './types';

describe('SectionView', () => {
  it('renders the section title', () => {
    const section: Section = { title: 'Header', category: 'file' };
    render(<SectionView section={section} />);
    expect(screen.getByText('Header')).toBeInTheDocument();
  });

  it('hides the section title when hideTitle is true', () => {
    const section: Section = { title: 'Blocks', category: 'content' };
    render(<SectionView section={section} hideTitle />);
    expect(screen.queryByText('Blocks')).not.toBeInTheDocument();
  });

  it('renders items', () => {
    const section: Section = {
      title: 'Blocks',
      category: 'content',
      items: [{ title: 'Data Block' }],
    };
    render(<SectionView section={section} />);
    expect(screen.getByText('Data Block')).toBeInTheDocument();
  });

  it('renders empty note when items list is empty', () => {
    const section: Section = { title: 'Blocks', category: 'content', items: [] };
    render(<SectionView section={section} />);
    expect(screen.getByText('None')).toBeInTheDocument();
  });
});
