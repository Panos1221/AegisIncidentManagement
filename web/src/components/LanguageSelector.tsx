import { Languages } from 'lucide-react'
import { useTheme } from '../lib/themeContext'
import { useTranslation } from '../hooks/useTranslation'
import { useState, useRef, useEffect } from 'react'

export default function LanguageSelector() {
  const { language, setLanguage } = useTheme()
  const t = useTranslation()
  const [isOpen, setIsOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [])

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="p-2 text-gray-600 hover:text-gray-900 dark:text-gray-300 dark:hover:text-gray-100 transition-colors"
        title={t.language}
      >
        <Languages className="w-5 h-5" />
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-32 bg-white dark:bg-gray-800 rounded-md shadow-lg border border-gray-200 dark:border-gray-700 z-50">
          <div className="py-1">
            <button
              onClick={() => {
                setLanguage('en')
                setIsOpen(false)
              }}
              className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors ${
                language === 'en' 
                  ? 'bg-primary-50 text-primary-700 dark:bg-primary-900 dark:text-primary-300' 
                  : 'text-gray-700 dark:text-gray-300'
              }`}
            >
              {t.english}
            </button>
            <button
              onClick={() => {
                setLanguage('el')
                setIsOpen(false)
              }}
              className={`w-full text-left px-4 py-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors ${
                language === 'el' 
                  ? 'bg-primary-50 text-primary-700 dark:bg-primary-900 dark:text-primary-300' 
                  : 'text-gray-700 dark:text-gray-300'
              }`}
            >
              {t.greek}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}