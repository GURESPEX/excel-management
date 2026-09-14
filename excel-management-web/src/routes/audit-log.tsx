import { createFileRoute } from '@tanstack/react-router'
import { AppShell } from '@/components/layout/AppShell'
import { requireAdmin } from '@/features/auth/guards'
import { AuditLogPage } from '@/features/auditLogs/AuditLogPage'

export const Route = createFileRoute('/audit-log')({
  beforeLoad: requireAdmin,
  component: () => (
    <AppShell>
      <AuditLogPage />
    </AppShell>
  ),
})
