import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'

export function useEmployees(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['employees', page, pageSize],
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/employees', {
        params: { query: { page, pageSize } },
      })
      if (error) {
        throw new Error('Failed to load employees')
      }
      return data
    },
  })
}
