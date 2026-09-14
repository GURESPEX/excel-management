import { Link, useNavigate, useSearch } from '@tanstack/react-router'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
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
import { useDepartments } from '@/features/departments/api'
import { DeleteEmployeeDialog } from './DeleteEmployeeDialog'
import { useEmployees } from './api'

function EmployeeFilterPanel() {
  const search = useSearch({ from: '/' })
  const navigate = useNavigate({ from: '/' })
  // Include inactive departments so a filter on a since-disabled department
  // still resolves to a label instead of showing the raw id.
  const { data: departments, isLoading: departmentsLoading } = useDepartments(true)

  // replace: true so every keystroke doesn't push a new history entry —
  // otherwise "back" becomes unusable after typing into a filter.
  const setFilter = (patch: Partial<typeof search>) =>
    navigate({ search: (prev) => ({ ...prev, ...patch }), replace: true })

  // Mirrors the Select label-resolution gotcha fixed in EmployeeForm: the
  // trigger only resolves a non-empty value's label from `items` the first
  // time, so this must not mount until `items` (from `departments`) is ready.
  if (departmentsLoading) {
    return <p className="mb-4 text-sm text-muted-foreground">Loading filters...</p>
  }

  const departmentItems = Object.fromEntries((departments ?? []).map((d) => [String(d.id), d.name]))

  return (
    <div className="mb-4 flex flex-wrap items-end gap-3">
      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-name">Name</Label>
        <Input
          id="filter-name"
          value={search.name ?? ''}
          onChange={(event) => setFilter({ name: event.target.value || undefined })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-department">Department</Label>
        <Select
          items={{ '': 'All', ...departmentItems }}
          value={search.departmentId === undefined ? '' : String(search.departmentId)}
          onValueChange={(value) => setFilter({ departmentId: value ? Number(value) : undefined })}
        >
          <SelectTrigger id="filter-department">
            <SelectValue placeholder="All" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="">All</SelectItem>
            {departments?.map((department) => (
              <SelectItem key={department.id} value={String(department.id)}>
                {department.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-status">Status</Label>
        <Select
          items={{ '': 'All', true: 'Active', false: 'Inactive' }}
          value={search.isActive === undefined ? '' : String(search.isActive)}
          onValueChange={(value) => setFilter({ isActive: value === '' ? undefined : value === 'true' })}
        >
          <SelectTrigger id="filter-status">
            <SelectValue placeholder="All" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="">All</SelectItem>
            <SelectItem value="true">Active</SelectItem>
            <SelectItem value="false">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-min-salary">Min Salary</Label>
        <Input
          id="filter-min-salary"
          type="number"
          value={search.minSalary ?? ''}
          onChange={(event) => setFilter({ minSalary: event.target.value ? Number(event.target.value) : undefined })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-max-salary">Max Salary</Label>
        <Input
          id="filter-max-salary"
          type="number"
          value={search.maxSalary ?? ''}
          onChange={(event) => setFilter({ maxSalary: event.target.value ? Number(event.target.value) : undefined })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-join-from">Join Date From</Label>
        <Input
          id="filter-join-from"
          type="date"
          value={search.joinDateFrom ?? ''}
          onChange={(event) => setFilter({ joinDateFrom: event.target.value || undefined })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-join-to">Join Date To</Label>
        <Input
          id="filter-join-to"
          type="date"
          value={search.joinDateTo ?? ''}
          onChange={(event) => setFilter({ joinDateTo: event.target.value || undefined })}
        />
      </div>

      <Button variant="outline" onClick={() => navigate({ search: {} })}>
        Clear filters
      </Button>
    </div>
  )
}

export function EmployeeListPage() {
  const search = useSearch({ from: '/' })
  const { data, isLoading, isError } = useEmployees(search)
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
            <Link to="/audit-log" className={buttonVariants({ variant: 'outline' })}>
              Audit Log
            </Link>
          )}
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
      <EmployeeFilterPanel />
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
