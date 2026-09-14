import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { renderApp } from '@/test/renderApp'
import { server } from '@/test/server'

const departments = [
  { id: 1, name: 'Engineering', isActive: true },
  { id: 2, name: 'Marketing', isActive: true },
]

describe('create employee flow', () => {
  it('shows a field-level validation error from the API and does not navigate away', async () => {
    // Client-side Zod already mirrors these rules, so to exercise the server
    // error path we submit a payload that passes client validation (valid
    // name/department/date) and have the API reject it anyway — e.g. the
    // department was deleted between page load and submit.
    server.use(
      http.get('http://localhost:5289/departments', () => HttpResponse.json(departments)),
      http.post('http://localhost:5289/employees', () =>
        HttpResponse.json(
          { title: 'Bad Request', errors: { DepartmentId: ['Department does not exist.'] } },
          { status: 400 },
        ),
      ),
    )

    const user = userEvent.setup()
    await renderApp('/employees/new')

    await user.type(await screen.findByLabelText(/name/i), 'New Hire')
    await user.click(screen.getByRole('combobox'))
    await user.click(await screen.findByRole('option', { name: 'Engineering' }))
    await user.type(screen.getByLabelText(/join date/i), '2024-01-01')
    await user.click(screen.getByRole('button', { name: /create/i }))

    expect(await screen.findByText('Department does not exist.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create/i })).toBeInTheDocument()
  })

  it('creates an employee and returns to the list', async () => {
    server.use(
      http.get('http://localhost:5289/departments', () => HttpResponse.json(departments)),
      http.post('http://localhost:5289/employees', async ({ request }) => {
        const body = (await request.json()) as Record<string, unknown>
        return HttpResponse.json(
          { id: 999, ...body, updatedAt: '2026-01-01T00:00:00Z' },
          { status: 201 },
        )
      }),
      http.get('http://localhost:5289/employees', () =>
        HttpResponse.json({ items: [], page: 1, pageSize: 20, totalCount: 0 }),
      ),
    )

    const user = userEvent.setup()
    await renderApp('/employees/new')

    await user.type(await screen.findByLabelText(/name/i), 'New Hire')
    await user.click(screen.getByRole('combobox'))
    await user.click(await screen.findByRole('option', { name: 'Engineering' }))
    await user.type(screen.getByLabelText(/join date/i), '2024-01-01')
    await user.click(screen.getByRole('button', { name: /create/i }))

    await waitFor(() => expect(screen.getByRole('heading', { name: /employees/i })).toBeInTheDocument())
  })
})

describe('edit employee flow', () => {
  it('shows a disabled department as the current selection, but omits it from the option list', async () => {
    server.use(
      http.get('http://localhost:5289/departments', () =>
        HttpResponse.json([
          { id: 1, name: 'Engineering', isActive: true },
          { id: 3, name: 'Legacy Dept', isActive: false },
        ]),
      ),
      http.get('http://localhost:5289/employees/2', () =>
        HttpResponse.json({
          id: 2,
          name: 'Long Timer',
          departmentId: 3,
          salary: 1000,
          joinDate: '2024-01-01',
          isActive: true,
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      ),
    )

    const user = userEvent.setup()
    await renderApp('/employees/2')

    expect(await screen.findByRole('combobox', { name: /department/i })).toHaveTextContent('Legacy Dept')

    await user.click(screen.getByRole('combobox'))
    expect(await screen.findByRole('option', { name: 'Engineering' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'Legacy Dept' })).not.toBeInTheDocument()
  })

  it('loads existing values and submits an update', async () => {
    server.use(
      http.get('http://localhost:5289/departments', () => HttpResponse.json(departments)),
      http.get('http://localhost:5289/employees/1', () =>
        HttpResponse.json({
          id: 1,
          name: 'Original Name',
          departmentId: 1,
          salary: 1000,
          joinDate: '2024-01-01',
          isActive: true,
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      ),
      http.put('http://localhost:5289/employees/1', () => new HttpResponse(null, { status: 204 })),
      http.get('http://localhost:5289/employees', () =>
        HttpResponse.json({ items: [], page: 1, pageSize: 20, totalCount: 0 }),
      ),
    )

    const user = userEvent.setup()
    await renderApp('/employees/1')

    const nameInput = await screen.findByDisplayValue('Original Name')
    await user.clear(nameInput)
    await user.type(nameInput, 'Updated Name')
    await user.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => expect(screen.getByRole('heading', { name: /employees/i })).toBeInTheDocument())
  })
})
