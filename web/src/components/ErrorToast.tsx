import React, { useEffect, useState } from 'react'
import { AlertTriangle, X, CheckCircle, Info, AlertCircle } from 'lucide-react'

export type ToastType = 'error' | 'success' | 'warning' | 'info'

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

interface ErrorToastProps {
  toast: Toast
  onClose: (id: string) => void
}

const ErrorToast: React.FC<ErrorToastProps> = ({ toast, onClose }) => {
  const [isVisible, setIsVisible] = useState(false)
  const [isLeaving, setIsLeaving] = useState(false)

  useEffect(() => {
    // Animate in
    const timer = setTimeout(() => setIsVisible(true), 10)
    return () => clearTimeout(timer)
  }, [])

  useEffect(() => {
    if (toast.duration && toast.duration > 0) {
      const timer = setTimeout(() => {
        handleClose()
      }, toast.duration)
      return () => clearTimeout(timer)
    }
  }, [toast.duration])

  const handleClose = () => {
    setIsLeaving(true)
    setTimeout(() => {
      onClose(toast.id)
    }, 300) // Match animation duration
  }

  const getIcon = () => {
    switch (toast.type) {
      case 'error':
        return <AlertTriangle className="w-5 h-5 text-red-500 dark:text-red-400" />
      case 'success':
        return <CheckCircle className="w-5 h-5 text-green-500 dark:text-green-400" />
      case 'warning':
        return <AlertCircle className="w-5 h-5 text-yellow-500 dark:text-yellow-400" />
      case 'info':
        return <Info className="w-5 h-5 text-blue-500 dark:text-blue-400" />
    }
  }

  const getBackgroundColor = () => {
    switch (toast.type) {
      case 'error':
        return 'bg-red-50 dark:bg-red-950/40 border-red-200 dark:border-red-700/50 backdrop-blur-sm'
      case 'success':
        return 'bg-green-50 dark:bg-green-950/40 border-green-200 dark:border-green-700/50 backdrop-blur-sm'
      case 'warning':
        return 'bg-yellow-50 dark:bg-yellow-950/40 border-yellow-200 dark:border-yellow-700/50 backdrop-blur-sm'
      case 'info':
        return 'bg-blue-50 dark:bg-blue-950/40 border-blue-200 dark:border-blue-700/50 backdrop-blur-sm'
    }
  }

  const getTextColor = () => {
    switch (toast.type) {
      case 'error':
        return 'text-red-800 dark:text-red-200'
      case 'success':
        return 'text-green-800 dark:text-green-200'
      case 'warning':
        return 'text-yellow-800 dark:text-yellow-200'
      case 'info':
        return 'text-blue-800 dark:text-blue-200'
    }
  }

  return (
    <div
      className={`
        transform transition-all duration-300 ease-in-out
        ${isVisible && !isLeaving ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'}
        ${getBackgroundColor()}
        border rounded-lg p-4 shadow-lg dark:shadow-2xl dark:shadow-black/20 max-w-md w-full
        ring-1 ring-black/5 dark:ring-white/10
      `}
    >
      <div className="flex items-start">
        <div className="flex-shrink-0">
          {getIcon()}
        </div>
        <div className="ml-3 flex-1">
          <h3 className={`text-sm font-medium ${getTextColor()}`}>
            {toast.title}
          </h3>
          {toast.message && (
            <p className={`mt-1 text-sm ${getTextColor()} opacity-90 dark:opacity-95`}>
              {toast.message}
            </p>
          )}
          {toast.action && (
            <div className="mt-2">
              <button
                onClick={toast.action.onClick}
                className={`
                  text-sm font-medium underline hover:no-underline transition-colors duration-200
                  ${toast.type === 'error' ? 'text-red-700 dark:text-red-300 hover:text-red-800 dark:hover:text-red-200' : 
                    toast.type === 'success' ? 'text-green-700 dark:text-green-300 hover:text-green-800 dark:hover:text-green-200' :
                    toast.type === 'warning' ? 'text-yellow-700 dark:text-yellow-300 hover:text-yellow-800 dark:hover:text-yellow-200' : 
                    'text-blue-700 dark:text-blue-300 hover:text-blue-800 dark:hover:text-blue-200'}
                `}
              >
                {toast.action.label}
              </button>
            </div>
          )}
        </div>
        <div className="ml-4 flex-shrink-0">
          <button
            onClick={handleClose}
            className={`
              inline-flex rounded-md p-1.5 focus:outline-none focus:ring-2 focus:ring-offset-2 transition-all duration-200
              ${toast.type === 'error' ? 'text-red-400 dark:text-red-300 hover:bg-red-100 dark:hover:bg-red-800/30 focus:ring-red-500 dark:focus:ring-red-400' :
                toast.type === 'success' ? 'text-green-400 dark:text-green-300 hover:bg-green-100 dark:hover:bg-green-800/30 focus:ring-green-500 dark:focus:ring-green-400' :
                toast.type === 'warning' ? 'text-yellow-400 dark:text-yellow-300 hover:bg-yellow-100 dark:hover:bg-yellow-800/30 focus:ring-yellow-500 dark:focus:ring-yellow-400' :
                'text-blue-400 dark:text-blue-300 hover:bg-blue-100 dark:hover:bg-blue-800/30 focus:ring-blue-500 dark:focus:ring-blue-400'}
            `}
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  )
}

export default ErrorToast