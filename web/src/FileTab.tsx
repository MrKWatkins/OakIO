import type { FileInfo } from './types';
import { SectionView } from './SectionView';
import { formatFileSize } from './utils';

export function FileTab({ info, fileName, fileSize }: { info: FileInfo; fileName: string; fileSize: number }) {
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
