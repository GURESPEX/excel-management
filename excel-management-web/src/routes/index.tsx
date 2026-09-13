import { createFileRoute } from '@tanstack/react-router'
import { EmployeeListPage } from '@/features/employees/EmployeeListPage'

export const Route = createFileRoute('/')({
  component: EmployeeListPage,
})
