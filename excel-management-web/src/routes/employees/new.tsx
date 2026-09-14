import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { AppShell } from '@/components/layout/AppShell'
import { requireAdmin } from '@/features/auth/guards'
import { EmployeeForm } from '@/features/employees/EmployeeForm'
import { useCreateEmployee } from '@/features/employees/api'

export const Route = createFileRoute('/employees/new')({
  beforeLoad: requireAdmin,
  component: NewEmployeePage,
})

function NewEmployeePage() {
  const navigate = useNavigate()
  const createEmployee = useCreateEmployee()

  return (
    <AppShell>
      <div className="flex flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-2xl font-semibold">New Employee</h1>
          <p className="mt-1 text-sm text-muted-foreground">Add a new employee record.</p>
        </div>
        <EmployeeForm
          submitLabel="Create"
          defaultValues={{ name: '', departmentId: 0, salary: 0, joinDate: '', isActive: true }}
          onSubmit={async (values) => {
            await createEmployee.mutateAsync(values)
            await navigate({ to: '/' })
          }}
        />
      </div>
    </AppShell>
  )
}
