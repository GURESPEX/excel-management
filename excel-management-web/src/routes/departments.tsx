import { createFileRoute } from '@tanstack/react-router'
import { DepartmentAdminPage } from '@/features/departments/DepartmentAdminPage'

export const Route = createFileRoute('/departments')({
  component: DepartmentAdminPage,
})
