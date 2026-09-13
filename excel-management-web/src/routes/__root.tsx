import { createRootRoute, Outlet } from '@tanstack/react-router'
import { bootstrapAuth } from '@/features/auth/api'
import { useAuthStore } from '@/features/auth/store'

export const Route = createRootRoute({
  beforeLoad: async () => {
    if (useAuthStore.getState().status === 'loading') {
      await bootstrapAuth()
    }
  },
  component: () => <Outlet />,
})
