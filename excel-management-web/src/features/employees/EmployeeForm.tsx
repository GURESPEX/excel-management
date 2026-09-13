import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { type EmployeeFormValues, useDepartments, ValidationError } from './api'

const employeeFormSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  departmentId: z.number().int().positive('Department is required'),
  salary: z.number().min(0, 'Salary must be non-negative'),
  joinDate: z.string().min(1, 'Join date is required'),
  isActive: z.boolean(),
}) satisfies z.ZodType<EmployeeFormValues>

function toFieldName(apiFieldName: string): keyof EmployeeFormValues {
  return (apiFieldName.charAt(0).toLowerCase() + apiFieldName.slice(1)) as keyof EmployeeFormValues
}

export function EmployeeForm({
  defaultValues,
  onSubmit,
  submitLabel,
}: {
  defaultValues: EmployeeFormValues
  onSubmit: (values: EmployeeFormValues) => Promise<void>
  submitLabel: string
}) {
  const { data: departments } = useDepartments()
  const form = useForm<EmployeeFormValues>({
    resolver: zodResolver(employeeFormSchema),
    defaultValues,
  })

  const handleSubmit = form.handleSubmit(async (values) => {
    try {
      await onSubmit(values)
    } catch (err) {
      if (err instanceof ValidationError) {
        for (const [field, messages] of Object.entries(err.fieldErrors)) {
          form.setError(toFieldName(field), { message: messages[0] })
        }
        return
      }
      form.setError('root', { message: 'Something went wrong. Please try again.' })
    }
  })

  return (
    <form onSubmit={handleSubmit} className="flex max-w-md flex-col gap-4 p-6">
      <div className="flex flex-col gap-1">
        <Label htmlFor="name">Name</Label>
        <Input id="name" {...form.register('name')} />
        {form.formState.errors.name && (
          <p className="text-sm text-destructive">{form.formState.errors.name.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="departmentId">Department</Label>
        <Select
          items={Object.fromEntries((departments ?? []).map((department) => [String(department.id), department.name]))}
          value={form.watch('departmentId') ? String(form.watch('departmentId')) : undefined}
          onValueChange={(value) => form.setValue('departmentId', Number(value), { shouldValidate: true })}
        >
          <SelectTrigger id="departmentId">
            <SelectValue placeholder="Select a department" />
          </SelectTrigger>
          <SelectContent>
            {departments?.map((department) => (
              <SelectItem key={department.id} value={String(department.id)}>
                {department.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {form.formState.errors.departmentId && (
          <p className="text-sm text-destructive">{form.formState.errors.departmentId.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="salary">Salary</Label>
        <Input id="salary" type="number" step="0.01" {...form.register('salary', { valueAsNumber: true })} />
        {form.formState.errors.salary && (
          <p className="text-sm text-destructive">{form.formState.errors.salary.message}</p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <Label htmlFor="joinDate">Join Date</Label>
        <Input id="joinDate" type="date" {...form.register('joinDate')} />
        {form.formState.errors.joinDate && (
          <p className="text-sm text-destructive">{form.formState.errors.joinDate.message}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <Checkbox
          id="isActive"
          checked={form.watch('isActive')}
          onCheckedChange={(checked) => form.setValue('isActive', checked === true)}
        />
        <Label htmlFor="isActive">Active</Label>
      </div>

      {form.formState.errors.root && (
        <p className="text-sm text-destructive">{form.formState.errors.root.message}</p>
      )}

      <Button type="submit" disabled={form.formState.isSubmitting}>
        {submitLabel}
      </Button>
    </form>
  )
}
