import { useState } from 'react';
import type { Item, Section } from './types';
import { PropertiesTable, DetailsTable } from './PropertiesTable';

// ItemView and SectionView are co-located because they reference each other.

export function ItemView({ item, index }: { item: Item; index: number }) {
  const hasNested = item.sections && item.sections.length > 0;
  const hasDetails = item.details && Object.keys(item.details).length > 0;
  const [open, setOpen] = useState(false);

  if (hasNested) {
    return (
      <div className={`item item-nested${open ? ' item-nested--open' : ''}`}>
        <div className="item-nested-summary" onClick={() => setOpen(o => !o)}>
          <span className="item-nested-arrow">▸</span>
          <span className="item-index">{index}.</span>
          <span className="item-title">{item.title}</span>
          {item.properties && (
            <span className="item-inline-props">
              {item.properties.map(p => `${p.name}: ${p.value}`).join(', ')}
            </span>
          )}
        </div>
        {open && (
          <div className="item-nested-content">
            {item.sections!.map((section, i) => (
              <SectionView key={i} section={section} hideTitle={section.title === 'Blocks'} />
            ))}
          </div>
        )}
      </div>
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

export function SectionView({ section, hideTitle = false }: { section: Section; hideTitle?: boolean }) {
  return (
    <div className="section">
      {!hideTitle && <h3 className="section-title">{section.title}</h3>}
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
