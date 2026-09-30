import { createRootRoute, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools';

import { Navbar } from '../components/layout/navbar';
import { Footer } from '../components/layout/footer';

import '../index.css';

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

export const Route = createRootRoute({ component: RootLayout });
