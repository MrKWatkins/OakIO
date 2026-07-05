import { useState, useCallback, useRef } from 'react'
import { getInfo, convert, getCompressedFilename } from './oakio'
import type { CompressionFormat, FileInfo } from './types'
import { FileTab } from './FileTab'
import { ContentsTab } from './ContentsTab'
import { ConvertTab } from './ConvertTab'
import './App.css'

type Tab = 'file' | 'contents' | 'convert';

function App({ showTitle = true }: { showTitle?: boolean }) {
  const [tab, setTab] = useState<Tab>('file');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [infoResult, setInfoResult] = useState<FileInfo | null>(null);
  const [fileName, setFileName] = useState('');
  const [fileSize, setFileSize] = useState(0);
  const [converting, setConverting] = useState<string | null>(null);
  const [convertError, setConvertError] = useState<string | null>(null);
  const [compressionFormat, setCompressionFormat] = useState<CompressionFormat>('None');
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
    setConvertError(null);
    setCompressionFormat('None');
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
    setConvertError(null);
    setError(null);

    try {
      const [result, downloadFilename] = await Promise.all([
        convert(fileName, data, outputFilename, compressionFormat),
        getCompressedFilename(outputFilename, compressionFormat),
      ]);

      const blob = new Blob([result.buffer as ArrayBuffer]);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = downloadFilename;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      setConvertError(err instanceof Error ? err.message : String(err));
    } finally {
      setConverting(null);
    }
  }, [fileName, compressionFormat]);

  return (
    <div className="app">
      {showTitle && <h1>OakIO</h1>}
      {showTitle && <p className="subtitle">ZX Spectrum file tools</p>}

      <div className="file-picker">
        <label htmlFor="fileInput">File:</label>
        <input
          type="file"
          id="fileInput"
          accept=".tap,.tzx,.pzx,.z80,.sna,.nex,.rzx"
          onChange={handleFileChange}
        />
        {loading && <span className="loading">Loading…</span>}
      </div>

      {error && <div className="error">{error}</div>}

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
        {infoResult && tab === 'convert' && (
          <ConvertTab
            formats={infoResult.convertibleTo}
            compressionFormat={compressionFormat}
            onCompressionFormatChange={setCompressionFormat}
            onConvert={handleConvert}
            converting={converting}
            error={convertError}
          />
        )}
      </div>
    </div>
  );
}

export default App

