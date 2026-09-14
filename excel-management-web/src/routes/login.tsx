import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useState } from 'react'
import chememanLogo from '@/assets/chememan-logo.svg'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useLogin } from '@/features/auth/api'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  const login = useLogin()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError(null)
    try {
      await login.mutateAsync({ username, password })
      await navigate({ to: '/' })
    } catch {
      setError('Invalid username or password.')
    }
  }

  return (
    <div className="flex min-h-svh">
      <div className="relative hidden w-[40%] min-w-105 flex-col justify-between overflow-hidden bg-sidebar p-10 text-sidebar-foreground md:flex">
        <div
          className="pointer-events-none absolute -top-40 -right-44 size-105 rounded-full opacity-30"
          style={{ background: 'radial-gradient(circle, #00522c 0%, transparent 70%)' }}
        />
        <img src={chememanLogo} alt="Chememan" className="relative h-10 w-auto" />
        <div className="relative max-w-md">
          <p className="mb-3 text-xs font-semibold tracking-wide text-primary/70 uppercase">
            Employee Management System
          </p>
          <h2 className="mb-5 text-3xl leading-tight font-bold text-white">Human Chemical for a Better Future</h2>
          <p className="text-sm leading-relaxed text-sidebar-foreground/60">
            Manage employees, departments, and every change to your company&apos;s data in one secure, fully
            auditable place.
          </p>
        </div>
        <p className="relative text-xs text-sidebar-foreground/40">© 2026 Chememan Public Company Limited.</p>
      </div>

      <div className="flex flex-1 items-center justify-center p-6">
        <form onSubmit={handleSubmit} className="flex w-full max-w-sm flex-col gap-4">
          <div>
            <h1 className="text-2xl font-semibold">Sign in</h1>
            <p className="mt-1 text-sm text-muted-foreground">Enter your credentials to access the system.</p>
          </div>

          <div className="flex flex-col gap-1">
            <Label htmlFor="username">Username</Label>
            <Input id="username" value={username} onChange={(event) => setUsername(event.target.value)} />
          </div>

          <div className="flex flex-col gap-1">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </div>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <Button type="submit" disabled={login.isPending}>
            Sign in
          </Button>
        </form>
      </div>
    </div>
  )
}
