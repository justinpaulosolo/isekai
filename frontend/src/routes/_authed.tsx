import { createFileRoute, Outlet, redirect } from '@tanstack/react-router';

import { meQueryOptions, type User } from '../lib/auth';

export const Route = createFileRoute('/_authed')({
  beforeLoad: async ({ context, location }) => {
    const user: User | null = await context.queryClient.query(meQueryOptions);
    if (!user) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects are thrown, not Error instances
      throw redirect({ to: '/', search: { redirect: location.href } });
    }
  },
  component: () => <Outlet />,
});
