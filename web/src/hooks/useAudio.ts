import { useRef, useCallback } from 'react';

export const useAudio = () => {
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const playAudio = useCallback((audioPath: string) => {
    try {
      // Create new audio instance if it doesn't exist or if it's a different audio file
      if (!audioRef.current || audioRef.current.src !== audioPath) {
        audioRef.current = new Audio(audioPath);
      }
      
      // Reset audio to beginning and play
      audioRef.current.currentTime = 0;
      audioRef.current.play().catch((error) => {
        console.warn('Audio playback failed:', error);
      });
    } catch (error) {
      console.warn('Audio initialization failed:', error);
    }
  }, []);

  const stopAudio = useCallback(() => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current.currentTime = 0;
    }
  }, []);

  return {
    playAudio,
    stopAudio
  };
};