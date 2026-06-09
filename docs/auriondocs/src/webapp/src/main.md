# main

> **File:** `src/webapp/src/main.tsx`  
> **Kind:** file

Bootstraps the single-page web application: imports global styles and third-party CSS, installs a one-time authentication interceptor for the generated axios client, and mounts the React App component into the DOM using React 18's createRoot wrapped in StrictMode. Edit this file when you need to change global side effects (styles or interceptors) or adjust how the app is mounted.

## Remarks
This file performs the app-level initialization that must run exactly once on startup. It ensures the refresh-on-401 auth interceptor is registered before any components render, so network requests initiated during initial render are handled correctly. Global CSS and theme imports are included here as side-effectful module imports so the bundler emits them into the client bundle. Using React.StrictMode helps reveal unsafe lifecycle usage during development; createRoot is used for a client-rendered React 18 application (this file does not perform server-side hydration).

## Notes
- document.getElementById('root') uses a non-null assertion (!). If the root div is missing from the host HTML the call will throw at runtime — ensure your index.html provides an element with id="root".
- installAuthInterceptor registers global axios interceptors; calling it more than once will add duplicate interceptors. Keep this invocation as a one-time setup.
- React.StrictMode double-invokes certain lifecycle methods and effects in development; avoid placing irreversible side effects in component mounts that assume single execution.