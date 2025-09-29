import React from 'react'
import { useTranslation } from '../hooks/useTranslation'
import { useRanksByAgency } from '../utils/rankUtils'

interface RankSelectProps {
  value: string
  onChange: (value: string) => void
  agencyName?: string
  className?: string
  required?: boolean
  disabled?: boolean
}

export const RankSelect: React.FC<RankSelectProps> = ({
  value,
  onChange,
  agencyName,
  className = '',
  required = false,
  disabled = false
}) => {
  const t = useTranslation()
  const rankGroups = useRanksByAgency(agencyName)

  // Flatten all rank options from all groups
  const allRankOptions = rankGroups.flatMap(group => group.options)

  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className={`input w-full ${className}`}
      required={required}
      disabled={disabled}
    >
      <option value="" style={{ fontWeight: 'bold' }}>{t.selectRank}</option>
      {allRankOptions.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  )
}

export default RankSelect