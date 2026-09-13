import { Link } from '@tanstack/react-router'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useLogout } from '@/features/auth/api'
import { useIsAdmin } from '@/features/auth/store'
import { DeleteEmployeeDialog } from './DeleteEmployeeDialog'
import { useEmployees } from './api'

export function EmployeeListPage() {
  const { data, isLoading, isError } = useEmployees()
  const isAdmin = useIsAdmin()
  const logout = useLogout()

  if (isLoading) {
    return <p className="p-6">Loading employees...</p>
  }

  if (isError || !data) {
    return <p className="p-6 text-destructive">Failed to load employees.</p>
  }

  return (
    <div className="p-6">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Employees</h1>
        <div className="flex gap-2">
          <Link to="/departments" className={buttonVariants({ variant: 'outline' })}>
            Departments
          </Link>
          {isAdmin && (
            <Link to="/employees/new" className={buttonVariants({})}>
              New Employee
            </Link>
          )}
          <Button variant="outline" onClick={() => logout.mutate()}>
            Log out
          </Button>
        </div>
      </div>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Name</TableHead>
            <TableHead>Department</TableHead>
            <TableHead>Salary</TableHead>
            <TableHead>Join Date</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.items?.map((employee) => (
            <TableRow key={employee.id}>
              <TableCell>{employee.id}</TableCell>
              <TableCell>{employee.name}</TableCell>
              <TableCell>{employee.departmentName}</TableCell>
              <TableCell>
                {employee.salary?.toLocaleString(undefined, {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                })}
              </TableCell>
              <TableCell>{employee.joinDate}</TableCell>
              <TableCell>
                <Badge variant={employee.isActive ? 'default' : 'secondary'}>
                  {employee.isActive ? 'Active' : 'Inactive'}
                </Badge>
              </TableCell>
              <TableCell className="flex justify-end gap-2">
                {isAdmin && (
                  <>
                    <Link
                      to="/employees/$employeeId"
                      params={{ employeeId: String(employee.id) }}
                      className={buttonVariants({ variant: 'outline', size: 'sm' })}
                    >
                      Edit
                    </Link>
                    <DeleteEmployeeDialog employeeId={employee.id!} employeeName={employee.name ?? 'this employee'} />
                  </>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
