import type { ConvertibleFormat } from './types';

export function ConvertTab({ formats, onConvert, converting, error }: {
  formats: ConvertibleFormat[];
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
