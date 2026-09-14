import { Plus } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useIsAdmin } from '@/features/auth/store'
import { useCreateDepartment, useDepartments, useUpdateDepartment } from './api'

interface DepartmentRow {
  id?: number
  name?: string | null
  isActive?: boolean
}

function DepartmentTableRow({ department }: { department: DepartmentRow }) {
  const [name, setName] = useState(department.name ?? '')
  const [error, setError] = useState<string | null>(null)
  const updateDepartment = useUpdateDepartment()
  const isAdmin = useIsAdmin()

  // Resync the edit buffer with the server value after a refetch (e.g.
  // another admin renamed this department), so a stale local value can't be
  // silently persisted the next time this row saves.
  useEffect(() => {
    setName(department.name ?? '')
  }, [department.name])

  const save = async (nextName: string, nextIsActive: boolean) => {
    setError(null)
    try {
      await updateDepartment.mutateAsync({ id: department.id!, name: nextName, isActive: nextIsActive })
    } catch {
      setError('Failed to save department.')
    }
  }

  return (
    <TableRow>
      <TableCell>
        <Input
          aria-label={`Name for ${department.name}`}
          value={name}
          disabled={!isAdmin}
          onChange={(event) => setName(event.target.value)}
        />
        {error && <p className="text-sm text-destructive">{error}</p>}
      </TableCell>
      <TableCell>
        <Badge variant={department.isActive ? 'default' : 'secondary'}>
          {department.isActive ? 'Active' : 'Disabled'}
        </Badge>
      </TableCell>
      <TableCell className="flex items-center gap-3">
        {isAdmin && (
          <>
            <Button size="sm" onClick={() => save(name, department.isActive ?? true)}>
              Save
            </Button>
            <div className="flex items-center gap-2">
              <Checkbox
                id={`active-${department.id}`}
                checked={department.isActive ?? true}
                onCheckedChange={(checked) => save(department.name ?? '', checked === true)}
              />
              <Label htmlFor={`active-${department.id}`}>Active</Label>
            </div>
          </>
        )}
      </TableCell>
    </TableRow>
  )
}

export function DepartmentAdminPage() {
  const { data: departments, isLoading } = useDepartments(true)
  const createDepartment = useCreateDepartment()
  const [newName, setNewName] = useState('')
  const [createError, setCreateError] = useState<string | null>(null)
  const isAdmin = useIsAdmin()

  const handleCreate = async () => {
    setCreateError(null)
    try {
      await createDepartment.mutateAsync(newName)
      setNewName('')
    } catch {
      setCreateError('Failed to create department. The name may already be in use.')
    }
  }

  if (isLoading) {
    return <p className="p-6">Loading departments...</p>
  }

  return (
    <div className="flex flex-col gap-6 p-6 md:p-10">
      <div>
        <h1 className="text-2xl font-semibold">Departments</h1>
        <p className="mt-1 text-sm text-muted-foreground">Manage the list of departments and their status.</p>
      </div>

      {isAdmin && (
        <Card className="p-6">
          <div className="mb-3 text-sm font-semibold">Add a new department</div>
          <div className="flex items-end gap-2">
            <div className="flex max-w-sm flex-1 flex-col gap-1">
              <Label htmlFor="new-department-name">Name</Label>
              <Input
                id="new-department-name"
                value={newName}
                onChange={(event) => setNewName(event.target.value)}
              />
            </div>
            <Button onClick={handleCreate}>
              <Plus /> Add Department
            </Button>
          </div>
          {createError && <p className="mt-3 text-sm text-destructive">{createError}</p>}
        </Card>
      )}

      <Card className="overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {departments?.map((department) => (
              <DepartmentTableRow key={department.id} department={department} />
            ))}
          </TableBody>
        </Table>
      </Card>
    </div>
  )
}
