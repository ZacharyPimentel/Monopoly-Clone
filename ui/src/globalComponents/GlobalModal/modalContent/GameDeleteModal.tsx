import { Game } from "@generated";
import { ActionButtons } from "@globalComponents/GlobalModal";
import { useWebSocket } from "@hooks";

export const GameDeleteModal:React.FC<{game: Game}> = ({game}) => {

    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Delete Game</p>
            <p>Do you want to delete <b>{game.gameName}</b>?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.game.archive(game.id)
                }} 
                confirmButtonStyle={"warning"} 
                confirmButtonText={"Delete"}
            />
        </div>
    )
}