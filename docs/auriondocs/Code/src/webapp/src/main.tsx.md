# main

> **File:** `src/webapp/src/main.tsx`  
> **Kind:** file


This file is the application entry point: it bootstraps the UI by installing a one-time authentication interceptor for the HTTP client, loading global styles, and mounting the App into the root element inside React.StrictMode.

## Remarks
This module centralizes startup concerns that affect the entire application, separating bootstrapping from component logic. By installing the authentication interceptor and loading global styles in one place, it guarantees consistent behavior and styling before any component renders. It also enables development-time protections like StrictMode, helping reveal potential unsafe lifecycles and effects during development.

## Notes
- Be aware that this file runs side effects at import time (interceptor installation and root rendering); avoid importing it in test environments that don't want full bootstrap behavior.