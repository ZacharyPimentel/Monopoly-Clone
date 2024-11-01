import ReactDOM from 'react-dom/client'
import {App} from './App.tsx'
import './index.css'
import { BrowserRouter } from 'react-router-dom';

import * as signalR from '@microsoft/signalr';

//set up the web socket connection
//@ts-ignore
window.socketConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5014/monopoly",{
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets
    })
    .build();
    //@ts-ignore
    window.socketConnection.start();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
)
