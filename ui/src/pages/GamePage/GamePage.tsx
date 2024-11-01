import { GameBoard } from "./components/gameBoard/GameBoard";
export const GamePage = () => {
    return (
        <div className='flex flex-col'>
            {/* <Navbar/> */}
            <GameBoard/>
        </div>
    )
}