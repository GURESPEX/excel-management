import { screen } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { renderApp } from '@/test/renderApp'
import { server } from '@/test/server'

describe('auth flows', () => {
  it('redirects an unauthenticated visitor to the login page', async () => {
    server.use(http.post('http://localhost:5289/auth/refresh', () => HttpResponse.json(null, { status: 401 })))

    await renderApp('/')

    expect(await screen.findByRole('heading', { name: /sign in/i })).toBeInTheDocument()
  })

  it('hides create/edit/delete controls for a Viewer-role session', async () => {
    server.use(
      http.post('http://localhost:5289/auth/refresh', () =>
        HttpResponse.json({ id: 2, username: 'viewer', role: 'Viewer' }),
      ),
      http.get('http://localhost:5289/employees', () =>
        HttpResponse.json({
          items: [{ id: 1, name: 'Someone', departmentName: 'Engineering', salary: 1000, joinDate: '2024-01-01', isActive: true }],
          page: 1,
          pageSize: 20,
          totalCount: 1,
        }),
      ),
    )

    await renderApp('/')

    expect(await screen.findByRole('heading', { name: /employees/i })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /new employee/i })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /edit/i })).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /delete/i })).not.toBeInTheDocument()
  })
})
