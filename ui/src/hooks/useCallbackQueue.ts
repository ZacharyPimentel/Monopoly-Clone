import { useCallback, useRef } from "react";

type Queue = {
  processingQueue: boolean;
  queue: Function[];
};

export const useCallbackQueue = (delay:number) => {

    const queueRef = useRef<Queue>({
        processingQueue: false,
        queue: []
    });

    const processNextCallback = useCallback(() => {
        if(!queueRef.current) return
        const nextCallback = queueRef.current.queue.shift();
        if(nextCallback){
            nextCallback();
            setTimeout( () => {
                processNextCallback()
            },delay)
        }else{
            queueRef.current.processingQueue = false;
        }
    },[]);

    const pushToQueue = useCallback((callback:Function) => {
        queueRef.current.queue.push(callback);
        if (!queueRef.current.processingQueue) {
            queueRef.current.processingQueue = true;
            processNextCallback();
        }
    },[])

    return {
        pushToQueue,
        queueProcessing: () => queueRef.current.processingQueue
    }
};
