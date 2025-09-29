import React, { Component, ReactNode } from 'react'
import { Map, AlertTriangle, RefreshCw } from 'lucide-react'

interface Props {
  children: ReactNode
  onError?: (error: Error, errorInfo: React.ErrorInfo) => void
}

interface State {
  hasError: boolean
  error?: Error
}

class MapErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('MapErrorBoundary caught an error:', error, errorInfo)
    this.props.onError?.(error, errorInfo)
  }

  handleRetry = () => {
    this.setState({ hasError: false, error: undefined })
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="card">
          <div style={{ height: '600px' }} className="flex flex-col items-center justify-center bg-gray-50 dark:bg-gray-800">
            <div className="text-center p-8">
              <div className="flex items-center justify-center mb-4">
                <Map className="w-8 h-8 text-gray-400 dark:text-gray-500 mr-2" />
                <AlertTriangle className="w-8 h-8 text-red-500 dark:text-red-400" />
              </div>
              <h3 className="text-lg font-semibold text-gray-800 dark:text-gray-200 mb-2">Map Error</h3>
              <p className="text-gray-600 dark:text-gray-400 mb-4 max-w-md">
                {this.state.error?.message || 'Unable to load the map. This could be due to network issues or map service problems.'}
              </p>
              <button
                onClick={this.handleRetry}
                className="btn btn-primary flex items-center mx-auto"
              >
                <RefreshCw className="w-4 h-4 mr-2" />
                Reload Map
              </button>
            </div>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}

export default MapErrorBoundary