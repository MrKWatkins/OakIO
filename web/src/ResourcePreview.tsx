import { useEffect, useRef, useState } from 'react';
import type { FileInfo } from './types';

export function ResourcePreview({ info }: { info: FileInfo }) {
  const canvas = useRef<HTMLCanvasElement>(null);
  const [zoom, setZoom] = useState(1);
  const image = info.image;

  useEffect(() => {
    if (!image || !canvas.current) {
      return;
    }
    const context = canvas.current.getContext('2d');
    if (!context) {
      return;
    }
    const bytes = Uint8ClampedArray.from(atob(image.pixels), c => c.charCodeAt(0));
    const data = context.createImageData(image.width, image.height);
    data.data.set(bytes);
    context.putImageData(data, 0, 0);
  }, [image]);

  return (
    <div className="resource-preview">
      {image && <>
        <h2>Image preview</h2>
        <label>Zoom <select value={zoom} onChange={event => setZoom(Number(event.target.value))}>
          {[1, 2, 4, 8].map(value => <option key={value} value={value}>{value}×</option>)}
        </select></label>
        <div className="image-preview-scroll">
          <canvas ref={canvas} aria-label={`${image.width} × ${image.height} image preview`}
            width={image.width} height={image.height}
            style={{ width: image.width * zoom, height: image.height * zoom, imageRendering: 'pixelated' }} />
        </div>
      </>}
      {info.palette && <>
        <h2>Palette ({info.palette.length} colours)</h2>
        <div className="palette-grid">
          {info.palette.map((colour, index) => {
            const hex = '#' + [colour.red, colour.green, colour.blue, colour.alpha]
              .map(value => value.toString(16).padStart(2, '0')).join('').toUpperCase();
            return <div className="palette-entry" key={index}>
              <div className="palette-swatch" style={{ backgroundColor: hex }}
                title={`Index ${index}: ${hex}`} role="img" aria-label={`Index ${index}: ${hex}`} />
              <span>{index}: {hex}</span>
            </div>;
          })}
        </div>
      </>}
    </div>
  );
}
