import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
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

export interface EmployeeFilters {
  page?: number
  pageSize?: number
  name?: string
  departmentId?: number
  isActive?: boolean
  minSalary?: number
  maxSalary?: number
  joinDateFrom?: string
  joinDateTo?: string
}

export function useEmployees(filters: EmployeeFilters = {}) {
  return useQuery({
    queryKey: ['employees', filters],
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/employees', {
        params: {
          query: {
            Page: filters.page ?? 1,
            PageSize: filters.pageSize ?? 20,
            Name: filters.name,
            DepartmentId: filters.departmentId,
            IsActive: filters.isActive,
            MinSalary: filters.minSalary,
            MaxSalary: filters.maxSalary,
            JoinDateFrom: filters.joinDateFrom,
            JoinDateTo: filters.joinDateTo,
          },
        },
      })
      if (error) {
        throw new Error('Failed to load employees')
      }
      return data
    },
    // Keep showing the previous filter's results while a new filter's query
    // is in flight, instead of flashing the whole page back to a loading
    // state on every keystroke (which would also unmount the filter inputs
    // themselves, dropping focus mid-type).
    placeholderData: keepPreviousData,
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
