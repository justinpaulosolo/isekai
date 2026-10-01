import { createFileRoute, redirect, Outlet } from '@tanstack/react-router'
import { meQueryOptions} from '../lib/auth'

export const Route = createFileRoute('/_authed')({
  beforeLoad: async ({ context, location }) => {
    const user = await context.queryClient.query(meQueryOptions)
    if (!user) {
      throw redirect({to: '/', search: { redirect: location.href }})
    }
  },
  component: () => <Outlet />,
})