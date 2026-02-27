import { useState } from 'react';
import type { Property } from './types';
import { zxColours } from './utils';

export function PropertyValue({ property }: { property: Property }) {
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

export function PropertiesTable({ properties }: { properties: Property[] }) {
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

export function DetailsTable({ details }: { details: Record<string, string> }) {
  const [open, setOpen] = useState(false);
  return (
    <div className={`details-expander${open ? ' details-expander--open' : ''}`}>
      <div className="details-expander-summary" onClick={() => setOpen(o => !o)}>
        <span className="details-expander-arrow">▸</span> Details
      </div>
      {open && (
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
      )}
    </div>
  );
}
