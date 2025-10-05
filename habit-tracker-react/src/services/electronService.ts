/**
 * Electron Integration Service
 * Provides native desktop features when running in Electron
 */

export class ElectronService {
  private static instance: ElectronService;

  private constructor() {
    this.initialize();
  }

  public static getInstance(): ElectronService {
    if (!ElectronService.instance) {
      ElectronService.instance = new ElectronService();
    }
    return ElectronService.instance;
  }

  /**
   * Check if app is running in Electron
   */
  public isElectron(): boolean {
    return typeof window !== 'undefined' && !!window.electron;
  }

  /**
   * Initialize Electron-specific features
   */
  private initialize(): void {
    if (this.isElectron() && window.electron) {
      // Listen for navigation events from main process
      window.electron.onNavigate((route: string) => {
        // Navigate to the route
        if (window.location.pathname !== route) {
          window.location.href = route;
        }
      });

      console.log('✅ Electron integration initialized');
      console.log('Platform:', window.electron.platform);
    }
  }

  /**
   * Show native notification (uses Electron if available, falls back to browser)
   */
  public showNotification(title: string, body: string, icon?: string): void {
    if (this.isElectron() && window.electron) {
      // Use Electron's native notification
      window.electron.showNotification(title, body, icon);
    } else {
      // Fallback to browser notification
      if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, {
          body,
          icon,
        });
      }
    }
  }

  /**
   * Get platform information
   */
  public getPlatform(): string {
    if (this.isElectron() && window.electron) {
      return window.electron.platform;
    }
    return 'web';
  }

  /**
   * Quit the application (Electron only)
   */
  public quitApp(): void {
    if (this.isElectron() && window.electron) {
      window.electron.quitApp();
    } else {
      console.warn('quitApp() is only available in Electron');
    }
  }

  /**
   * Check if running on Windows
   */
  public isWindows(): boolean {
    const platform = this.getPlatform();
    return platform === 'win32';
  }

  /**
   * Check if running on macOS
   */
  public isMac(): boolean {
    const platform = this.getPlatform();
    return platform === 'darwin';
  }

  /**
   * Check if running on Linux
   */
  public isLinux(): boolean {
    const platform = this.getPlatform();
    return platform === 'linux';
  }
}

// Export singleton instance
export const electronService = ElectronService.getInstance();
