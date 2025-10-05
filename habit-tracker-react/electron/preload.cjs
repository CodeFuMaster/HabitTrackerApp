const { contextBridge, ipcRenderer } = require('electron');

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electron', {
  // Navigation
  onNavigate: (callback) => {
    ipcRenderer.on('navigate', (event, route) => callback(route));
  },
  
  // Notifications
  showNotification: (title, body, icon) => {
    ipcRenderer.send('show-notification', { title, body, icon });
  },
  
  // App control
  quitApp: () => {
    ipcRenderer.send('app-quit');
  },
  
  // System info
  platform: process.platform,
  isElectron: true,
});
