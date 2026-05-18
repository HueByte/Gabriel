import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import { AuthProvider } from './auth/AuthContext';
import { AuthGate } from './auth/AuthGate';
import { installAuthInterceptor } from './api/authInterceptor';
import 'react-toastify/dist/ReactToastify.css';
import './styles.css';

// One-time global setup: refresh-on-401 retry for the generated axios client.
installAuthInterceptor();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <AuthGate>
        <App />
      </AuthGate>
    </AuthProvider>
  </StrictMode>,
);
