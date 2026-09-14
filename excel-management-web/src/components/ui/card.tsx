import { cn } from "cn"

function Card({ className, ...props }: React.ComponentProps<"div">) {
  return (
    <div
      data-slot="card"
      className={cn("rounded-xl border bg-card shadow-sm", className)}
      {...props}
    />
  )
}

export { Card }
