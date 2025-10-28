import { useState, useEffect, useCallback } from 'react';

const useWindowSize = () => {
  // Set the initial window size
  const [windowSize, setWindowSize] = useState({
    width: window.innerWidth,
    height: window.innerHeight,
  });

  // function to manually trigger an update
  const recalculate = useCallback(() => {
    setWindowSize({
      width: window.innerWidth,
      height: window.innerHeight,
    });
  }, []);

  useEffect(() => {

    let timeout: number | null | undefined = null;

    // Define a function to handle the resize event
    // Debounce to prevent spam updates
    const handleResize = () => {
      if(timeout) clearTimeout(timeout)
      timeout = setTimeout(() => {
        setWindowSize({
          width: window.innerWidth,
          height: window.innerHeight,
        });
      }, 100);
    };

    // Add the event listener
    window.addEventListener('resize', handleResize);

    // Cleanup the event listener on component unmount
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  return {
    width:windowSize.width,
    height:windowSize.height,
    recalculate
  };
};

export default useWindowSize;