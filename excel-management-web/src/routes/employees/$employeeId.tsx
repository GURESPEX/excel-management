import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { AppShell } from '@/components/layout/AppShell'
import { requireAdmin } from '@/features/auth/guards'
import { EmployeeForm } from '@/features/employees/EmployeeForm'
import { useEmployee, useUpdateEmployee } from '@/features/employees/api'

export const Route = createFileRoute('/employees/$employeeId')({
  beforeLoad: requireAdmin,
  component: EditEmployeePage,
})

function EditEmployeePage() {
  const { employeeId } = Route.useParams()
  const id = Number(employeeId)
  const navigate = useNavigate()
  const { data: employee, isLoading } = useEmployee(id)
  const updateEmployee = useUpdateEmployee(id)

  if (isLoading) {
    return (
      <AppShell>
        <p className="p-6">Loading employee...</p>
      </AppShell>
    )
  }

  if (!employee) {
    return (
      <AppShell>
        <p className="p-6 text-destructive">Employee not found.</p>
      </AppShell>
    )
  }

  return (
    <AppShell>
      <div className="flex flex-col gap-6 p-6 md:p-10">
        <div>
          <h1 className="text-2xl font-semibold">Edit Employee</h1>
          <p className="mt-1 text-sm text-muted-foreground">Update this employee&apos;s record.</p>
        </div>
        <EmployeeForm
          submitLabel="Save"
          defaultValues={{
            name: employee.name ?? '',
            departmentId: employee.departmentId ?? 0,
            salary: employee.salary ?? 0,
            joinDate: employee.joinDate ?? '',
            isActive: employee.isActive ?? true,
          }}
          onSubmit={async (values) => {
            await updateEmployee.mutateAsync(values)
            await navigate({ to: '/' })
          }}
        />
      </div>
    </AppShell>
  )
}
