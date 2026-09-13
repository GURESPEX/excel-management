import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { renderApp } from '@/test/renderApp'
import { server } from '@/test/server'

describe('EmployeeListPage', () => {
  it('renders seeded employees from the API', async () => {
    server.use(
      http.get('http://localhost:5289/employees', () =>
        HttpResponse.json({
          items: [
            {
              id: 101,
              name: 'John Doe',
              departmentName: 'Engineering',
              salary: 65000,
              joinDate: '2023-01-15',
              isActive: true,
              updatedAt: '2026-01-10T00:00:00Z',
            },
            {
              id: 104,
              name: 'Bob Brown',
              departmentName: 'Engineering',
              salary: 72000,
              joinDate: '2022-11-10',
              isActive: false,
              updatedAt: '2026-03-01T00:00:00Z',
            },
          ],
          page: 1,
          pageSize: 20,
          totalCount: 2,
        }),
      ),
    )

    await renderApp('/')

    expect(await screen.findByText('John Doe')).toBeInTheDocument()
    expect(screen.getByText('Bob Brown')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('Inactive')).toBeInTheDocument()
  })

  it('applies a name filter and updates the list without a full page reload', async () => {
    server.use(
      http.get('http://localhost:5289/employees', ({ request }) => {
        const nameFilter = new URL(request.url).searchParams.get('Name')
        const all = [
          { id: 1, name: 'John Doe', departmentName: 'Engineering', salary: 65000, joinDate: '2023-01-15', isActive: true, updatedAt: '2026-01-10T00:00:00Z' },
          { id: 2, name: 'Jane Smith', departmentName: 'Marketing', salary: 58000, joinDate: '2023-03-22', isActive: true, updatedAt: '2026-01-10T00:00:00Z' },
        ]
        const items = nameFilter ? all.filter((e) => e.name.includes(nameFilter)) : all
        return HttpResponse.json({ items, page: 1, pageSize: 20, totalCount: items.length })
      }),
    )

    const user = userEvent.setup()
    await renderApp('/')

    expect(await screen.findByText('Jane Smith')).toBeInTheDocument()

    await user.type(await screen.findByLabelText(/^name$/i), 'John')

    await waitFor(() => expect(screen.queryByText('Jane Smith')).not.toBeInTheDocument())
    expect(screen.getByText('John Doe')).toBeInTheDocument()
  })
})
