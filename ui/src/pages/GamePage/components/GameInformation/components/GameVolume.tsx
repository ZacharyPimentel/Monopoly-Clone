import { useAudio } from "@context"
import { Volume2, VolumeOff } from "lucide-react"
import { useState } from "react";

export const GameVolume = () => {

    const audio = useAudio();
    const [localVolume, setLocalVolume] = useState(audio.volume)

    return(
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            <p className='font-bold'>Volume</p>
            <div className='flex gap-[20px]'>
                <button onClick={() => {
                    if(audio.volume > 0){
                        setLocalVolume(audio.volume);
                        audio.setVolume(0);
                    }else{
                        audio.setVolume(localVolume)
                    }
                }}>
                    {audio.volume == 0
                        ?   <VolumeOff/>
                        :   <Volume2/>
                    }
                </button>
                <input 
                    value={audio.volume} 
                    min={0} 
                    max={1} 
                    step={0.1} 
                    type='range'
                    onChange={(e) => {
                        setLocalVolume(audio.volume);
                        audio.setVolume(parseFloat(e.target.value))
                    }}
                />
            </div>
        </div>
    )
}