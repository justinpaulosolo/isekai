import {queryOptions, useQuery, useQueryClient} from "@tanstack/react-query";

export type User = { id: string; name: string; email: string; picture?: string }

export const meQueryOptions = queryOptions({
    queryKey: ['me'],
    queryFn: async (): Promise<User | null> => {
        const res = await fetch('/api/auth/me');
        if (res.status === 401 || res.status === 403) return null;
        if (!res.ok) throw new Error(`Failed to load user: ${res.status}`);
        return res.json();
    },
    staleTime: 5*60*1000,
    retry: false,
})

export function useAuth() {
    const qc = useQueryClient()
    const { data: user, isLoading } = useQuery(meQueryOptions)

    const login = (returnUrl = window.location.pathname + window.location.search) => {
        window.location.href = `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
    }

    const logout = async () => {
        await fetch('/api/auth/logout', { method: 'POST'})
        qc.setQueryData(meQueryOptions.queryKey, null)
        window.location.href = "/login"
    }

    return { user: user ?? null, isLoading, isAuthenticated: !!user, login, logout}
}

export async function fetchMe(): Promise<User | null> {
    const res = await fetch('/api/auth/me')
    if (res.status === 401) return null
    if (!res.ok) throw new Error(`Failed to load user: ${res.status}`)
    return res.json()
}

export async function logout() {
    await fetch('/api/auth/logout', { method: 'POST' })
    window.location.href = '/login'
}