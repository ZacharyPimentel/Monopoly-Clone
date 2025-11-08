import { AudioFiles } from '@generated';
import React, { createContext, useCallback, useContext, useRef, useState } from 'react';

type AudioContextType = {
  play: (file: AudioFiles) => void;
  volume: number;
  setVolume: (volume: number) => void;
};

const AudioContext = createContext<AudioContextType | null>(null);

const audioFiles = {
  [AudioFiles.PropertyPurchased]: '/audio/purchaseMade.wav',
  [AudioFiles.TurnNotification]: '/audio/turnNotification.wav',
  [AudioFiles.PlayerMovement]: '/audio/playerMoved.wav',
  [AudioFiles.RollStart]: '/audio/woosh.mp3',
  [AudioFiles.RollEnd]: '/audio/tap.wav',
  [AudioFiles.TradeUpdated]: '/audio/notificationDing.mp3',
  [AudioFiles.LandedOnEventTile]: '/audio/event.mp3',
  [AudioFiles.MoneyPaid]: '/audio/chaChing.mp3',
} as const;

export const AudioProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const volumeRef = useRef(0.5);
  const [volume,setVolume] = useState(0.5);

  const audioCache = useRef<Record<AudioFiles, HTMLAudioElement>>({} as Record<AudioFiles, HTMLAudioElement>);
  const play = useCallback((file: AudioFiles) => {
      let audio = audioCache.current[file];
      // Lazy load audio only when first used
      if (!audio) {
        const src = audioFiles[file];
        if (!src) {
          console.warn(`Audio file not found for key: ${file}`);
          return;
        }
        audio = new Audio(src);
        audioCache.current[file] = audio;
      }

      // Always reset volume/time and play fresh
      audio.currentTime = 0;
      audio.volume = volumeRef.current;

      // Play can sometimes reject (e.g., autoplay restrictions)
      audio.play().catch(err => {
        console.log('Audio playback failed:', err);
      });
    },
    []
  );

  const changeVolume = useCallback((newVolume: number) => {
    const clamped = Math.min(Math.max(newVolume, 0), 1);
    volumeRef.current = clamped;
    setVolume(clamped)
  }, []);

  const value: AudioContextType = {
    play,
    volume,
    setVolume: changeVolume,
  };

  return <AudioContext.Provider value={value}>{children}</AudioContext.Provider>;
};

export function useAudio(): AudioContextType {
  const ctx = useContext(AudioContext);
  if (!ctx) throw new Error('useAudio must be used within AudioProvider');
  return ctx;
}
