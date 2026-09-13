import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { server } from '@/test/server'
import { EmployeeListPage } from './EmployeeListPage'

function renderWithClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>)
}

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

    renderWithClient(<EmployeeListPage />)

    expect(await screen.findByText('John Doe')).toBeInTheDocument()
    expect(screen.getByText('Bob Brown')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('Inactive')).toBeInTheDocument()
  })
})
