import { AlertTriangle, Check, X } from 'lucide-react'

interface ConfirmationToastProps {
  message: string
  onConfirm: () => void
  onCancel: () => void
  confirmText?: string
  cancelText?: string
  type?: 'warning' | 'danger' | 'info'
}

export function ConfirmationToast({
  message,
  onConfirm,
  onCancel,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  type = 'warning'
}: ConfirmationToastProps) {
  const getTypeStyles = () => {
    switch (type) {
      case 'danger':
        return {
          bg: 'bg-red-50 dark:bg-red-950/40 border-red-200 dark:border-red-700/50 backdrop-blur-sm',
          icon: 'text-red-600 dark:text-red-400',
          confirmBtn: 'bg-red-600 hover:bg-red-700 dark:bg-red-600 dark:hover:bg-red-500 text-white transition-colors duration-200',
          cancelBtn: 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors duration-200',
          textColor: 'text-gray-800 dark:text-gray-200'
        }
      case 'warning':
        return {
          bg: 'bg-yellow-50 dark:bg-yellow-950/40 border-yellow-200 dark:border-yellow-700/50 backdrop-blur-sm',
          icon: 'text-yellow-600 dark:text-yellow-400',
          confirmBtn: 'bg-yellow-600 hover:bg-yellow-700 dark:bg-yellow-600 dark:hover:bg-yellow-500 text-white transition-colors duration-200',
          cancelBtn: 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors duration-200',
          textColor: 'text-gray-800 dark:text-gray-200'
        }
      default:
        return {
          bg: 'bg-blue-50 dark:bg-blue-950/40 border-blue-200 dark:border-blue-700/50 backdrop-blur-sm',
          icon: 'text-blue-600 dark:text-blue-400',
          confirmBtn: 'bg-blue-600 hover:bg-blue-700 dark:bg-blue-600 dark:hover:bg-blue-500 text-white transition-colors duration-200',
          cancelBtn: 'bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-200 transition-colors duration-200',
          textColor: 'text-gray-800 dark:text-gray-200'
        }
    }
  }

  const styles = getTypeStyles()

  return (
    <div className={`p-4 border rounded-lg ${styles.bg} shadow-lg dark:shadow-2xl dark:shadow-black/20 max-w-sm ring-1 ring-black/5 dark:ring-white/10`}>
      <div className="flex items-start space-x-3">
        <AlertTriangle className={`w-5 h-5 mt-0.5 ${styles.icon}`} />
        <div className="flex-1">
          <p className={`text-sm mb-3 ${styles.textColor}`}>{message}</p>
          <div className="flex space-x-2">
            <button
              onClick={onConfirm}
              className={`px-3 py-1.5 text-xs font-medium rounded transition-colors ${styles.confirmBtn}`}
            >
              <Check className="w-3 h-3 mr-1 inline" />
              {confirmText}
            </button>
            <button
              onClick={onCancel}
              className={`px-3 py-1.5 text-xs font-medium rounded transition-colors ${styles.cancelBtn}`}
            >
              <X className="w-3 h-3 mr-1 inline" />
              {cancelText}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}