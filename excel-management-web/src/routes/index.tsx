import { createFileRoute } from '@tanstack/react-router'
import { requireAuth } from '@/features/auth/guards'
import { EmployeeListPage } from '@/features/employees/EmployeeListPage'

export const Route = createFileRoute('/')({
  beforeLoad: requireAuth,
  component: EmployeeListPage,
})
