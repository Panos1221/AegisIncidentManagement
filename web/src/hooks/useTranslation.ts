import { useTheme } from '../lib/themeContext'
import { translations, Translations } from '../lib/translations'

export function useTranslation(): Translations {
  const { language } = useTheme()
  return translations[language]
}