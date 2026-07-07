# App

> **File:** `src/webapp/src/App.tsx`  
> **Kind:** function

```typescript
export function App()
```


App is the root React component that wires the authentication context to the router by rendering AuthProvider around RouterProvider with the app's router. Use App as the entry point when booting the UI so all routes and descendants have access to authentication state and routing.

## Remarks
Centralizes the provider composition, establishing a stable composition root for the app. It ensures that authentication and routing contexts are consistently available to every page and component. This pattern also makes it easier to extend the root with additional global providers (such as theming or localization) without scattering wrappers throughout the codebase.

## Example
```tsx
import ReactDOM from 'react-dom/client';
import { App } from './src/webapp/src/App';

ReactDOM.createRoot(document.getElementById('root')!).render(<App />);
```

## Notes
- The App component assumes a router variable defined (likely in the same module or imported) to be passed to RouterProvider.
- Wrapping order matters: AuthProvider must wrap RouterProvider to ensure auth context is accessible inside route components.
- If you introduce additional providers, keep them inside App to preserve a single entry point for the provider tree.
