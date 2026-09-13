import { createFileRoute } from '@tanstack/react-router'
import { requireAuth } from '@/features/auth/guards'
import { DepartmentAdminPage } from '@/features/departments/DepartmentAdminPage'

export const Route = createFileRoute('/departments')({
  beforeLoad: requireAuth,
  component: DepartmentAdminPage,
})
