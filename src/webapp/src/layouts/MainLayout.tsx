import { useCallback, useState } from 'react';
import { Outlet, useOutletContext } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { Sidebar } from '../components/Sidebar';

// Cross-page coordination signal. ChatPage bumps this after a successful
// turn so the sidebar re-fetches its conversation list (which re-sorts by
// updatedAt). Lifted here so the Sidebar (in the layout) and the page (in
// the Outlet) share a single source of truth without going through a context
// provider that lives above the router.
export interface MainLayoutContext {
  bumpSidebar: () => void;
}

export function useMainLayout(): MainLayoutContext {
  return useOutletContext<MainLayoutContext>();
}

export function MainLayout() {
  const [sidebarRefresh, setSidebarRefresh] = useState(0);
  const bumpSidebar = useCallback(() => setSidebarRefresh(n => n + 1), []);

  return (
    <div className="app">
      <Sidebar refreshKey={sidebarRefresh} />
      <main className="main-col">
        <Outlet context={{ bumpSidebar } satisfies MainLayoutContext} />
      </main>
      <ToastContainer
        position="bottom-right"
        autoClose={4000}
        newestOnTop
        closeOnClick
        pauseOnHover
        draggable={false}
        theme="dark"
        toastClassName="gbr-toast"
      />
    </div>
  );
}
