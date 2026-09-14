import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api/client'

export interface AuditLogFilters {
  entityName?: string
  from?: string
  to?: string
}

export function useAuditLogs(filters: AuditLogFilters = {}) {
  return useQuery({
    queryKey: ['audit-logs', filters],
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/audit-logs', {
        params: {
          query: {
            EntityName: filters.entityName,
            From: filters.from,
            To: filters.to,
          },
        },
      })
      if (error) {
        throw new Error('Failed to load audit logs')
      }
      return data
    },
  })
}
