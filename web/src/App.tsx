import { useState, useCallback, useRef } from 'react'
import { getInfo, convert } from './oakio'
import type { FileInfo, Section, Property, Item, ConvertibleFormat } from './types'
import './App.css'

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
    return <span className={`bool-${property.value}`}>{property.value === 'true' ? '✓' : '✗'}</span>;
  }
  if (property.format === 'colour') {
    const bg = zxColours[property.value] ?? property.value.toLowerCase();
    return (
      <span className="colour-value">
        <span className="colour-swatch" style={{ backgroundColor: bg }} />
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
    <table className="properties-table">
      <tbody>
        {properties.map((prop, i) => (
          <tr key={i}>
            <td className="prop-name">{prop.name}</td>
            <td className="prop-value"><PropertyValue property={prop} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function DetailsTable({ details }: { details: Record<string, string> }) {
  return (
    <details className="details-expander">
      <summary>Details</summary>
      <table className="properties-table">
        <tbody>
          {Object.entries(details).map(([key, value]) => (
            <tr key={key}>
              <td className="prop-name">{key}</td>
              <td className="prop-value">{value}</td>
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
      <details className="item item-nested">
        <summary>
          <span className="item-index">{index}.</span>
          <span className="item-title">{item.title}</span>
          {item.properties && (
            <span className="item-inline-props">
              {item.properties.map(p => `${p.name}: ${p.value}`).join(', ')}
            </span>
          )}
        </summary>
        <div className="item-nested-content">
          {item.sections!.map((section, i) => (
            <SectionView key={i} section={section} />
          ))}
        </div>
      </details>
    );
  }

  return (
    <div className="item">
      <div className="item-header">
        <span className="item-index">{index}.</span>
        <span className="item-title">{item.title}</span>
      </div>
      {item.properties && <PropertiesTable properties={item.properties} />}
      {hasDetails && <DetailsTable details={item.details!} />}
    </div>
  );
}

function SectionView({ section }: { section: Section }) {
  return (
    <div className="section">
      <h3 className="section-title">{section.title}</h3>
      {section.properties && <PropertiesTable properties={section.properties} />}
      {section.items && (
        <div className="items-list">
          {section.items.map((item, i) => (
            <ItemView key={i} item={item} index={i + 1} />
          ))}
          {section.items.length === 0 && <p className="empty-note">None</p>}
        </div>
      )}
    </div>
  );
}

function FileTab({ info, fileName, fileSize }: { info: FileInfo; fileName: string; fileSize: number }) {
  const fileSections = info.sections.filter(s => s.category === 'file');

  return (
    <div className="tab-content">
      <table className="properties-table file-meta">
        <tbody>
          <tr>
            <td className="prop-name">Format</td>
            <td className="prop-value">{info.format}</td>
          </tr>
          <tr>
            <td className="prop-name">Type</td>
            <td className="prop-value">{info.type}</td>
          </tr>
          <tr>
            <td className="prop-name">Filename</td>
            <td className="prop-value">{fileName}</td>
          </tr>
          <tr>
            <td className="prop-name">Size</td>
            <td className="prop-value">{formatFileSize(fileSize)}</td>
          </tr>
          <tr>
            <td className="prop-name">Extension</td>
            <td className="prop-value">.{info.fileExtension}</td>
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
    return <div className="tab-content"><p className="empty-note">No content sections available.</p></div>;
  }

  return (
    <div className="tab-content">
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
    </div>
  );
}

function App() {
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
    <div className="app">
      <h1>OakIO</h1>
      <p className="subtitle">ZX Spectrum file tools</p>

      <div className="file-picker">
        <label htmlFor="fileInput">Select a ZX Spectrum file:</label>
        <input
          type="file"
          id="fileInput"
          accept=".tap,.tzx,.pzx,.z80,.sna,.nex"
          onChange={handleFileChange}
        />
      </div>

      {error && <div className="error">{error}</div>}
      {loading && <div className="loading">Loading…</div>}

      <div className="tabs">
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

      <div className="tab-panel">
        {!infoResult && !loading && (
          <p className="empty-note">Load a file to get started.</p>
        )}
        {infoResult && tab === 'file' && <FileTab info={infoResult} fileName={fileName} fileSize={fileSize} />}
        {infoResult && tab === 'contents' && <ContentsTab info={infoResult} />}
        {infoResult && tab === 'convert' && <ConvertTab formats={infoResult.convertibleTo} onConvert={handleConvert} converting={converting} />}
      </div>
    </div>
  );
}

export default App
