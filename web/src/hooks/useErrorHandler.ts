import { useState, useCallback } from 'react'
import { ApiError } from '../lib/api'

interface ErrorState {
  error: Error | null
  isError: boolean
  errorMessage: string | null
}

interface UseErrorHandlerReturn extends ErrorState {
  setError: (error: Error | null) => void
  clearError: () => void
  handleError: (error: unknown) => void
  retry: (operation: () => void | Promise<void>) => Promise<void>
}

export const useErrorHandler = (): UseErrorHandlerReturn => {
  const [errorState, setErrorState] = useState<ErrorState>({
    error: null,
    isError: false,
    errorMessage: null,
  })

  const setError = useCallback((error: Error | null) => {
    if (error) {
      setErrorState({
        error,
        isError: true,
        errorMessage: error.message,
      })
    } else {
      setErrorState({
        error: null,
        isError: false,
        errorMessage: null,
      })
    }
  }, [])

  const clearError = useCallback(() => {
    setErrorState({
      error: null,
      isError: false,
      errorMessage: null,
    })
  }, [])

  const handleError = useCallback((error: unknown) => {
    if (error instanceof Error) {
      setError(error)
    } else if (typeof error === 'string') {
      setError(new Error(error))
    } else {
      setError(new Error('An unknown error occurred'))
    }
  }, [setError])

  const retry = useCallback(async (operation: () => void | Promise<void>) => {
    try {
      clearError()
      await operation()
    } catch (error) {
      handleError(error)
    }
  }, [clearError, handleError])

  return {
    ...errorState,
    setError,
    clearError,
    handleError,
    retry,
  }
}

// Hook for handling API errors specifically
export const useApiErrorHandler = () => {
  const errorHandler = useErrorHandler()

  const handleApiError = useCallback((error: unknown) => {
    if (error instanceof ApiError) {
      errorHandler.setError(error)
    } else if (error instanceof Error) {
      errorHandler.setError(new ApiError(error.message))
    } else {
      errorHandler.setError(new ApiError('An unexpected error occurred'))
    }
  }, [errorHandler])

  return {
    ...errorHandler,
    handleApiError,
  }
}

export default useErrorHandler