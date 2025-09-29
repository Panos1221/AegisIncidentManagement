import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react'
import ErrorToast, { ToastType } from './ErrorToast'
import { ConfirmationToast } from './ConfirmationToast'

interface Toast {
  id: string
  type: ToastType
  title: string
  message?: string
  duration?: number
  action?: {
    label: string
    onClick: () => void
  }
}

interface ConfirmationToastData {
  id: string
  message: string
  onConfirm: () => void
  onCancel: () => void
  confirmText?: string
  cancelText?: string
  type?: 'warning' | 'danger' | 'info'
}

interface ToastContextType {
  showToast: (toast: Omit<Toast, 'id'>) => void
  showError: (title: string, message?: string, action?: Toast['action']) => void
  showSuccess: (title: string, message?: string) => void
  showWarning: (title: string, message?: string) => void
  showInfo: (title: string, message?: string) => void
  showConfirmation: (options: Omit<ConfirmationToastData, 'id'>) => void
  clearToasts: () => void
}

const ToastContext = createContext<ToastContextType | undefined>(undefined)

export const useToast = () => {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider')
  }
  return context
}

interface ToastProviderProps {
  children: ReactNode
}

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([])
  const [confirmationToasts, setConfirmationToasts] = useState<ConfirmationToastData[]>([])

  const showToast = useCallback((toast: Omit<Toast, 'id'>) => {
    const id = Math.random().toString(36).substr(2, 9)
    const newToast: Toast = {
      ...toast,
      id,
      duration: toast.duration ?? (toast.type === 'error' ? 0 : 5000), // Errors persist, others auto-dismiss
    }
    
    setToasts(prev => [...prev, newToast])
  }, [])

  const showError = useCallback((title: string, message?: string, action?: Toast['action']) => {
    showToast({ type: 'error', title, message, action })
  }, [showToast])

  const showSuccess = useCallback((title: string, message?: string) => {
    showToast({ type: 'success', title, message })
  }, [showToast])

  const showWarning = useCallback((title: string, message?: string) => {
    showToast({ type: 'warning', title, message })
  }, [showToast])

  const showInfo = useCallback((title: string, message?: string) => {
    showToast({ type: 'info', title, message })
  }, [showToast])

  const showConfirmation = useCallback((options: Omit<ConfirmationToastData, 'id'>) => {
    const id = Math.random().toString(36).substr(2, 9)
    const confirmationToast: ConfirmationToastData = {
      ...options,
      id,
      onConfirm: () => {
        options.onConfirm()
        setConfirmationToasts(prev => prev.filter(toast => toast.id !== id))
      },
      onCancel: () => {
        options.onCancel()
        setConfirmationToasts(prev => prev.filter(toast => toast.id !== id))
      }
    }
    
    setConfirmationToasts(prev => [...prev, confirmationToast])
  }, [])

  const clearToasts = useCallback(() => {
    setToasts([])
    setConfirmationToasts([])
  }, [])

  const removeToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(toast => toast.id !== id))
  }, [])

  const contextValue: ToastContextType = {
    showToast,
    showError,
    showSuccess,
    showWarning,
    showInfo,
    showConfirmation,
    clearToasts,
  }

  return (
    <ToastContext.Provider value={contextValue}>
      {children}
      
      {/* Toast Container */}
      <div className="fixed top-4 right-4 z-50 space-y-2">
        {toasts.map(toast => (
          <ErrorToast
            key={toast.id}
            toast={toast}
            onClose={removeToast}
          />
        ))}
        {confirmationToasts.map(confirmationToast => (
          <ConfirmationToast
            key={confirmationToast.id}
            message={confirmationToast.message}
            onConfirm={confirmationToast.onConfirm}
            onCancel={confirmationToast.onCancel}
            confirmText={confirmationToast.confirmText}
            cancelText={confirmationToast.cancelText}
            type={confirmationToast.type}
          />
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export { ToastProvider as default }