import ReactDOM from 'react-dom/client'
import {App} from './App'
import './index.css'
import { BrowserRouter } from 'react-router-dom';
import { AudioProvider } from './context/AudioProvider';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <AudioProvider>
      <App />
    </AudioProvider>
  </BrowserRouter>
)
