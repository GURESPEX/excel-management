import { create } from 'zustand'

export type Role = 'Admin' | 'Viewer'

export interface AuthUser {
  id: number
  username: string
  role: Role
}

interface AuthState {
  user: AuthUser | null
  status: 'loading' | 'authenticated' | 'unauthenticated'
  setUser: (user: AuthUser) => void
  clear: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  status: 'loading',
  setUser: (user) => set({ user, status: 'authenticated' }),
  clear: () => set({ user: null, status: 'unauthenticated' }),
}))

export const useIsAdmin = () => useAuthStore((state) => state.user?.role === 'Admin')
