import { useMutation } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'
import { type AuthUser, type Role, useAuthStore } from './store'

// The backend always returns a fully-populated AuthenticatedUserDto on 200;
// its generated OpenAPI type marks fields optional only because Swashbuckle
// doesn't infer non-nullability from C# records, so it's safe to assert here.
function toAuthUser(dto: { id?: number; username?: string | null; role?: string | null }): AuthUser {
  return { id: dto.id!, username: dto.username!, role: dto.role as Role }
}

export function useLogin() {
  return useMutation({
    mutationFn: async (input: { username: string; password: string }) => {
      const { data, error } = await apiClient.POST('/auth/login', { body: input })
      if (error || !data) {
        throw new Error('Invalid username or password')
      }
      return toAuthUser(data)
    },
    onSuccess: (user) => useAuthStore.getState().setUser(user),
  })
}

export function useLogout() {
  return useMutation({
    mutationFn: async () => {
      await apiClient.POST('/auth/logout')
    },
    onSuccess: () => useAuthStore.getState().clear(),
  })
}

export async function bootstrapAuth(): Promise<void> {
  const { data, error } = await apiClient.POST('/auth/refresh')
  if (error || !data) {
    useAuthStore.getState().clear()
    return
  }
  useAuthStore.getState().setUser(toAuthUser(data))
}
