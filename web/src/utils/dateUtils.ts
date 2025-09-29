import { format } from 'date-fns'

/**
 * Formats a UTC date string to the user's local timezone
 * @param utcDateString - UTC date string from the backend
 * @param formatString - date-fns format string (default: 'MMM d, HH:mm')
 * @returns Formatted date string in user's local timezone
 */
export function formatInLocalTimezone(utcDateString: string, formatString: string = 'MMM d, HH:mm'): string {
  // Create a Date object from the UTC string
  // The backend sends UTC dates, so we need to treat them as UTC
  const utcDate = new Date(utcDateString + (utcDateString.endsWith('Z') ? '' : 'Z'))
  
  // Format the date using date-fns, which will automatically use the local timezone
  return format(utcDate, formatString)
}

/**
 * Formats a UTC date string to a specific timezone
 * @param utcDateString - UTC date string from the backend
 * @param formatString - date-fns format string
 * @param timeZone - IANA timezone identifier (e.g., 'America/New_York')
 * @returns Formatted date string in the specified timezone
 */
export function formatInTimezone(utcDateString: string, formatString: string, timeZone: string): string {
  const utcDate = new Date(utcDateString + (utcDateString.endsWith('Z') ? '' : 'Z'))
  
  // Use Intl.DateTimeFormat for timezone conversion
  const options: Intl.DateTimeFormatOptions = {
    timeZone,
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  }
  
  return new Intl.DateTimeFormat('en-US', options).format(utcDate)
}

/**
 * Gets the user's current timezone
 * @returns IANA timezone identifier
 */
export function getUserTimezone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone
}