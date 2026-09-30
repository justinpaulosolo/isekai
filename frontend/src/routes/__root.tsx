import { createRootRoute, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/react-router-devtools';

import { Navbar } from '../components/layout/navbar';

import '../index.css';

const RootLayout = () => (
  <div className="font-inter">
    <Navbar />
    {/* <div className="">
      <Link to="/" className="">
        Home
      </Link>{' '}
    </div>
    <hr /> */}
    <Outlet />
    <TanStackRouterDevtools />
  </div>
);

export const Route = createRootRoute({ component: RootLayout });
