import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { LoadingSpinner, AnimatedLogo } from '../components'
import ThemeToggle from '../components/ThemeToggle'
import LanguageSelector from '../components/LanguageSelector'
import SlidingDemoCredentials from '../components/SlidingDemoCredentials'
import { authService } from '../lib/authService'
import { useTranslation } from '../hooks/useTranslation'
import { Mail, Lock, LogIn } from 'lucide-react'


interface LoginForm {
  email: string
  password: string
}



const Login: React.FC = () => {
  const [form, setForm] = useState<LoginForm>({ email: '', password: '' })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [fieldErrors, setFieldErrors] = useState<{ email?: string; password?: string }>({})
  const navigate = useNavigate()
  const t = useTranslation()


  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) {
      return
    }
    
    setLoading(true)
    setError('')
    setSuccess('')
    setFieldErrors({})

    try {
      await authService.login(form)
      setSuccess(t.loginSuccess)
      setTimeout(() => {
        navigate('/dashboard')
      }, 500)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : t.loginError
      setError(errorMessage)
      
      // If it's a credential error, focus on email field
      if (errorMessage.toLowerCase().includes('credential') || errorMessage.toLowerCase().includes('invalid')) {
        document.getElementById('email')?.focus()
      }
    } finally {
      setLoading(false)
    }
  }

  const handleCredentialSelect = (email: string, password: string) => {
    setForm({ email, password })
    setError('')
    setSuccess('')
    setFieldErrors({})
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
    // Clear field-specific errors when user starts typing
    if (fieldErrors[name as keyof typeof fieldErrors]) {
      setFieldErrors(prev => ({ ...prev, [name]: undefined }))
    }
    // Clear general error when user starts typing
    if (error) {
      setError('')
    }
  }

  const validateForm = (): boolean => {
    const errors: { email?: string; password?: string } = {}
    
    if (!form.email.trim()) {
      errors.email = 'Email is required'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      errors.email = 'Please enter a valid email address'
    }
    
    if (!form.password.trim()) {
      errors.password = 'Password is required'
    } else if (form.password.length < 1) {
      errors.password = 'Password must be at least 1 character'
    }
    
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-gray-100 to-gray-200 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 relative">
      {/* Theme and Language Controls */}
      <div className="absolute top-4 right-4 flex items-center space-x-2 z-10">
        <ThemeToggle />
        <LanguageSelector />
      </div>
      
      <div className="max-w-6xl w-full grid grid-cols-1 lg:grid-cols-2 gap-8 items-center">
        
        {/* Left Side - Branding and Info */}
        <div className="hidden lg:flex flex-col items-center justify-center space-y-8 p-8">
          <div className="text-center space-y-6">
            <AnimatedLogo size="xl" className="mx-auto" />
            
            <div className="space-y-4">
              <h1 className="text-4xl font-bold bg-gradient-to-r from-red-600 via-red-700 to-red-800 bg-clip-text text-transparent">
                {t.loginTitle}
              </h1>
              <p className="text-lg text-gray-600 dark:text-gray-300 max-w-md">
                {t.loginSubtitle}
              </p>
            </div>
          </div>
        </div>

        {/* Right Side - Login Form */}
        <div className="w-full max-w-md mx-auto lg:mx-0">
          <div className="bg-white dark:bg-gray-800 shadow-2xl rounded-2xl p-8 border border-gray-200 dark:border-gray-700">
            
            {/* Mobile Logo */}
            <div className="lg:hidden text-center mb-8">
              <AnimatedLogo size="lg" className="mx-auto mb-4" />
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
                {t.loginWelcome}
              </h2>
              <p className="text-sm text-gray-600 dark:text-gray-400 mt-2">
                {t.loginDescription}
              </p>
            </div>

            {/* Desktop Header */}
            <div className="hidden lg:block text-center mb-8">
              <h2 className="text-3xl font-bold text-gray-900 dark:text-white">
                {t.loginWelcome}
              </h2>
              <p className="text-gray-600 dark:text-gray-400 mt-2">
                {t.loginDescription}
              </p>
            </div>

            <form className="space-y-6" onSubmit={handleSubmit}>
              {/* Email Field */}
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.email}
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Mail className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    autoComplete="email"
                    required
                    className={`block w-full pl-10 pr-3 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-colors duration-200 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 ${
                      fieldErrors.email 
                        ? 'border-red-500 dark:border-red-500' 
                        : 'border-gray-300 dark:border-gray-600'
                    }`}
                    placeholder={t.enterEmail}
                    value={form.email}
                    onChange={handleChange}
                  />
                </div>
                {fieldErrors.email && (
                  <p className="text-red-600 dark:text-red-400 text-sm mt-1">
                    {fieldErrors.email}
                  </p>
                )}
              </div>

              {/* Password Field */}
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  {t.password}
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Lock className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    id="password"
                    name="password"
                    type="password"
                    autoComplete="current-password"
                    required
                    className={`block w-full pl-10 pr-3 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-transparent transition-colors duration-200 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 ${
                      fieldErrors.password 
                        ? 'border-red-500 dark:border-red-500' 
                        : 'border-gray-300 dark:border-gray-600'
                    }`}
                    placeholder={t.enterPassword}
                    value={form.password}
                    onChange={handleChange}
                  />
                </div>
                {fieldErrors.password && (
                  <p className="text-red-600 dark:text-red-400 text-sm mt-1">
                    {fieldErrors.password}
                  </p>
                )}
              </div>

              {/* Error Message */}
              {error && (
                <div className="rounded-lg bg-red-50 dark:bg-red-900/50 p-4 border border-red-200 dark:border-red-800">
                  <div className="text-sm text-red-700 dark:text-red-200">{error}</div>
                </div>
              )}

              {/* Success Message */}
              {success && (
                <div className="rounded-lg bg-green-50 dark:bg-green-900/50 p-4 border border-green-200 dark:border-green-800">
                  <div className="text-sm text-green-700 dark:text-green-200">{success}</div>
                </div>
              )}

              {/* Submit Button */}
              <button
                type="submit"
                disabled={loading || !form.email.trim() || !form.password.trim()}
                className="w-full flex justify-center items-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200 transform hover:scale-[1.02] active:scale-[0.98]"
              >
                {loading ? (
                  <LoadingSpinner size="sm" />
                ) : (
                  <>
                    <LogIn className="h-4 w-4 mr-2" />
                    {t.loginButton}
                  </>
                )}
              </button>
            </form>

            {/* Demo Credentials Button */}
            <div className="mt-8">
              <SlidingDemoCredentials onCredentialSelect={handleCredentialSelect} />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Login