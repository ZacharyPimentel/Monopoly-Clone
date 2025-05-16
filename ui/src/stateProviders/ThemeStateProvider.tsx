import { createContext, useCallback, useContext, useState } from 'react';
import { ThemeState } from '../types/stateProviders/ThemeStateProvider';

const ThemeStateContext = createContext<any | null>(null);
const ThemeDispatchContext = createContext<(newState:Partial<ThemeState>) => void>(() => {});

export const ThemeStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const initialThemeState:ThemeState = {}

  const [themeState, setThemeState] = useState<ThemeState>(initialThemeState)
  const updateThemeState = useCallback((newThemeState:Partial<ThemeState>) => {
    setThemeState( (prevState) => {
        return {...prevState, ...newThemeState}
    })
  },[])

  return (
    <ThemeStateContext.Provider value={themeState}>
      <ThemeDispatchContext.Provider value={updateThemeState}>
        {children}
      </ThemeDispatchContext.Provider>
    </ThemeStateContext.Provider>
  );
}

export function useThemeState():ThemeState {
  return useContext(ThemeStateContext);
}

export function useThemeDispatch():React.Dispatch<Partial<ThemeState>>{
  return useContext(ThemeDispatchContext);
}