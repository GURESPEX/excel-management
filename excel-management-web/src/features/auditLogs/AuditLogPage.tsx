import { useState } from 'react'
import { Link } from '@tanstack/react-router'
import { Filter } from 'lucide-react'
import { buttonVariants } from '@/components/ui/button'
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
import { useAuditLogs } from './api'

const ENTITY_TYPES = { '': 'All', Employee: 'Employee', Department: 'Department' }

export function AuditLogPage() {
  const [entityName, setEntityName] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const { data, isLoading, isError } = useAuditLogs({
    entityName: entityName || undefined,
    // Date inputs give a bare "YYYY-MM-DD"; widen to end-of-day for "to" so
    // that day's own entries aren't excluded by the time-of-day comparison.
    from: from ? new Date(from).toISOString() : undefined,
    to: to ? new Date(`${to}T23:59:59.999Z`).toISOString() : undefined,
  })

  return (
    <div className="flex flex-col gap-6 p-6 md:p-10">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Audit Log</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Every change made to employee and department records.
          </p>
        </div>
        <Link to="/" className={buttonVariants({ variant: 'outline' })}>
          Back to Employees
        </Link>
      </div>

      <Card className="p-6">
        <div className="mb-4 flex items-center gap-2 text-sm font-semibold">
          <Filter className="size-4" />
          Filters
        </div>
        <div className="flex flex-wrap items-end gap-3">
          <div className="flex flex-col gap-1">
            <Label htmlFor="filter-entity">Entity Type</Label>
            <Select items={ENTITY_TYPES} value={entityName} onValueChange={(value) => setEntityName(value ?? '')}>
              <SelectTrigger id="filter-entity">
                <SelectValue placeholder="All" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">All</SelectItem>
                <SelectItem value="Employee">Employee</SelectItem>
                <SelectItem value="Department">Department</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex flex-col gap-1">
            <Label htmlFor="filter-from">From</Label>
            <DatePicker id="filter-from" value={from} onChange={setFrom} />
          </div>

          <div className="flex flex-col gap-1">
            <Label htmlFor="filter-to">To</Label>
            <DatePicker id="filter-to" value={to} onChange={setTo} />
          </div>
        </div>
      </Card>

      {isLoading && <p>Loading audit log...</p>}
      {isError && <p className="text-destructive">Failed to load audit log.</p>}

      {data && (
        <Card className="overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Timestamp</TableHead>
                <TableHead>Entity</TableHead>
                <TableHead>Entity Id</TableHead>
                <TableHead>Action</TableHead>
                <TableHead>Changed By</TableHead>
                <TableHead>Changes</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.map((entry) => (
                <TableRow key={entry.id}>
                  <TableCell>{new Date(entry.timestamp!).toLocaleString()}</TableCell>
                  <TableCell>{entry.entityName}</TableCell>
                  <TableCell>{entry.entityId}</TableCell>
                  <TableCell>{entry.action}</TableCell>
                  <TableCell>{entry.changedBy ?? 'system'}</TableCell>
                  <TableCell className="max-w-md truncate font-mono text-xs" title={entry.changes ?? ''}>
                    {entry.changes}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}
    </div>
  )
}
