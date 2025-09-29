import React from 'react'
import { RefreshCw } from 'lucide-react'

interface RetryButtonProps {
  onRetry: () => void
  isLoading?: boolean
  text?: string
  className?: string
  size?: 'sm' | 'md' | 'lg'
}

const RetryButton: React.FC<RetryButtonProps> = ({
  onRetry,
  isLoading = false,
  text = 'Retry',
  className = '',
  size = 'md'
}) => {
  const sizeClasses = {
    sm: 'px-2 py-1 text-sm',
    md: 'px-3 py-2 text-sm',
    lg: 'px-4 py-2 text-base'
  }

  const iconSizeClasses = {
    sm: 'w-3 h-3',
    md: 'w-4 h-4',
    lg: 'w-5 h-5'
  }

  return (
    <button
      onClick={onRetry}
      disabled={isLoading}
      className={`
        btn btn-secondary flex items-center
        ${sizeClasses[size]}
        ${isLoading ? 'opacity-50 cursor-not-allowed' : ''}
        ${className}
      `}
    >
      <RefreshCw className={`${iconSizeClasses[size]} mr-2 ${isLoading ? 'animate-spin' : ''}`} />
      {isLoading ? 'Retrying...' : text}
    </button>
  )
}

export default RetryButton