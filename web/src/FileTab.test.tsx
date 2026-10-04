import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { FileTab } from './FileTab';

describe('FileTab', () => {
  it('shows file metadata without image or palette previews', () => {
    render(<FileTab fileName="test.bmp" fileSize={123} info={{
      format: 'Windows Bitmap', fileExtension: 'bmp', type: 'image', convertibleTo: [], sections: [],
      palette: [{ red: 255, green: 0, blue: 0, alpha: 255 }],
      image: { width: 1, height: 1, pixels: 'AAAA/w==' },
    }} />);
    expect(screen.getByText('test.bmp')).toBeInTheDocument();
    expect(screen.getByText('Windows Bitmap')).toBeInTheDocument();
    expect(screen.getByText('.bmp')).toBeInTheDocument();
    expect(screen.queryByRole('img')).not.toBeInTheDocument();
    expect(screen.queryByText('Image preview')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Zoom')).not.toBeInTheDocument();
  });
});
