# main

> **File:** `src/webapp/src/main.tsx`  
> **Kind:** file

Initializes and mounts the web application. This entrypoint runs one-time global setup (notably installing an auth/refresh interceptor for the generated axios client), imports global CSS and third-party styles used by the app (toast notifications, syntax highlighting, KaTeX), and attaches the React <App /> to the DOM inside React.StrictMode.

## Remarks
The file exists to centralize side-effectful, application-wide initialization that must happen before any UI or API activity: the auth interceptor is installed before the app renders so that any API calls made by components immediately after mount benefit from the refresh-on-401 behavior. Global styles and third-party CSS are imported here to ensure they are included once for the entire app.

## Notes
- The non-null assertion on document.getElementById('root') (!) will throw if the host HTML does not provide an element with id="root"; ensure your index.html contains that element.
- React.StrictMode may double-invoke certain lifecycle and effect behavior in development; avoid relying on single execution for side effects during mount.
- The order of imports and setup matters: installAuthInterceptor should run before components that make API calls, and global CSS imports here can affect style specificity and theming for the entire app.