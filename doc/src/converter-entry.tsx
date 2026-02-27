import { createRoot } from 'react-dom/client';
import { Converter } from './converter';

const init = () => {
  const container = document.getElementById('oakio-converter');
  if (container) {
    const root = createRoot(container);
    root.render(<Converter />);
  }
};

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}
