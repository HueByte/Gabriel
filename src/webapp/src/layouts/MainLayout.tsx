import { Outlet } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { Sidebar } from '../components/Sidebar';

// The sidebar fetches its conversation list once on mount and refreshes only
// on its own internal mutations (rename/delete/create). Cross-page bumping
// was removed: refetching the entire conversation list (and the projects
// list, via the same signal) after every chat turn was wasteful, and the
// active conversation is already highlighted - stale position-in-list is
// harmless until next mount.
export function MainLayout() {
  return (
    <div className="app">
      <Sidebar />
      <main className="main-col">
        <Outlet />
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
