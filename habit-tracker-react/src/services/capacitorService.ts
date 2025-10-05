/**
 * Capacitor Service - Mobile Platform Integration
 * Handles mobile-specific features and platform detection
 */

import { Capacitor } from '@capacitor/core';
import { App } from '@capacitor/app';
import { StatusBar, Style } from '@capacitor/status-bar';
import { SplashScreen } from '@capacitor/splash-screen';
import { Haptics, ImpactStyle, NotificationType } from '@capacitor/haptics';
import { LocalNotifications } from '@capacitor/local-notifications';
import { Toast } from '@capacitor/toast';
import { Keyboard } from '@capacitor/keyboard';

export class CapacitorService {
  private static instance: CapacitorService;
  private initialized = false;

  private constructor() {}

  public static getInstance(): CapacitorService {
    if (!CapacitorService.instance) {
      CapacitorService.instance = new CapacitorService();
    }
    return CapacitorService.instance;
  }

  /**
   * Initialize Capacitor plugins and setup
   */
  public async initialize(): Promise<void> {
    if (this.initialized) return;

    console.log('🚀 Initializing Capacitor Service...');
    console.log('Platform:', Capacitor.getPlatform());
    console.log('Native:', Capacitor.isNativePlatform());

    if (this.isNative()) {
      await this.setupStatusBar();
      await this.setupSplashScreen();
      await this.setupAppListeners();
      await this.setupKeyboard();
    }

    this.initialized = true;
    console.log('✅ Capacitor Service initialized');
  }

  // ==================== Platform Detection ====================

  /**
   * Check if running on native platform (iOS/Android)
   */
  public isNative(): boolean {
    return Capacitor.isNativePlatform();
  }

  /**
   * Check if running on iOS
   */
  public isIOS(): boolean {
    return Capacitor.getPlatform() === 'ios';
  }

  /**
   * Check if running on Android
   */
  public isAndroid(): boolean {
    return Capacitor.getPlatform() === 'android';
  }

  /**
   * Check if running on web
   */
  public isWeb(): boolean {
    return Capacitor.getPlatform() === 'web';
  }

  /**
   * Get current platform
   */
  public getPlatform(): string {
    return Capacitor.getPlatform();
  }

  // ==================== Status Bar ====================

  /**
   * Setup status bar styling
   */
  private async setupStatusBar(): Promise<void> {
    try {
      await StatusBar.setStyle({ style: Style.Light });
      
      if (this.isAndroid()) {
        await StatusBar.setBackgroundColor({ color: '#6366F1' });
      }
    } catch (error) {
      console.error('Error setting up status bar:', error);
    }
  }

  /**
   * Show status bar
   */
  public async showStatusBar(): Promise<void> {
    if (this.isNative()) {
      await StatusBar.show();
    }
  }

  /**
   * Hide status bar
   */
  public async hideStatusBar(): Promise<void> {
    if (this.isNative()) {
      await StatusBar.hide();
    }
  }

  // ==================== Splash Screen ====================

  /**
   * Setup splash screen
   */
  private async setupSplashScreen(): Promise<void> {
    try {
      // Splash screen will auto-hide based on config
      // Or manually hide after app is ready
      setTimeout(() => {
        SplashScreen.hide();
      }, 2000);
    } catch (error) {
      console.error('Error setting up splash screen:', error);
    }
  }

  /**
   * Hide splash screen manually
   */
  public async hideSplashScreen(): Promise<void> {
    if (this.isNative()) {
      await SplashScreen.hide();
    }
  }

  // ==================== Haptics/Vibration ====================

  /**
   * Trigger light haptic feedback
   */
  public async hapticsLight(): Promise<void> {
    if (this.isNative()) {
      try {
        await Haptics.impact({ style: ImpactStyle.Light });
      } catch (error) {
        console.error('Haptics error:', error);
      }
    }
  }

  /**
   * Trigger medium haptic feedback
   */
  public async hapticsMedium(): Promise<void> {
    if (this.isNative()) {
      try {
        await Haptics.impact({ style: ImpactStyle.Medium });
      } catch (error) {
        console.error('Haptics error:', error);
      }
    }
  }

  /**
   * Trigger heavy haptic feedback
   */
  public async hapticsHeavy(): Promise<void> {
    if (this.isNative()) {
      try {
        await Haptics.impact({ style: ImpactStyle.Heavy });
      } catch (error) {
        console.error('Haptics error:', error);
      }
    }
  }

  /**
   * Trigger success vibration
   */
  public async vibrateSuccess(): Promise<void> {
    if (this.isNative()) {
      try {
        await Haptics.notification({ type: NotificationType.Success });
      } catch (error) {
        console.error('Haptics error:', error);
      }
    }
  }

