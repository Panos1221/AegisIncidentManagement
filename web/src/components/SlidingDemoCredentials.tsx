import React, { useState } from 'react'
import { Eye, EyeOff, Copy, Check, X } from 'lucide-react'
import { useTranslation } from '../hooks/useTranslation'

interface DemoCredential {
  id: string
  role: string
  agency: string
  email: string
  password: string
  color: string
}

interface SlidingDemoCredentialsProps {
  onCredentialSelect?: (email: string, password: string) => void
}

const SlidingDemoCredentials: React.FC<SlidingDemoCredentialsProps> = ({ 
  onCredentialSelect
}) => {
  const [isOpen, setIsOpen] = useState(false)
  const [copiedEmail, setCopiedEmail] = useState<string | null>(null)
  const t = useTranslation()

  const demoCredentials: DemoCredential[] = [
    {
      id: 'fire-dispatch',
      role: t.dispatcher,
      agency: t.fireService,
      email: 'dispatcher@fireservice.gr',
      password: '1',
      color: 'text-red-600 dark:text-red-400'
    },
    {
      id: 'fire-member',
      role: t.member,
      agency: t.fireService,
      email: 'firefighter@fireservice.gr',
      password: '1',
      color: 'text-red-600 dark:text-red-400'
    },
    {
      id: 'coast-dispatch',
      role: t.dispatcher,
      agency: t.coastGuard,
      email: 'dispatcher@coastguard.gr',
      password: '1',
      color: 'text-sky-500 dark:text-sky-400'
    },
    {
      id: 'coast-member',
      role: t.member,
      agency: t.coastGuard,
      email: 'member@coastguard.gr',
      password: '1',
      color: 'text-sky-500 dark:text-sky-400'
    },
    {
      id: 'police-dispatch',
      role: t.dispatcher,
      agency: t.police,
      email: 'dispatcher@police.gr',
      password: '1',
      color: 'text-blue-600 dark:text-blue-400'
    },
    {
      id: 'police-member',
      role: t.member,
      agency: t.police,
      email: 'member@police.gr',
      password: '1',
      color: 'text-blue-600 dark:text-blue-400'
    },
    {
      id: 'ambulance-dispatch',
      role: t.dispatcher,
      agency: t.ekab,
      email: 'dispatcher@ekab.gr',
      password: '1',
      color: 'text-yellow-600 dark:text-yellow-400'
    },
    {
      id: 'ambulance-member',
      role: t.member,
      agency: t.ekab,
      email: 'member@ekab.gr',
      password: '1',
      color: 'text-yellow-600 dark:text-yellow-400'
    } 
   
  ]

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
      setCopiedEmail(text)
      setTimeout(() => setCopiedEmail(null), 2000)
    } catch (err) {
      console.error('Failed to copy email:', err)
    }
  }

  const handleCredentialClick = (credential: DemoCredential) => {
    if (onCredentialSelect) {
      onCredentialSelect(credential.email, credential.password)
    }
    setIsOpen(false) // Close the modal after selection
  }

  return (
    <div className="relative">
      {/* Toggle Button - Inline position */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center justify-center gap-2 w-full py-3 px-4 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg transition-colors duration-200 border border-gray-300 dark:border-gray-600"
      >
        {isOpen ? (
          <EyeOff className="h-4 w-4 text-gray-600 dark:text-gray-400" />
        ) : (
          <Eye className="h-4 w-4 text-gray-600 dark:text-gray-400" />
        )}
        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
          {isOpen ? t.hideDemoCredentials : t.showDemoCredentials}
        </span>
      </button>

      {/* Overlay */}
      {isOpen && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-25 z-30 transition-opacity duration-300"
          onClick={() => setIsOpen(false)}
        />
      )}

      {/* Sliding Panel */}
      <div className={`fixed top-1/2 left-1/2 transform -translate-y-1/2 translate-x-8 w-96 max-h-[80vh] bg-white dark:bg-gray-800 shadow-2xl z-50 rounded-lg border border-gray-200 dark:border-gray-700 transition-all duration-300 ease-in-out ${
        isOpen ? 'opacity-100 scale-100 translate-x-8' : 'opacity-0 scale-95 translate-x-full pointer-events-none'
      }`}>
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
            {t.demoCredentials}
          </h3>
          <button
            onClick={() => setIsOpen(false)}
            className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors duration-150 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-4 overflow-y-auto max-h-[calc(80vh-120px)]">
          <div className="space-y-3">
            {demoCredentials.map((credential) => (
              <div
                key={credential.id}
                className="p-4 bg-gray-50 dark:bg-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-600 transition-colors duration-150 cursor-pointer border border-gray-200 dark:border-gray-600"
                onClick={() => handleCredentialClick(credential)}
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-2">
                      <span className={`text-sm font-medium ${credential.color}`}>
                        {credential.agency}
                      </span>
                      <span className="text-xs text-gray-500 dark:text-gray-400 bg-gray-200 dark:bg-gray-600 px-2 py-1 rounded">
                        {credential.role}
                      </span>
                    </div>
                    <div className="space-y-1">
                      <div className="text-sm text-gray-900 dark:text-white font-mono">
                        {credential.email}
                      </div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">
                        {t.password}: {credential.password}
                      </div>
                    </div>
                  </div>
                  
                  <button
                    onClick={(e) => {
                      e.stopPropagation()
                      copyToClipboard(credential.email)
                    }}
                    className="ml-3 p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors duration-150 rounded-md hover:bg-gray-200 dark:hover:bg-gray-500"
                    title={t.copyEmail}
                  >
                    {copiedEmail === credential.email ? (
                      <Check className="h-4 w-4 text-green-500" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </button>
                </div>
              </div>
            ))}
          </div>
          
          <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
            <p className="text-sm text-blue-700 dark:text-blue-300 text-center">
              {t.clickOnDemoToAutoFill}
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default SlidingDemoCredentials