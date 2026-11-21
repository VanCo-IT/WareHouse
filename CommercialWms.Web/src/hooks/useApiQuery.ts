import { useCallback, useEffect, useState } from 'react'
import { apiClient, getErrorMessage } from '../services/apiClient'

interface QueryState<T> {
  path: string | null
  version: number
  data: T | null
  error: string | null
}

export function useApiQuery<T>(path: string | null) {
  const [version, setVersion] = useState(0)
  const [result, setResult] = useState<QueryState<T>>({
    path: null,
    version: 0,
    data: null,
    error: null,
  })

  const reload = useCallback(() => setVersion((current) => current + 1), [])

  useEffect(() => {
    if (path === null) {
      return
    }

    let active = true

    void apiClient
      .get<T>(path)
      .then((result) => {
        if (active) {
          setResult({ path, version, data: result, error: null })
        }
      })
      .catch((reason: unknown) => {
        if (active) {
          setResult({ path, version, data: null, error: getErrorMessage(reason) })
        }
      })

    return () => {
      active = false
    }
  }, [path, version])

  const isCurrent = result.path === path && result.version === version
  return {
    data: isCurrent ? result.data : null,
    loading: path !== null && !isCurrent,
    error: isCurrent ? result.error : null,
    reload,
  }
}
