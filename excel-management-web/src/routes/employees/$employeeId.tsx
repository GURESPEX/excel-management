import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { EmployeeForm } from '@/features/employees/EmployeeForm'
import { useEmployee, useUpdateEmployee } from '@/features/employees/api'

export const Route = createFileRoute('/employees/$employeeId')({
  component: EditEmployeePage,
})

function EditEmployeePage() {
  const { employeeId } = Route.useParams()
  const id = Number(employeeId)
  const navigate = useNavigate()
  const { data: employee, isLoading } = useEmployee(id)
  const updateEmployee = useUpdateEmployee(id)

  if (isLoading) {
    return <p className="p-6">Loading employee...</p>
  }

  if (!employee) {
    return <p className="p-6 text-destructive">Employee not found.</p>
  }

  return (
    <div>
      <h1 className="p-6 pb-0 text-2xl font-semibold">Edit Employee</h1>
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
  )
}
