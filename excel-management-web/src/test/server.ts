import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'

// Defaults every test gets unless it overrides them with server.use():
// - bootstrapped as an authenticated Admin (unauthenticated-redirect and
//   Viewer-role tests override this)
// - a couple of departments, since the employee filter panel always calls
//   useDepartments() to populate its Department dropdown
export const server = setupServer(
  http.post('http://localhost:5289/auth/refresh', () =>
    HttpResponse.json({ id: 1, username: 'admin', role: 'Admin' }),
  ),
  http.get('http://localhost:5289/departments', () =>
    HttpResponse.json([
      { id: 1, name: 'Engineering', isActive: true },
      { id: 2, name: 'Marketing', isActive: true },
    ]),
  ),
)
