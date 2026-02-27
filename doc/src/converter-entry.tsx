import { createRoot } from 'react-dom/client';
import App from '../../web/src/App';
import './converter.css';

const init = () => {
  const container = document.getElementById('oakio-converter');
  if (container) {
    const root = createRoot(container);
    root.render(<App showTitle={false} />);
  }
};

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', init);
} else {
  init();
}
