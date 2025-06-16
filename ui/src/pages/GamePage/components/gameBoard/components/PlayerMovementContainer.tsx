import { useGameState } from "@stateProviders/GameStateProvider";
import { useEffect, useMemo, useRef } from "react";

export const PlayerMovementContainer:React.FC<{
    tileRefs: React.MutableRefObject<Record<number, HTMLDivElement | null>>
}> = ({tileRefs}) => {

    const divRef = useRef<HTMLDivElement>(null);
    const gameState = useGameState();
    const currentPlayer = gameState?.players.find( player => player.id === gameState?.game?.currentPlayerTurn);
    
    const tileCoordinates = useMemo( () => {
    const prevTile = tileRefs.current[currentPlayer!.previousBoardSpaceId];
    const currentTile = tileRefs.current[currentPlayer!.boardSpaceId];

    if(!prevTile || !currentTile) return
    const prevTileBbox = prevTile.getBoundingClientRect();
    const currentTileBbox = currentTile.getBoundingClientRect();
    
    return {
        x1: prevTileBbox.x + (prevTileBbox.width / 2) - 15,
        x2: currentTileBbox.x + (currentTileBbox.width / 2) - 30,
        y1: prevTileBbox.y + (prevTileBbox.height / 2) - 15,
        y2: currentTileBbox.y + (currentTileBbox.height / 2) - 30
    }

    },[currentPlayer])

    useEffect(() => {
        const div = divRef.current;
        if (!div) return;

        //Set initial position
        div.style.transform = `translate(${tileCoordinates?.x1}px, ${tileCoordinates?.y1}px)`;

        //Trigger reflow to force browser to register initial position
        //before animating to end position
        requestAnimationFrame(() => {
        div.style.transform = `translate(${tileCoordinates?.x2}px, ${tileCoordinates?.y2}px)`;
        });
    }, [tileCoordinates]);

    return(<>
        {/* <div style={{
            top:(tileCoordinates?.y1 || 0),
            left:(tileCoordinates?.x1 || 0)
        }} className='absolute w-[30px] h-[30px] bg-[blue] z-[20]'></div>
        <div style={{
            top:tileCoordinates?.y2,
            left:tileCoordinates?.x2
        }} className='absolute w-[30px] h-[30px] bg-[yellow] z-[20]'></div> */}
        <div
            ref={divRef}
            style={{
                position: 'absolute',
                width: 30,
                height: 30,
                transform:`translate(${tileCoordinates?.x1}px, ${tileCoordinates?.y1}px)`,
                zIndex:20,
                transition: `transform 500ms ease-in-out`,
                pointerEvents: 'none', // optional
            }}
        >
            <span className={'bg-pink-500 animate-ping absolute w-full h-full rounded-[50%]'}></span>
            <img 
                className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative pointer-events-none' 
                src={currentPlayer?.iconUrl}
            />
        </div>
    </>)
}