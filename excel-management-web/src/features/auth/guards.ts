import { redirect } from '@tanstack/react-router'
import { useAuthStore } from './store'

export function requireAuth() {
  if (useAuthStore.getState().status !== 'authenticated') {
    throw redirect({ to: '/login' })
  }
}

export function requireAdmin() {
  requireAuth()
  if (useAuthStore.getState().user?.role !== 'Admin') {
    throw redirect({ to: '/' })
  }
}
