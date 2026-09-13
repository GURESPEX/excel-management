import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { EmployeeForm } from '@/features/employees/EmployeeForm'
import { useCreateEmployee } from '@/features/employees/api'

export const Route = createFileRoute('/employees/new')({
  component: NewEmployeePage,
})

function NewEmployeePage() {
  const navigate = useNavigate()
  const createEmployee = useCreateEmployee()

  return (
    <div>
      <h1 className="p-6 pb-0 text-2xl font-semibold">New Employee</h1>
      <EmployeeForm
        submitLabel="Create"
        defaultValues={{ name: '', departmentId: 0, salary: 0, joinDate: '', isActive: true }}
        onSubmit={async (values) => {
          await createEmployee.mutateAsync(values)
          await navigate({ to: '/' })
        }}
      />
    </div>
  )
}
