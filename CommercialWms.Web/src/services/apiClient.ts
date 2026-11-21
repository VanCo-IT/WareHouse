export type ValidationErrors = Record<string, string[]>

export class ApiError extends Error {
  public readonly status: number
  public readonly validationErrors: ValidationErrors

  constructor(
    message: string,
    status: number,
    validationErrors: ValidationErrors = {},
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
  }
}

type JsonRecord = Record<string, unknown>

function isRecord(value: unknown): value is JsonRecord {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function readValidationErrors(value: unknown): ValidationErrors {
  if (!isRecord(value) || !isRecord(value.errors)) {
    return {}
  }

  return Object.fromEntries(
    Object.entries(value.errors).map(([field, messages]) => [
      field,
      Array.isArray(messages)
        ? messages.filter((message): message is string => typeof message === 'string')
        : typeof messages === 'string'
          ? [messages]
          : [],
    ]),
  )
}

function readProblemMessage(value: unknown, fallback: string): string {
  if (!isRecord(value)) {
    return typeof value === 'string' && value.length > 0 ? value : fallback
  }

  if (typeof value.detail === 'string' && value.detail.length > 0) {
    return value.detail
  }

  if (typeof value.title === 'string' && value.title.length > 0) {
    return value.title
  }

  return fallback
}

async function request<T>(path: string, method: string, body?: unknown): Promise<T> {
  const headers = new Headers({ Accept: 'application/json' })
  if (body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(path, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const responseText = await response.text()
  let payload: unknown
  if (responseText.length > 0) {
    try {
      payload = JSON.parse(responseText) as unknown
    } catch {
      payload = responseText
    }
  }

  if (!response.ok) {
    throw new ApiError(
      readProblemMessage(payload, `Yêu cầu thất bại (HTTP ${response.status}).`),
      response.status,
      readValidationErrors(payload),
    )
  }

  if (responseText.length === 0) {
    return undefined as T
  }

  if (typeof payload === 'string') {
    throw new Error('API trả về nội dung không phải JSON hợp lệ.')
  }

  return payload as T
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path, 'GET'),
  post: <T>(path: string, body: unknown) => request<T>(path, 'POST', body),
  put: <T>(path: string, body: unknown) => request<T>(path, 'PUT', body),
  patch: <T>(path: string, body: unknown) => request<T>(path, 'PATCH', body),
  delete: (path: string) => request<void>(path, 'DELETE'),
}

export function getErrorMessage(error: unknown): string {
  if (error instanceof Error && error.message.length > 0) {
    return error.message
  }

  return 'Đã xảy ra lỗi không xác định.'
}
