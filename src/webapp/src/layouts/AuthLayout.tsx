import { Outlet } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';

// Login + register share the centered-card chrome (.auth-screen styling lives
// on the page components themselves). The layout exists mainly so toast
// notifications still render when a user is unauthenticated — surfacing things
// like "session expired" or refresh failures without forcing them into the
// main app shell.
export function AuthLayout() {
  return (
    <>
      <Outlet />
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
    </>
  );
}
