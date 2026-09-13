import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useState } from 'react'
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
    <form onSubmit={handleSubmit} className="mx-auto flex max-w-sm flex-col gap-4 p-6">
      <h1 className="text-2xl font-semibold">Sign in</h1>

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
  )
}
