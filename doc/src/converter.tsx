import { useState, useCallback, useRef } from 'react';
import { getInfo, convert } from './oakio';
import type { FileInfo, Section, Property, Item, ConvertibleFormat } from './types';
import './converter.css';

type Tab = 'file' | 'contents' | 'convert';

const zxColours: Record<string, string> = {
  'Black': '#000000',
  'Blue': '#0000D7',
  'Red': '#D70000',
  'Magenta': '#D700D7',
  'Green': '#00D700',
  'Cyan': '#00D7D7',
  'Yellow': '#D7D700',
  'White': '#D7D7D7',
};

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} bytes`;
  }
  const kb = bytes / 1024;
  if (kb < 1024) {
    return `${kb.toFixed(1)} KB`;
  }
  const mb = kb / 1024;
  return `${mb.toFixed(1)} MB`;
}

function PropertyValue({ property }: { property: Property }) {
  if (property.format === 'boolean') {
    return <span className={`oakio-bool-${property.value}`}>{property.value === 'true' ? '✓' : '✗'}</span>;
  }
  if (property.format === 'colour') {
    const bg = zxColours[property.value] ?? property.value.toLowerCase();
    return (
      <span className="oakio-colour-value">
        <span className="oakio-colour-swatch" style={{ backgroundColor: bg }} />
        {property.value}
      </span>
    );
  }
  if (property.format === 'hex') {
    return <code>{property.value}</code>;
  }
  return <>{property.value}</>;
}

function PropertiesTable({ properties }: { properties: Property[] }) {
  return (
    <table className="oakio-properties-table">
      <tbody>
        {properties.map((prop, i) => (
          <tr key={i}>
            <td className="oakio-prop-name">{prop.name}</td>
            <td className="oakio-prop-value"><PropertyValue property={prop} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function DetailsTable({ details }: { details: Record<string, string> }) {
  return (
    <details className="oakio-details-expander">
      <summary>Details</summary>
      <table className="oakio-properties-table">
        <tbody>
          {Object.entries(details).map(([key, value]) => (
            <tr key={key}>
              <td className="oakio-prop-name">{key}</td>
              <td className="oakio-prop-value">{value}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </details>
  );
}

function ItemView({ item, index }: { item: Item; index: number }) {
  const hasNested = item.sections && item.sections.length > 0;
  const hasDetails = item.details && Object.keys(item.details).length > 0;

  if (hasNested) {
    return (
      <details className="oakio-item oakio-item-nested">
        <summary>
          <span className="oakio-item-index">{index}.</span>
          <span className="oakio-item-title">{item.title}</span>
          {item.properties && (
            <span className="oakio-item-inline-props">
              {item.properties.map(p => `${p.name}: ${p.value}`).join(', ')}
            </span>
          )}
        </summary>
        <div className="oakio-item-nested-content">
          {item.sections!.map((section, i) => (
            <SectionView key={i} section={section} />
          ))}
        </div>
      </details>
    );
  }

  return (
    <div className="oakio-item">
      <div className="oakio-item-header">
        <span className="oakio-item-index">{index}.</span>
        <span className="oakio-item-title">{item.title}</span>
      </div>
      {item.properties && <PropertiesTable properties={item.properties} />}
      {hasDetails && <DetailsTable details={item.details!} />}
    </div>
  );
}

function SectionView({ section }: { section: Section }) {
  return (
    <div className="oakio-section">
      <h3 className="oakio-section-title">{section.title}</h3>
      {section.properties && <PropertiesTable properties={section.properties} />}
      {section.items && (
        <div className="oakio-items-list">
          {section.items.map((item, i) => (
            <ItemView key={i} item={item} index={i + 1} />
          ))}
          {section.items.length === 0 && <p className="oakio-empty-note">None</p>}
        </div>
      )}
    </div>
  );
}

function FileTab({ info, fileName, fileSize }: { info: FileInfo; fileName: string; fileSize: number }) {
  const fileSections = info.sections.filter(s => s.category === 'file');

  return (
    <div className="oakio-tab-content">
      <table className="oakio-properties-table oakio-file-meta">
        <tbody>
          <tr>
            <td className="oakio-prop-name">Format</td>
            <td className="oakio-prop-value">{info.format}</td>
          </tr>
          <tr>
            <td className="oakio-prop-name">Type</td>
            <td className="oakio-prop-value">{info.type}</td>
          </tr>
          <tr>
            <td className="oakio-prop-name">Filename</td>
            <td className="oakio-prop-value">{fileName}</td>
          </tr>
          <tr>
            <td className="oakio-prop-name">Size</td>
            <td className="oakio-prop-value">{formatFileSize(fileSize)}</td>
          </tr>
          <tr>
            <td className="oakio-prop-name">Extension</td>
            <td className="oakio-prop-value">.{info.fileExtension}</td>
          </tr>
        </tbody>
      </table>
      {fileSections.map((section, i) => (
        <SectionView key={i} section={section} />
      ))}
    </div>
  );
}

function ContentsTab({ info }: { info: FileInfo }) {
  const contentSections = info.sections.filter(s => s.category === 'content');

  if (contentSections.length === 0) {
    return <div className="oakio-tab-content"><p className="oakio-empty-note">No content sections available.</p></div>;
  }

  return (
    <div className="oakio-tab-content">
      {contentSections.map((section, i) => (
        <SectionView key={i} section={section} />
      ))}
    </div>
  );
}

function ConvertTab({ formats, onConvert, converting }: {
  formats: ConvertibleFormat[];
  onConvert: (ext: string) => void;
  converting: string | null;
}) {
  if (formats.length === 0) {
    return <div className="oakio-tab-content"><p className="oakio-empty-note">No conversion options available for this format.</p></div>;
  }

  return (
    <div className="oakio-tab-content">
      <p className="oakio-convert-hint">Choose a format to convert and download:</p>
      <div className="oakio-convert-buttons">
        {formats.map(f => (
          <button
            key={f.extension}
            className="oakio-convert-button"
            disabled={converting !== null}
            onClick={() => onConvert(f.extension)}
          >
            {f.name}
          </button>
        ))}
      </div>
      {converting && <p className="oakio-converting-message">Converting…</p>}
    </div>
  );
}

export function Converter() {
  const [tab, setTab] = useState<Tab>('file');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [infoResult, setInfoResult] = useState<FileInfo | null>(null);
  const [fileName, setFileName] = useState('');
  const [fileSize, setFileSize] = useState(0);
  const [converting, setConverting] = useState<string | null>(null);
  const fileDataRef = useRef<Uint8Array | null>(null);

  const handleFileChange = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) {
      return;
    }

    setLoading(true);
    setError(null);
    setInfoResult(null);
    setConverting(null);
    setTab('file');

    try {
      const data = new Uint8Array(await file.arrayBuffer());
      fileDataRef.current = data;
      setFileName(file.name);
      setFileSize(file.size);
      const result = await getInfo(file.name, data);
      setInfoResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  }, []);

  const handleConvert = useCallback(async (outputExtension: string) => {
    const data = fileDataRef.current;
    if (!data || !fileName) {
      return;
    }

    const outputFilename = fileName.replace(/\.[^.]+$/, `.${outputExtension}`);

    setConverting(outputExtension);
    setError(null);

    try {
      const result = await convert(fileName, data, outputFilename);

      const blob = new Blob([result.buffer as ArrayBuffer]);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = outputFilename;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setConverting(null);
    }
  }, [fileName]);

  return (
    <div className="oakio-converter">
      <div className="oakio-file-picker">
        <label htmlFor="oakio-fileInput">Select a ZX Spectrum file:</label>
        <input
          type="file"
          id="oakio-fileInput"
          accept=".tap,.tzx,.pzx,.z80,.sna,.nex"
          onChange={handleFileChange}
        />
      </div>

      {error && <div className="oakio-error">{error}</div>}
      {loading && <div className="oakio-loading">Loading…</div>}

      <div className="oakio-tabs">
        <button className={tab === 'file' ? 'active' : ''} onClick={() => setTab('file')}>
          File
        </button>
        <button className={tab === 'contents' ? 'active' : ''} onClick={() => setTab('contents')}>
          Contents
        </button>
        <button className={tab === 'convert' ? 'active' : ''} onClick={() => setTab('convert')}>
          Convert
        </button>
      </div>

      <div className="oakio-tab-panel">
        {!infoResult && !loading && (
          <p className="oakio-empty-note">Load a file to get started.</p>
        )}
        {infoResult && tab === 'file' && <FileTab info={infoResult} fileName={fileName} fileSize={fileSize} />}
        {infoResult && tab === 'contents' && <ContentsTab info={infoResult} />}
        {infoResult && tab === 'convert' && <ConvertTab formats={infoResult.convertibleTo} onConvert={handleConvert} converting={converting} />}
      </div>
    </div>
  );
}
