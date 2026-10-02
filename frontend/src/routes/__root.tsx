import '../index.css';

import type { QueryClient } from '@tanstack/react-query';
import { createRootRouteWithContext, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools';

import { Footer } from '../components/layout/footer';
import { Navbar } from '../components/layout/navbar';

const RootLayout = () => (
  <div className="font-inter flex min-h-dvh flex-col">
    <Navbar />
    <main className="flex-1">
      <Outlet />
    </main>
    <Footer />
    <TanStackRouterDevtools />
  </div>
);

export const Route = createRootRouteWithContext<{ queryClient: QueryClient }>()({
  component: RootLayout,
});
