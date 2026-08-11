# main

> **File:** `src/webapp/src/main.tsx`  
> **Kind:** file


Bootstraps the React application: installs a one-time authentication interceptor for the HTTP client, loads global styles, and renders App into the DOM's root element inside StrictMode.

## Remarks

By centralizing initialization here, the app ensures that global concerns (HTTP retry behavior, theming, and math/code styling) are in place before components mount. Wrapping the render in StrictMode in development helps surface potential side effects and lifecycle issues early, while the imported CSS assets guarantee consistent visuals and correct math/code rendering across the UI. This file acts as a clear boundary between setup and application logic, keeping App focused on rendering and state management.

## Notes

- The code uses a non-null assertion on the root element (document.getElementById('root')!). Ensure the HTML contains an element with id 'root'; otherwise, the app will crash at startup.
- installAuthInterceptor is described as a one-time global setup; in development environments with hot module replacement, repeated evaluation could re-register interceptors. Ensure this setup remains idempotent or guarded in such environments.