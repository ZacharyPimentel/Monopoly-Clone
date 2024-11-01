export const SideDot:React.FC<{top:number, left:number}> = ({top,left}) => {
    return (
        <div 
            style={{boxShadow:"inset 2px 2px #d90429",top:`${top}%`, left:`${left}%`}} 
            className='absolute w-[20px] h-[20px] mt-[-10px] mr-[5px] mb-[5px] ml-[-10px] rounded-[20px] bg-[#f25f5c]'>
        </div>
    )
}