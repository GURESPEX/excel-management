import * as React from "react"
import { Calendar as CalendarIcon } from "lucide-react"
import { cn } from "cn"
import { Calendar } from "./calendar"
import { Input } from "./input"
import { Popover, PopoverContent, PopoverTrigger } from "./popover"

function parseDateValue(value: string): Date | undefined {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value)
  if (!match) return undefined
  const [, year, month, day] = match
  const date = new Date(Number(year), Number(month) - 1, Number(day))
  return Number.isNaN(date.getTime()) ? undefined : date
}

function formatDateValue(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const day = String(date.getDate()).padStart(2, "0")
  return `${year}-${month}-${day}`
}

export interface DatePickerProps {
  id?: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
  className?: string
}

function DatePicker({ id, value, onChange, placeholder = "YYYY-MM-DD", className }: DatePickerProps) {
  const [open, setOpen] = React.useState(false)

  return (
    <div className={cn("relative", className)}>
      <Input
        id={id}
        type="text"
        inputMode="numeric"
        placeholder={placeholder}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="pr-8"
      />
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger
          aria-label="Open calendar"
          className="absolute inset-y-0 right-0 flex w-8 items-center justify-center text-muted-foreground transition-colors hover:text-foreground"
        >
          <CalendarIcon className="size-4" />
        </PopoverTrigger>
        <PopoverContent align="end">
          <Calendar
            selected={parseDateValue(value)}
            onSelect={(date) => {
              onChange(formatDateValue(date))
              setOpen(false)
            }}
          />
        </PopoverContent>
      </Popover>
    </div>
  )
}

export { DatePicker }