  /**
   * Trigger error vibration
   */
  public async vibrateError(): Promise<void> {
    if (this.isNative()) {
      try {
        await Haptics.notification({ type: NotificationType.Error });
      } catch (error) {
        console.error('Haptics error:', error);
      }
    }
  }

  // ==================== Local Notifications ====================

  /**
   * Request notification permissions
   */
  public async requestNotificationPermissions(): Promise<boolean> {
    if (!this.isNative()) return false;

    try {
      const result = await LocalNotifications.requestPermissions();
      return result.display === 'granted';
    } catch (error) {
      console.error('Error requesting notification permissions:', error);
      return false;
    }
  }

  /**
   * Check notification permissions
   */
  public async checkNotificationPermissions(): Promise<boolean> {
    if (!this.isNative()) return false;

    try {
      const result = await LocalNotifications.checkPermissions();
      return result.display === 'granted';
    } catch (error) {
      console.error('Error checking notification permissions:', error);
      return false;
    }
  }

  /**
   * Schedule a local notification
   */
  public async scheduleNotification(options: {
    id: number;
    title: string;
    body: string;
    schedule?: Date;
    extra?: any;
  }): Promise<void> {
    if (!this.isNative()) {
      console.log('Not native platform, skipping notification');
      return;
    }

    try {
      const hasPermission = await this.checkNotificationPermissions();
      if (!hasPermission) {
        const granted = await this.requestNotificationPermissions();
        if (!granted) {
          console.warn('Notification permission denied');
          return;
        }
      }

      await LocalNotifications.schedule({
        notifications: [{
          id: options.id,
          title: options.title,
          body: options.body,
          schedule: options.schedule ? { at: options.schedule } : undefined,
          extra: options.extra,
        }]
      });

      console.log('✅ Notification scheduled:', options.title);
    } catch (error) {
      console.error('Error scheduling notification:', error);
    }
  }

  /**
   * Cancel a notification
   */
  public async cancelNotification(id: number): Promise<void> {
    if (this.isNative()) {
      await LocalNotifications.cancel({ notifications: [{ id }] });
    }
  }

  /**
   * Cancel all notifications
   */
  public async cancelAllNotifications(): Promise<void> {
    if (this.isNative()) {
      const pending = await LocalNotifications.getPending();
      if (pending.notifications.length > 0) {
        await LocalNotifications.cancel({ notifications: pending.notifications });
      }
    }
  }

  // ==================== Toast Messages ====================

  /**
   * Show a toast message (native on mobile, can fallback to custom on web)
   */
  public async showToast(text: string, duration: 'short' | 'long' = 'short'): Promise<void> {
    if (this.isNative()) {
      try {
        await Toast.show({
          text,
          duration: duration,
          position: 'bottom'
        });
      } catch (error) {
        console.error('Error showing toast:', error);
      }
    } else {
      // Fallback for web - could use your existing notification system
      console.log('Toast:', text);
    }
  }

  // ==================== Keyboard ====================

  /**
   * Setup keyboard handling
   */
  private async setupKeyboard(): Promise<void> {
    if (this.isNative()) {
      try {
        // Add keyboard listeners if needed
        Keyboard.addListener('keyboardWillShow', info => {
          console.log('Keyboard will show:', info.keyboardHeight);
        });

        Keyboard.addListener('keyboardWillHide', () => {
          console.log('Keyboard will hide');
        });
      } catch (error) {
        console.error('Error setting up keyboard:', error);
      }
    }
  }

  /**
   * Hide keyboard
   */
  public async hideKeyboard(): Promise<void> {
    if (this.isNative()) {
      await Keyboard.hide();
    }
  }

  // ==================== App Lifecycle ====================

  /**
   * Setup app lifecycle listeners
   */
  private async setupAppListeners(): Promise<void> {
    try {
      // App state change
      App.addListener('appStateChange', ({ isActive }) => {
        console.log('App state changed. Is active:', isActive);
        // You can trigger data sync here when app becomes active
      });

      // URL Open (for deep linking)
      App.addListener('appUrlOpen', (data) => {
        console.log('App opened with URL:', data.url);
        // Handle deep links here
      });

      // Back button (Android)
      if (this.isAndroid()) {
        App.addListener('backButton', ({ canGoBack }) => {
          if (!canGoBack) {
            App.exitApp();
          } else {
            window.history.back();
          }
        });
      }
    } catch (error) {
      console.error('Error setting up app listeners:', error);
    }
  }

  /**
   * Get app info
   */
  public async getAppInfo(): Promise<{
    name: string;
    id: string;
    build: string;
    version: string;
  } | null> {
    if (this.isNative()) {
      return await App.getInfo();
    }
    return null;
  }

  /**
   * Exit app (Android only)
   */
  public async exitApp(): Promise<void> {
    if (this.isAndroid()) {
      await App.exitApp();
    }
  }
}

// Export singleton instance
export const capacitorService = CapacitorService.getInstance();
