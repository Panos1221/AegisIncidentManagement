import React, { useState, useEffect } from 'react'
import logoUnified from '../icons/Logos/logo_unified.png'

interface AnimatedLogoProps {
  size?: 'sm' | 'md' | 'lg' | 'xl'
  className?: string
}

const AnimatedLogo: React.FC<AnimatedLogoProps> = ({ size = 'lg', className = '' }) => {
  const [imgError, setImgError] = useState(false)
  const [isLoaded, setIsLoaded] = useState(false)

  const sizeClasses = {
    sm: 'h-16 w-16',
    md: 'h-24 w-24',
    lg: 'h-32 w-32',
    xl: 'h-40 w-40'
  }

  useEffect(() => {
    // Preload the image
    const img = new Image()
    img.onload = () => setIsLoaded(true)
    img.onerror = () => setImgError(true)
    img.src = logoUnified
  }, [])

  return (
    <div className={`relative flex items-center justify-center ${sizeClasses[size]} ${className}`}>
      {/* Animated background rings */}
      <div className="absolute inset-0" style={{ animation: 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite' }}>
        <div className="absolute inset-0 rounded-full bg-gradient-to-r from-red-500/20 to-red-600/20" style={{ animation: 'ping 3s cubic-bezier(0, 0, 0.2, 1) infinite' }}></div>
        <div className="absolute inset-2 rounded-full bg-gradient-to-r from-red-400/30 to-red-500/30" style={{ animation: 'ping 3s cubic-bezier(0, 0, 0.2, 1) infinite', animationDelay: '1.5s' }}></div>
      </div>

      {/* Rotating border */}
      <div className="absolute inset-0 rounded-full bg-gradient-to-r from-red-500 via-red-600 to-red-700 animate-spin-slow opacity-30"></div>
      <div className="absolute inset-1 rounded-full bg-gray-50 dark:bg-gray-900"></div>

      {/* Logo container with hover effects */}
      <div className="relative z-10 group cursor-pointer transform transition-all duration-300 hover:scale-110">
        {imgError ? (
          <div className="flex items-center justify-center h-full w-full">
            <svg
              className="h-12 w-12 text-red-600 dark:text-red-400 animate-bounce"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M17.657 18.657A8 8 0 016.343 7.343S7 9 9 10c0-2 .5-5 2.5-7 0 0 .5 2 2.5 2C16 5 16.657 5.343 17.657 6.343a8 8 0 010 11.314z"
              />
            </svg>
          </div>
        ) : (
          <img
            src={logoUnified}
            alt="Aegis Logo"
            className={`${sizeClasses[size]} object-contain transition-all duration-300 ${
              isLoaded ? 'opacity-100 animate-fade-in' : 'opacity-0'
            } group-hover:brightness-110 group-hover:drop-shadow-lg`}
            onError={() => setImgError(true)}
            onLoad={() => setIsLoaded(true)}
          />
        )}

        {/* Floating particles effect */}
        <div className="absolute inset-0 pointer-events-none">
          <div className="absolute top-2 left-2 w-1 h-1 bg-red-400 rounded-full animate-float opacity-60"></div>
          <div className="absolute top-4 right-3 w-1 h-1 bg-red-500 rounded-full animate-float-delayed opacity-40"></div>
          <div className="absolute bottom-3 left-4 w-1 h-1 bg-red-300 rounded-full animate-float-slow opacity-50"></div>
          <div className="absolute bottom-2 right-2 w-1 h-1 bg-red-600 rounded-full animate-float-delayed-2 opacity-30"></div>
        </div>
      </div>

      {/* Glow effect on hover */}
      <div className="absolute inset-0 rounded-full bg-gradient-to-r from-red-500 to-red-600 opacity-0 group-hover:opacity-20 transition-opacity duration-300 blur-xl"></div>
    </div>
  )
}

export default AnimatedLogo