export const Jail = () => {
    return (
        <div className='h-full bg-totorolightgreen shadow-lg border border-totorodarkgreen rounded-[5px] relative'>
            <p className='text-center'>Visiting</p>
            <div className='w-[75%] h-[75%] absolute bottom-0 left-0 border-t-2 border-r-2 rounded-tr-[5px] border-black flex justify-between flex-col items-center'>
                <svg className='rotate-90 flex-1 h-full w-full'  xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M760-360v-80H200v80h560Zm0-160v-80H200v80h560Zm0-160v-80H200v80h560ZM200-120q-33 0-56.5-23.5T120-200v-560q0-33 23.5-56.5T200-840h560q33 0 56.5 23.5T840-760v560q0 33-23.5 56.5T760-120H200Zm560-80v-80H200v80h560Z"/></svg>
                <p>Jail</p>
            </div>
        </div>
    )
}