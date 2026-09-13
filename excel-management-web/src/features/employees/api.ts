import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'
import { throwOnMutationError } from '@/lib/api/mutationError'

export interface EmployeeFormValues {
  name: string
  departmentId: number
  salary: number
  joinDate: string
  isActive: boolean
}

export { ValidationError } from '@/lib/api/mutationError'

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

export function useEmployee(id: number | undefined) {
  return useQuery({
    queryKey: ['employees', 'detail', id],
    enabled: id !== undefined,
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/employees/{id}', {
        params: { path: { id: id! } },
      })
      if (error) {
        throw new Error('Failed to load employee')
      }
      return data
    },
  })
}

export function useCreateEmployee() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (input: EmployeeFormValues) => {
      const { data, error, response } = await apiClient.POST('/employees', { body: input })
      throwOnMutationError(error, response.status, 'Failed to create employee')
      return data
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['employees'] }),
  })
}

export function useUpdateEmployee(id: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (input: EmployeeFormValues) => {
      const { error, response } = await apiClient.PUT('/employees/{id}', {
        params: { path: { id } },
        body: input,
      })
      throwOnMutationError(error, response.status, 'Failed to update employee')
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['employees'] }),
  })
}

export function useDeleteEmployee() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      const { error } = await apiClient.DELETE('/employees/{id}', {
        params: { path: { id } },
      })
      if (error) {
        throw new Error('Failed to delete employee')
      }
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['employees'] }),
  })
}
