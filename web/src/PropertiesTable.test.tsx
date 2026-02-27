import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { PropertyValue, PropertiesTable } from './PropertiesTable';
import type { Property } from './types';

describe('PropertyValue', () => {
  it('renders plain text value', () => {
    const prop: Property = { name: 'Format', value: 'TAP' };
    render(<PropertyValue property={prop} />);
    expect(screen.getByText('TAP')).toBeInTheDocument();
  });

  it('renders boolean true as checkmark', () => {
    const prop: Property = { name: 'Flag', value: 'true', format: 'boolean' };
    render(<PropertyValue property={prop} />);
    expect(screen.getByText('✓')).toBeInTheDocument();
  });

  it('renders boolean false as cross', () => {
    const prop: Property = { name: 'Flag', value: 'false', format: 'boolean' };
    render(<PropertyValue property={prop} />);
    expect(screen.getByText('✗')).toBeInTheDocument();
  });

  it('renders hex value in code element', () => {
    const prop: Property = { name: 'Address', value: '0x8000', format: 'hex' };
    render(<PropertyValue property={prop} />);
    expect(screen.getByText('0x8000').tagName).toBe('CODE');
  });
});

describe('PropertiesTable', () => {
  it('renders all property names and values', () => {
    const properties: Property[] = [
      { name: 'Format', value: 'TAP' },
      { name: 'Size', value: '1024 bytes' },
    ];
    render(<PropertiesTable properties={properties} />);
    expect(screen.getByText('Format')).toBeInTheDocument();
    expect(screen.getByText('TAP')).toBeInTheDocument();
    expect(screen.getByText('Size')).toBeInTheDocument();
    expect(screen.getByText('1024 bytes')).toBeInTheDocument();
  });
});
