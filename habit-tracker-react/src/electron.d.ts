// Electron API declarations for TypeScript
export interface ElectronAPI {
  onNavigate: (callback: (route: string) => void) => void;
  showNotification: (title: string, body: string, icon?: string) => void;
  quitApp: () => void;
  platform: string;
  isElectron: boolean;
}

declare global {
  interface Window {
    electron?: ElectronAPI;
  }
}

export {};
