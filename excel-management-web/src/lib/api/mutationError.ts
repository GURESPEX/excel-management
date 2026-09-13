export class ValidationError extends Error {
  fieldErrors: Record<string, string[]>

  constructor(fieldErrors: Record<string, string[]>) {
    super('Validation failed')
    this.fieldErrors = fieldErrors
  }
}

export function throwOnMutationError(
  error: { errors?: Record<string, string[]> | null } | undefined,
  status: number,
  fallbackMessage: string,
): void {
  if (!error) {
    return
  }
  if (status === 400) {
    throw new ValidationError(error.errors ?? {})
  }
  throw new Error(fallbackMessage)
}
