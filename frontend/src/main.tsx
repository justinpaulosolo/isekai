import { createRouter, RouterProvider } from '@tanstack/react-router';
import ReactDOM, { type Root } from 'react-dom/client';

const queryClient: QueryClient = new QueryClient();

// Import the generated route tree
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

import { routeTree } from './routeTree.gen';

// Create a new router instance
const router = createRouter({
  routeTree,
  context: { queryClient },
});

// Register the router instance for type safety
declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}

// Render the app
const rootElement: HTMLElement | null = document.getElementById('root');
if (!rootElement) throw new Error('Root element #root not found');
if (!rootElement.innerHTML) {
  const root: Root = ReactDOM.createRoot(rootElement);
  root.render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );
}
