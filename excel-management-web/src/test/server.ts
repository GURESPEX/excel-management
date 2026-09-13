import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'

// Default: every test is bootstrapped as an authenticated Admin unless a test
// overrides this handler (e.g. to exercise the unauthenticated-redirect or
// Viewer-role-restriction scenarios).
export const server = setupServer(
  http.post('http://localhost:5289/auth/refresh', () =>
    HttpResponse.json({ id: 1, username: 'admin', role: 'Admin' }),
  ),
)
