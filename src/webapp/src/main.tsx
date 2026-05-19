import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { installAuthInterceptor } from './api/authInterceptor';
import 'react-toastify/dist/ReactToastify.css';
// Syntax highlighting theme - applied to .hljs spans that rehype-highlight
// emits inside `.md pre code` blocks.
import 'highlight.js/styles/github-dark.css';
// KaTeX font / layout styles for the math nodes rehype-katex produces.
import 'katex/dist/katex.min.css';
import './styles.css';

// One-time global setup: refresh-on-401 retry for the generated axios client.
installAuthInterceptor();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
