import { AudioFiles } from '@generated';
import { createContext, useCallback, useContext, useRef } from 'react';

const AudioContext = createContext<any | null>(null);

// PUT ADDITIONAL SOUND PATHS HERE
const audioFiles = {
    [AudioFiles.PropertyPurchased]: '/audio/purchaseMade.wav',
    [AudioFiles.TurnNotification]: '/audio/turnNotification.wav',
    [AudioFiles.PlayerMovement]: '/audio/playerMoved.wav',
    [AudioFiles.RollStart]: '/audio/woosh.mp3',
    [AudioFiles.RollEnd]: '/audio/tap.wav',
    [AudioFiles.TradeUpdated]:'/audio/notificationDing.mp3',
    [AudioFiles.LandedOnEventTile]:'/audio/event.mp3',
    [AudioFiles.MoneyPaid]:'/audio/chaChing.mp3'
} as const;

type AudioControl = {
  play: () => void;
  setVolume: (volume: number) => void;
};

type AudioOptions = Record<AudioFiles, AudioControl>;

export const AudioProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

    //generate ref keys
    const audioRefs = useRef<Record<AudioFiles, HTMLAudioElement>>(
        Object.fromEntries(
            (Object.values(AudioFiles).filter(v => typeof v === 'number') as AudioFiles[])
            .map(key => [key, new Audio(audioFiles[key])])
        ) as Record<AudioFiles, HTMLAudioElement>
    );

    //generate the context object
    const audioOptions = Object.fromEntries(
    (Object.values(AudioFiles).filter(v => typeof v === 'number') as AudioFiles[])
        .map(key => [
        key,
        {
            play: () => playAudio(audioRefs.current[key]),
            setVolume: (volume: number) => setVolume(audioRefs.current[key], volume),
        },
        ])
    ) as AudioOptions;

    const playAudio = useCallback( (audioElement:HTMLAudioElement) => {
        audioElement.currentTime = 0;
        audioElement.play();
    },[])

    const setVolume = useCallback( (audioElement:HTMLAudioElement, volume:number) => {
        audioElement.volume = volume;
    },[])

    return (
        <AudioContext.Provider value={audioOptions}>
            {children}
        </AudioContext.Provider>
    );
}

export function useAudio():AudioOptions {
  return useContext(AudioContext);
}