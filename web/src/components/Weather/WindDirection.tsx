import React, { useEffect, useState } from 'react';
import { ArrowUp } from 'lucide-react';

export const getWindDirectionLabel = (degrees: number, t?: any): string => {
  const directions = [
    'N', 'NNE', 'NE', 'ENE',
    'E', 'ESE', 'SE', 'SSE',
    'S', 'SSW', 'SW', 'WSW',
    'W', 'WNW', 'NW', 'NNW'
  ];
  const index = Math.round(((degrees % 360) + 360) % 360 / 22.5) % 16;
  const direction = directions[index];
  
  // Return translated direction if translation function is provided
  if (t) {
    const translationKey = `wind${direction}`;
    if (t[translationKey]) {
      return t[translationKey];
    }
  }
  
  return direction;
};

interface WindDirectionProps {
  degrees: number;
  size?: number;
  t?: any;
}

export const WindDirection: React.FC<WindDirectionProps> = ({ degrees, size = 64, t }) => {
  const [rotation, setRotation] = useState<number>(degrees);

  useEffect(() => {
    setRotation((prev) => {
      const from = ((prev % 360) + 360) % 360;
      const to = ((degrees % 360) + 360) % 360;
      let delta = to - from;
      if (delta > 180) delta -= 360;
      if (delta < -180) delta += 360;
      return prev + delta;
    });
  }, [degrees]);

  return (
    <div className="flex flex-col items-center">
      <div
        style={{ width: size, height: size }}
        className="relative flex items-center justify-center rounded-full border border-gray-300 dark:border-gray-500 bg-gray-100 dark:bg-gray-900 shadow-md"
      >
        <ArrowUp
          className="w-6 h-6 text-blue-600 dark:text-cyan-400 transition-transform duration-700 ease-out"
          style={{ transform: `rotate(${rotation}deg)` }}
        />
      </div>

      <span className="mt-2 text-sm text-gray-700 dark:text-gray-300 font-medium">
        {getWindDirectionLabel(degrees, t)}
      </span>
    </div>
  );
};