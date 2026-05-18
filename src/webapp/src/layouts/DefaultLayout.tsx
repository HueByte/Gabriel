import type { ReactNode } from 'react';
import { ToastContainer } from 'react-toastify';

interface DefaultLayoutProps {
  /** Pre-built sidebar element. Layout owns visual placement; the parent owns
   *  the sidebar's data and callbacks so different screens can vary it. */
  sidebar: ReactNode;
  children: ReactNode;
}

/**
 * The top-level app shell: sidebar overlay + main content column + global toast
 * container. Pages render their content into `children`; everything that's
 * persistent across pages (sidebar, toasts) lives here.
 */
export function DefaultLayout({ sidebar, children }: DefaultLayoutProps) {
  return (
    <div className="app">
      {sidebar}
      <main className="main-col">{children}</main>
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
