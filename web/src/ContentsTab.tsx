import type { FileInfo } from './types';
import { SectionView } from './SectionView';
import { PropertiesTable } from './PropertiesTable';

const REGISTER_ORDER: Record<string, number> = {
  'Registers': 1,
  'Shadow Registers': 2,
};

export function ContentsTab({ info }: { info: FileInfo }) {
  const contentSections = info.sections.filter(s => s.category === 'content');

  if (contentSections.length === 0) {
    return <div className="tab-content"><p className="empty-note">No content sections available.</p></div>;
  }

  // All property-only sections (no item lists) → snapshot layout: 3-column grid, no headers.
  // Order: Registers, Shadow Registers, then the rest.
  const isPropertyOnly = contentSections.every(s => !s.items);
  if (isPropertyOnly) {
    const sorted = [...contentSections].sort((a, b) => {
      const ao = REGISTER_ORDER[a.title] ?? 0;
      const bo = REGISTER_ORDER[b.title] ?? 0;
      return ao - bo;
    });
    return (
      <div className="tab-content">
        <div className="sections-grid">
          {sorted.map((section, i) => (
            <PropertiesTable key={i} properties={section.properties ?? []} />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="tab-content">
      {contentSections.map((section, i) => (
        <SectionView key={i} section={section} hideTitle={section.title === 'Blocks'} />
      ))}
    </div>
  );
}
