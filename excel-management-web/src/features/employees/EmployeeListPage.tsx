import { Link, useNavigate, useSearch } from '@tanstack/react-router'
import { Download, Filter, Pencil, Plus, Upload, UserCheck, Users, UserX } from 'lucide-react'
import { useRef, useState } from 'react'
import { Badge } from '@/components/ui/badge'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { DatePicker } from '@/components/ui/date-picker'
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
import { useIsAdmin } from '@/features/auth/store'
import { useDepartments } from '@/features/departments/api'
import { DeleteEmployeeDialog } from './DeleteEmployeeDialog'
import { exportEmployees, useEmployees, useImportEmployees } from './api'

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
    <div className="flex flex-wrap items-end gap-3">
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
        <DatePicker
          id="filter-join-from"
          value={search.joinDateFrom ?? ''}
          onChange={(value) => setFilter({ joinDateFrom: value || undefined })}
        />
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="filter-join-to">Join Date To</Label>
        <DatePicker
          id="filter-join-to"
          value={search.joinDateTo ?? ''}
          onChange={(value) => setFilter({ joinDateTo: value || undefined })}
        />
      </div>

      <Button variant="outline" onClick={() => navigate({ search: {} })}>
        Clear filters
      </Button>
    </div>
  )
}

function EmployeeImportExportControls() {
  const search = useSearch({ from: '/' })
  const fileInputRef = useRef<HTMLInputElement>(null)
  const importMutation = useImportEmployees()
  const [exportError, setExportError] = useState<string | null>(null)

  const handleExport = async (format: 'excel' | 'csv') => {
    setExportError(null)
    try {
      await exportEmployees(format, search)
    } catch {
      setExportError('Failed to export employees.')
    }
  }

  const handleFileChosen = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) {
      importMutation.mutate(file)
    }
  }

  const result = importMutation.data
  const hasErrors = !!result?.errors && result.errors.length > 0

  return (
    <div className="flex flex-col gap-2">
      <div className="flex flex-wrap gap-2">
        <Button variant="outline" onClick={() => handleExport('excel')}>
          <Download /> Export Excel
        </Button>
        <Button variant="outline" onClick={() => handleExport('csv')}>
          <Download /> Export CSV
        </Button>
        <Button variant="outline" onClick={() => fileInputRef.current?.click()} disabled={importMutation.isPending}>
          <Upload /> {importMutation.isPending ? 'Importing...' : 'Import Excel'}
        </Button>
        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx,.xls"
          className="hidden"
          onChange={handleFileChosen}
        />
      </div>

      {exportError && <p className="text-sm text-destructive">{exportError}</p>}

      {importMutation.isSuccess && result && !hasErrors && (
        <p className="text-sm text-muted-foreground">
          Imported {result.importedCount} employee(s) successfully.
        </p>
      )}

      {hasErrors && (
        <div className="rounded-md border border-destructive/50 p-3">
          <p className="mb-2 text-sm font-medium text-destructive">
            Import failed — no rows were saved. Fix the following and try again:
          </p>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Row</TableHead>
                <TableHead>Field</TableHead>
                <TableHead>Reason</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {result.errors!.map((error, index) => (
                <TableRow key={index}>
                  <TableCell>{error.row}</TableCell>
                  <TableCell>{error.field}</TableCell>
                  <TableCell>{error.reason}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  )
}

export function EmployeeListPage() {
  const search = useSearch({ from: '/' })
  const { data, isLoading, isError } = useEmployees(search)
  const isAdmin = useIsAdmin()

  if (isLoading) {
    return <p className="p-6">Loading employees...</p>
  }

  if (isError || !data) {
    return <p className="p-6 text-destructive">Failed to load employees.</p>
  }

  const employees = data.items ?? []
  const activeCount = employees.filter((employee) => employee.isActive).length

  return (
    <div className="flex flex-col gap-6 p-6 md:p-10">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Employees</h1>
          <p className="mt-1 text-sm text-muted-foreground">Manage every employee record for Chememan.</p>
        </div>
        {isAdmin && (
          <Link to="/employees/new" className={buttonVariants({})}>
            <Plus /> New Employee
          </Link>
        )}
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="flex items-center gap-3 p-4">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
            <Users className="size-5" />
          </div>
          <div>
            <div className="text-xl leading-none font-semibold">{employees.length}</div>
            <div className="mt-1 text-xs text-muted-foreground">Total employees</div>
          </div>
        </Card>
        <Card className="flex items-center gap-3 p-4">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <UserCheck className="size-5" />
          </div>
          <div>
            <div className="text-xl leading-none font-semibold">{activeCount}</div>
            <div className="mt-1 text-xs text-muted-foreground">Active employees</div>
          </div>
        </Card>
        <Card className="flex items-center gap-3 p-4">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
            <UserX className="size-5" />
          </div>
          <div>
            <div className="text-xl leading-none font-semibold">{employees.length - activeCount}</div>
            <div className="mt-1 text-xs text-muted-foreground">Inactive employees</div>
          </div>
        </Card>
      </div>

      <Card className="p-6">
        <div className="mb-4 flex items-center gap-2 text-sm font-semibold">
          <Filter className="size-4" />
          Filters
        </div>
        <EmployeeFilterPanel />
      </Card>

      {isAdmin && <EmployeeImportExportControls />}

      <Card className="overflow-hidden">
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
                        <Pencil /> Edit
                      </Link>
                      <DeleteEmployeeDialog employeeId={employee.id!} employeeName={employee.name ?? 'this employee'} />
                    </>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </div>
  )
}
