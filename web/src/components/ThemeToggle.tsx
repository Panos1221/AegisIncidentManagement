import { Moon, Sun } from 'lucide-react'
import { useTheme } from '../lib/themeContext'
import { useTranslation } from '../hooks/useTranslation'

export default function ThemeToggle() {
  const { theme, toggleTheme } = useTheme()
  const t = useTranslation()

  return (
    <button
      onClick={toggleTheme}
      className="p-2 text-gray-600 hover:text-gray-900 dark:text-gray-300 dark:hover:text-gray-100 transition-colors"
      title={theme === 'light' ? t.darkMode : t.lightMode}
    >
      {theme === 'light' ? (
        <Moon className="w-5 h-5" />
      ) : (
        <Sun className="w-5 h-5" />
      )}
    </button>
  )
}