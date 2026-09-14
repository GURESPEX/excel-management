import * as React from "react"
import { ChevronLeft, ChevronRight } from "lucide-react"
import { cn } from "cn"

const WEEKDAYS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"]

function startOfMonth(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

function isSameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

export interface CalendarProps {
  selected?: Date
  onSelect: (date: Date) => void
  className?: string
}

function Calendar({ selected, onSelect, className }: CalendarProps) {
  const [month, setMonth] = React.useState(() => startOfMonth(selected ?? new Date()))

  // Re-center on the selected date when it changes from outside (e.g. typed
  // into the paired text input), without fighting the user's own prev/next
  // navigation on every re-render — hence keying off the primitive time value
  // rather than the (newly constructed, always-different) `selected` object.
  const selectedTime = selected?.getTime()
  React.useEffect(() => {
    if (selectedTime !== undefined) {
      setMonth(startOfMonth(new Date(selectedTime)))
    }
  }, [selectedTime])

  const today = new Date()
  const firstWeekday = month.getDay()
  const daysInMonth = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate()
  const daysInPrevMonth = new Date(month.getFullYear(), month.getMonth(), 0).getDate()

  const cells: { date: Date; outside: boolean }[] = []
  for (let i = firstWeekday - 1; i >= 0; i--) {
    cells.push({ date: new Date(month.getFullYear(), month.getMonth() - 1, daysInPrevMonth - i), outside: true })
  }
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ date: new Date(month.getFullYear(), month.getMonth(), d), outside: false })
  }
  let trailing = 1
  while (cells.length < 42) {
    cells.push({ date: new Date(month.getFullYear(), month.getMonth() + 1, trailing), outside: true })
    trailing += 1
  }

  return (
    <div className={cn("w-64", className)}>
      <div className="mb-2 flex items-center justify-between">
        <button
          type="button"
          aria-label="Previous month"
          onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() - 1, 1))}
          className="flex size-7 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <ChevronLeft className="size-4" />
        </button>
        <div className="text-sm font-semibold">
          {month.toLocaleDateString(undefined, { month: "long", year: "numeric" })}
        </div>
        <button
          type="button"
          aria-label="Next month"
          onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1))}
          className="flex size-7 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <ChevronRight className="size-4" />
        </button>
      </div>

      <div className="grid grid-cols-7 gap-1 text-center text-xs text-muted-foreground">
        {WEEKDAYS.map((day) => (
          <div key={day} className="flex h-7 items-center justify-center">
            {day}
          </div>
        ))}
      </div>

      <div className="grid grid-cols-7 gap-1">
        {cells.map(({ date, outside }) => {
          const isSelected = selected ? isSameDay(date, selected) : false
          const isToday = isSameDay(date, today)
          return (
            <button
              key={date.toISOString()}
              type="button"
              tabIndex={outside ? -1 : 0}
              onClick={() => onSelect(date)}
              className={cn(
                "flex size-7 items-center justify-center rounded-md text-sm transition-colors",
                outside && "text-muted-foreground/40 hover:bg-transparent",
                !outside && "text-foreground hover:bg-accent hover:text-accent-foreground",
                isToday && !isSelected && "font-semibold text-primary",
                isSelected && "bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground"
              )}
            >
              {date.getDate()}
            </button>
          )
        })}
      </div>
    </div>
  )
}

export { Calendar }
