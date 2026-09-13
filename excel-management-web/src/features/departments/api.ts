import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'
import { throwOnMutationError } from '@/lib/api/mutationError'

export function useDepartments(includeInactive = false) {
  return useQuery({
    queryKey: ['departments', includeInactive],
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/departments', {
        params: { query: { includeInactive } },
      })
      if (error) {
        throw new Error('Failed to load departments')
      }
      return data
    },
  })
}

export function useCreateDepartment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const { data, error, response } = await apiClient.POST('/departments', { body: { name } })
      throwOnMutationError(error, response.status, 'Failed to create department')
      return data
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['departments'] }),
  })
}

export function useUpdateDepartment() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, name, isActive }: { id: number; name: string; isActive: boolean }) => {
      const { data, error, response } = await apiClient.PUT('/departments/{id}', {
        params: { path: { id } },
        body: { name, isActive },
      })
      throwOnMutationError(error, response.status, 'Failed to update department')
      return data
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['departments'] }),
  })
}
