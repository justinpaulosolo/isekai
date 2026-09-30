import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/login/')({
  validateSearch: (search: Record<string, unknown>) => ({
    redirect: typeof search.redirect == 'string' ? search.redirect : '/',
  }),
  component: LoginPage,
})

function LoginPage() {
  const { redirect } = Route.useSearch()
  const handleGoogleLogin = () => {
    window.location.href = `/api/auth/login?returnUrl=${encodeURIComponent(redirect)}`
  }

  const handleCheckMyInfo = async () => {
    try {
      const res = await fetch('/api/auth/me', {
            method: 'GET',
            credentials: 'include',
          },
      );

      if (!res.ok) {
        throw new Error(`HTTP error! Status: ${res.status}`);
      }

      const result = await res.json();
      console.log('Success:', result);
    } catch (error) {
      console.error(error);
    }
  }

  const handleLogout = async () => {
    try {
      const res = await fetch('/api/auth/logout', {
            method: 'GET',
            credentials: 'include',
          },
      );

      if (!res.ok) {
        throw new Error(`HTTP error! Status: ${res.status}`);
      }

      const result = await res.json();
      console.log('Success:', result);
    } catch (error) {
      console.error(error);
    }
  }
  return (
      <div className="flex gap-4">
        <button className="btn" onClick={handleGoogleLogin}>
          Login with Google
        </button>
        <button className="btn" onClick={handleCheckMyInfo}>Check my info</button>
        <button className="btn btn-error" onClick={handleLogout}>Logout</button>
      </div>

  )
}
