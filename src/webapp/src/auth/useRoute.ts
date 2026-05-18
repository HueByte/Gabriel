import { useEffect, useState } from 'react';

// Tiny pushState-based router. We have all of two routes (/login, /register)
// plus everything-else = chat, so pulling in react-router is overkill.
// Listens to popstate (back/forward) and a custom event for in-app navigations.

const NAV_EVENT = 'gabriel:navigate';

export function useRoute(): { path: string; navigate: (path: string) => void } {
  const [path, setPath] = useState(() => window.location.pathname);

  useEffect(() => {
    const onChange = () => setPath(window.location.pathname);
    window.addEventListener('popstate', onChange);
    window.addEventListener(NAV_EVENT, onChange);
    return () => {
      window.removeEventListener('popstate', onChange);
      window.removeEventListener(NAV_EVENT, onChange);
    };
  }, []);

  const navigate = (next: string) => {
    if (next === window.location.pathname) return;
    window.history.pushState({}, '', next);
    window.dispatchEvent(new Event(NAV_EVENT));
  };

  return { path, navigate };
}
