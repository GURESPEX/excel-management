import createClient from 'openapi-fetch'
import type { paths } from './schema'

export const apiClient = createClient<paths>({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5289',
  fetch: (...args: Parameters<typeof fetch>) => fetch(...args),
})
