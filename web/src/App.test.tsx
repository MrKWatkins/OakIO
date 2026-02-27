import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import App from './App';

describe('App', () => {
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
    expect(screen.getByText(/Select a ZX Spectrum file/)).toBeInTheDocument();
  });

  it('has a file input accepting ZX Spectrum formats', () => {
    render(<App />);
    const input = document.querySelector('input[type="file"]');
    expect(input).toBeInTheDocument();
    expect(input?.getAttribute('accept')).toBe('.tap,.tzx,.pzx,.z80,.sna,.nex');
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
