import { ReactNode, useEffect, useRef, useState } from "react"

export const SubmenuWrapper:React.FC<{
    submenuOpen:boolean,
    updateHeightDeps?:any[],
    children:ReactNode
    side: 'top' | 'right' | 'bottom' | 'left'
}>= ({submenuOpen,updateHeightDeps = [],children}) => {

    const submenuRef = useRef<HTMLDivElement | null>(null)
    const [submenuHeight,setSubmenuHeight] = useState(0);

    //update the submenu height if it's toggled open/closed or if any of the passed deps change
    useEffect( () => {
        if(!submenuRef.current)return;
        if(submenuOpen){
            setSubmenuHeight(submenuRef.current.offsetHeight);
        }else{
            setSubmenuHeight(0);
        }
    },[submenuOpen,[...updateHeightDeps]]);

    return (
        <div style={{height:submenuHeight}} className='max-h-[200px] absolute bg-white top-[100%] left-0 w-full overflow-y-scroll shadow-lg'>
            <div ref={submenuRef}>
                {children}
            </div>
        </div>
    )
}