import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { useAuthStore } from '@/features/auth/store'
import { server } from '@/test/server'
import { DepartmentAdminPage } from './DepartmentAdminPage'

function renderPage() {
  // This page is rendered directly (no router), so bootstrapAuth's beforeLoad
  // never runs — set the Admin session it needs to see edit/create controls.
  useAuthStore.setState({ user: { id: 1, username: 'admin', role: 'Admin' }, status: 'authenticated' })

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <DepartmentAdminPage />
    </QueryClientProvider>,
  )
}

describe('DepartmentAdminPage', () => {
  it('lists every department, including disabled ones', async () => {
    server.use(
      http.get('http://localhost:5289/departments', () =>
        HttpResponse.json([
          { id: 1, name: 'Engineering', isActive: true },
          { id: 2, name: 'Legacy Dept', isActive: false },
        ]),
      ),
    )

    renderPage()

    expect(await screen.findByDisplayValue('Engineering')).toBeInTheDocument()
    expect(screen.getByDisplayValue('Legacy Dept')).toBeInTheDocument()
    expect(screen.getByText('Disabled')).toBeInTheDocument()
  })

  it('renames a department and toggles it inactive', async () => {
    server.use(
      http.get('http://localhost:5289/departments', () =>
        HttpResponse.json([{ id: 1, name: 'Sales', isActive: true }]),
      ),
      http.put('http://localhost:5289/departments/1', async ({ request }) => {
        const body = (await request.json()) as { name: string; isActive: boolean }
        return HttpResponse.json({ id: 1, name: body.name, isActive: body.isActive })
      }),
    )

    const user = userEvent.setup()
    renderPage()

    const nameInput = await screen.findByLabelText('Name for Sales')
    await user.clear(nameInput)
    await user.type(nameInput, 'Sales & Marketing')
    await user.click(screen.getByRole('button', { name: /save/i }))

    await user.click(screen.getByRole('checkbox'))

    // Both mutations round-trip successfully without surfacing an error banner.
    await waitFor(() => expect(screen.queryByText(/failed to save/i)).not.toBeInTheDocument())
  })
})
