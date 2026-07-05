import { compressionFormats } from './types';
import type { CompressionFormat, ConvertibleFormat } from './types';

export function ConvertTab({ formats, compressionFormat, onCompressionFormatChange, onConvert, converting, error }: {
  formats: ConvertibleFormat[];
  compressionFormat: CompressionFormat;
  onCompressionFormatChange: (compressionFormat: CompressionFormat) => void;
  onConvert: (ext: string) => void;
  converting: string | null;
  error: string | null;
}) {
  if (formats.length === 0) {
    return <div className="tab-content"><p className="empty-note">No conversion options available for this format.</p></div>;
  }

  return (
    <div className="tab-content">
      <p className="convert-hint">Choose a format to convert and download:</p>
      <div className="convert-compression">
        <label htmlFor="compressionFormat">Compression:</label>
        <select
          id="compressionFormat"
          value={compressionFormat}
          disabled={converting !== null}
          onChange={e => onCompressionFormatChange(e.target.value as CompressionFormat)}
        >
          {compressionFormats.map(f => (
            <option key={f} value={f}>{f}</option>
          ))}
        </select>
      </div>
      <div className="convert-buttons">
        {formats.map(f => (
          <button
            key={f.extension}
            className="convert-button"
            disabled={converting !== null}
            onClick={() => onConvert(f.extension)}
          >
            {f.name}
          </button>
        ))}
      </div>
      {converting && <p className="converting-message">Converting…</p>}
      {error && <p className="convert-error">{error}</p>}
    </div>
  );
}
