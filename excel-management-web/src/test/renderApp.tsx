import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createMemoryHistory, createRouter, RouterProvider } from '@tanstack/react-router'
import { render } from '@testing-library/react'
import { useAuthStore } from '@/features/auth/store'
import { routeTree } from '@/routeTree.gen'

export async function renderApp(initialPath = '/') {
  // Auth is a module-level singleton store, so reset it before every render —
  // otherwise a later test in the same file inherits the previous test's
  // authenticated/unauthenticated state instead of re-running the bootstrap
  // (root beforeLoad) check against this test's own MSW handlers.
  useAuthStore.setState({ user: null, status: 'loading' })

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createRouter({
    routeTree,
    history: createMemoryHistory({ initialEntries: [initialPath] }),
  })
  await router.load()

  return render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  )
}
