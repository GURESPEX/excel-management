import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'
import { throwOnMutationError } from '@/lib/api/mutationError'
import type { components } from '@/lib/api/schema'

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

export type EmployeeImportResult = components['schemas']['EmployeeImportResult']

function exportQuery(filters: EmployeeFilters) {
  return {
    Page: filters.page ?? 1,
    PageSize: filters.pageSize ?? 20,
    Name: filters.name,
    DepartmentId: filters.departmentId,
    IsActive: filters.isActive,
    MinSalary: filters.minSalary,
    MaxSalary: filters.maxSalary,
    JoinDateFrom: filters.joinDateFrom,
    JoinDateTo: filters.joinDateTo,
  }
}

export async function exportEmployees(format: 'excel' | 'csv', filters: EmployeeFilters = {}) {
  const { data, response } = await apiClient.GET('/employees/export', {
    params: { query: { ...exportQuery(filters), format } },
    parseAs: 'blob',
  })
  if (!response.ok || !data) {
    throw new Error('Failed to export employees')
  }

  const url = URL.createObjectURL(data as Blob)
  const link = document.createElement('a')
  link.href = url
  link.download = format === 'excel' ? 'employees.xlsx' : 'employees.csv'
  link.click()
  URL.revokeObjectURL(url)
}

export function useImportEmployees() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (file: File) => {
      // openapi-fetch only auto-serializes a body that is already a FormData
      // instance (see defaultBodySerializer) — a plain `{ file }` object falls
      // through to JSON.stringify(), turning the File into `{}` and sending it
      // with a `application/json` content-type the server rejects with 415.
      const formData = new FormData()
      formData.append('file', file)
      const { data, error, response } = await apiClient.POST('/employees/import', {
        body: formData as unknown as { file: string },
      })

      // A 400 here is a row-by-row validation report, not an unexpected failure —
      // resolve with it so the UI can render every failing row instead of throwing.
      if (response.status === 400 && error) {
        return error as EmployeeImportResult
      }
      if (error || !data) {
        throw new Error('Failed to import employees')
      }
      return data
    },
    onSuccess: (result) => {
      if (!result.errors || result.errors.length === 0) {
        queryClient.invalidateQueries({ queryKey: ['employees'] })
      }
    },
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
