import { Link } from '@tanstack/react-router'
import { ClipboardList, LogOut, Table2, Users } from 'lucide-react'
import { cn } from 'cn'
import chememanLogo from '@/assets/chememan-logo.svg'
import { Button } from '@/components/ui/button'
import { useLogout } from '@/features/auth/api'
import { useAuthStore, useIsAdmin } from '@/features/auth/store'

const navLinkClassName =
  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-foreground/70 transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
const navLinkActiveClassName = 'bg-sidebar-primary text-sidebar-primary-foreground hover:bg-sidebar-primary hover:text-sidebar-primary-foreground'

export function AppShell({ children }: { children: React.ReactNode }) {
  const isAdmin = useIsAdmin()
  const user = useAuthStore((state) => state.user)
  const logout = useLogout()

  return (
    <div className="flex min-h-svh bg-background">
      <aside className="hidden w-64 shrink-0 flex-col gap-6 border-r border-sidebar-border bg-sidebar p-5 text-sidebar-foreground md:flex">
        <img src={chememanLogo} alt="Chememan" className="h-8 w-auto" />

        <nav className="flex flex-1 flex-col gap-1">
          <div className="px-3 pb-1 text-[11px] font-semibold tracking-wide text-sidebar-foreground/50 uppercase">
            Main menu
          </div>
          <Link to="/" activeOptions={{ exact: true }} className={navLinkClassName} activeProps={{ className: cn(navLinkClassName, navLinkActiveClassName) }}>
            <Users className="size-[18px]" />
            Employees
          </Link>
          <Link to="/departments" className={navLinkClassName} activeProps={{ className: cn(navLinkClassName, navLinkActiveClassName) }}>
            <Table2 className="size-[18px]" />
            Departments
          </Link>
          {isAdmin && (
            <Link to="/audit-log" className={navLinkClassName} activeProps={{ className: cn(navLinkClassName, navLinkActiveClassName) }}>
              <ClipboardList className="size-[18px]" />
              Audit Log
              <span className="ml-auto rounded-full bg-[#3a332f] px-2 py-0.5 text-[10px] font-bold text-[#c9a876]">
                Admin
              </span>
            </Link>
          )}
        </nav>

        <div className="flex flex-col gap-3">
          <div className="h-px bg-sidebar-border" />
          {user && (
            <div className="flex items-center gap-2.5 px-1">
              <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-sidebar-primary text-xs font-bold text-sidebar-primary-foreground">
                {user.username.slice(0, 2).toUpperCase()}
              </div>
              <div className="flex min-w-0 flex-col leading-tight">
                <span className="truncate text-sm font-medium text-sidebar-foreground">{user.username}</span>
                <span className="text-xs text-sidebar-foreground/50">{user.role}</span>
              </div>
            </div>
          )}
          <Button
            variant="outline"
            className="justify-start border-sidebar-border bg-transparent text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
            onClick={() => logout.mutate()}
          >
            <LogOut className="size-4" />
            Log out
          </Button>
        </div>
      </aside>

      <main className="min-w-0 flex-1">{children}</main>
    </div>
  )
}
