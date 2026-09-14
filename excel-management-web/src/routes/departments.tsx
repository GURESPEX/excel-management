import { createFileRoute } from '@tanstack/react-router'
import { AppShell } from '@/components/layout/AppShell'
import { requireAuth } from '@/features/auth/guards'
import { DepartmentAdminPage } from '@/features/departments/DepartmentAdminPage'

export const Route = createFileRoute('/departments')({
  beforeLoad: requireAuth,
  component: () => (
    <AppShell>
      <DepartmentAdminPage />
    </AppShell>
  ),
})
