import { createFileRoute } from '@tanstack/react-router'
import { z } from 'zod'
import { requireAuth } from '@/features/auth/guards'
import { EmployeeListPage } from '@/features/employees/EmployeeListPage'

// Booleans can arrive from the URL as either an actual boolean (the router's
// own search-param codec) or the literal string "true"/"false" (a manually
// typed/pasted URL), so normalize before validating.
const booleanSearchParam = z.preprocess(
  (value) => (typeof value === 'string' ? value === 'true' : value),
  z.boolean().optional(),
)

const employeeSearchSchema = z.object({
  name: z.string().optional(),
  departmentId: z.coerce.number().optional(),
  isActive: booleanSearchParam,
  minSalary: z.coerce.number().optional(),
  maxSalary: z.coerce.number().optional(),
  joinDateFrom: z.string().optional(),
  joinDateTo: z.string().optional(),
})

export const Route = createFileRoute('/')({
  beforeLoad: requireAuth,
  validateSearch: employeeSearchSchema,
  component: EmployeeListPage,
})
