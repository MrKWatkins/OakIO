import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import App from './App';

describe('App', () => {
  it('renders the OakIO heading', () => {
    render(<App />);
    expect(screen.getByText('OakIO')).toBeInTheDocument();
  });

  it('renders the subtitle', () => {
    render(<App />);
    expect(screen.getByText('ZX Spectrum tape file tools')).toBeInTheDocument();
  });

  it('renders Info and Convert tabs', () => {
    render(<App />);
    expect(screen.getByText('Info')).toBeInTheDocument();
    expect(screen.getByText('Convert')).toBeInTheDocument();
  });

  it('shows info panel by default', () => {
    render(<App />);
    expect(screen.getByText(/Select a ZX Spectrum file to view its information/)).toBeInTheDocument();
  });

  it('switches to convert panel when Convert tab is clicked', () => {
    render(<App />);
    fireEvent.click(screen.getByText('Convert'));
    expect(screen.getByText(/Select a ZX Spectrum file and choose an output format/)).toBeInTheDocument();
  });

  it('switches back to info panel when Info tab is clicked', () => {
    render(<App />);
    fireEvent.click(screen.getByText('Convert'));
    fireEvent.click(screen.getByText('Info'));
    expect(screen.getByText(/Select a ZX Spectrum file to view its information/)).toBeInTheDocument();
  });

  it('has file input for info tab', () => {
    render(<App />);
    const input = document.querySelector('input[type="file"]');
    expect(input).toBeInTheDocument();
    expect(input?.getAttribute('accept')).toBe('.tap,.tzx,.pzx,.z80,.sna');
  });

  it('has file input and format select for convert tab', () => {
    render(<App />);
    fireEvent.click(screen.getByText('Convert'));

    const fileInput = document.querySelector('input[type="file"]');
    expect(fileInput).toBeInTheDocument();
    expect(fileInput?.getAttribute('accept')).toBe('.tap,.tzx,.pzx');

    const select = screen.getByLabelText('Output format:');
    expect(select).toBeInTheDocument();
  });

  it('has all output format options in convert tab', () => {
    render(<App />);
    fireEvent.click(screen.getByText('Convert'));

    const select = screen.getByLabelText('Output format:') as HTMLSelectElement;
    const options = Array.from(select.options).map(o => o.value);
    expect(options).toEqual(['tap', 'tzx', 'pzx', 'wav']);
  });

  it('has a submit button in convert tab', () => {
    render(<App />);
    fireEvent.click(screen.getByText('Convert'));
    expect(screen.getByText('Convert & Download')).toBeInTheDocument();
  });

  it('marks the active tab', () => {
    render(<App />);
    const infoBtn = screen.getByText('Info');
    const convertBtn = screen.getByText('Convert');

    expect(infoBtn.className).toContain('active');
    expect(convertBtn.className).not.toContain('active');

    fireEvent.click(convertBtn);
    expect(convertBtn.className).toContain('active');
    expect(infoBtn.className).not.toContain('active');
  });
});
