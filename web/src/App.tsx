import { useState, useCallback } from 'react'
import { getInfo, convert } from './oakio'
import './App.css'

type Tab = 'info' | 'convert';

function App() {
  const [tab, setTab] = useState<Tab>('info');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [infoResult, setInfoResult] = useState<string | null>(null);

  const handleInfo = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setLoading(true);
    setError(null);
    setInfoResult(null);

    try {
      const data = new Uint8Array(await file.arrayBuffer());
      const result = await getInfo(file.name, data);
      setInfoResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setLoading(false);
    }
  }, []);

  const handleConvert = useCallback(async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = e.currentTarget;
    const fileInput = form.elements.namedItem('inputFile') as HTMLInputElement;
    const formatSelect = form.elements.namedItem('outputFormat') as HTMLSelectElement;

    const file = fileInput.files?.[0];
    if (!file) return;

    const outputFormat = formatSelect.value;
    const outputFilename = file.name.replace(/\.[^.]+$/, `.${outputFormat}`);

    setLoading(true);
    setError(null);

    try {
      const data = new Uint8Array(await file.arrayBuffer());
      const result = await convert(file.name, data, outputFilename);

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
      setLoading(false);
    }
  }, []);

  return (
    <div className="app">
      <h1>OakIO</h1>
      <p className="subtitle">ZX Spectrum tape file tools</p>

      <div className="tabs">
        <button className={tab === 'info' ? 'active' : ''} onClick={() => setTab('info')}>
          Info
        </button>
        <button className={tab === 'convert' ? 'active' : ''} onClick={() => setTab('convert')}>
          Convert
        </button>
      </div>

      {error && <div className="error">{error}</div>}
      {loading && <div className="loading">Processing…</div>}

      {tab === 'info' && (
        <div className="panel">
          <p>Select a ZX Spectrum file to view its information.</p>
          <input
            type="file"
            accept=".tap,.tzx,.pzx,.z80,.sna"
            onChange={handleInfo}
          />
          {infoResult && <pre className="result">{infoResult}</pre>}
        </div>
      )}

      {tab === 'convert' && (
        <div className="panel">
          <p>Select a ZX Spectrum file and choose an output format.</p>
          <form onSubmit={handleConvert}>
            <div className="form-row">
              <label htmlFor="inputFile">Input file:</label>
              <input
                type="file"
                id="inputFile"
                name="inputFile"
                accept=".tap,.tzx,.pzx"
                required
              />
            </div>
            <div className="form-row">
              <label htmlFor="outputFormat">Output format:</label>
              <select id="outputFormat" name="outputFormat">
                <option value="tap">TAP</option>
                <option value="tzx">TZX</option>
                <option value="pzx">PZX</option>
                <option value="wav">WAV</option>
              </select>
            </div>
            <button type="submit" disabled={loading}>Convert &amp; Download</button>
          </form>
        </div>
      )}
    </div>
  );
}

export default App
