import { electronService } from './electronService';
import { capacitorService } from './capacitorService';

export interface ReminderSettings {
  habitId: number;
  habitName: string;
  enabled: boolean;
  time: string; // HH:mm format
  days: number[]; // 0-6 (Sunday-Saturday)
  sound: boolean;
  vibrate: boolean;
}

export interface NotificationHistoryItem {
  id: number;
  habitId: number;
  habitName: string;
  timestamp: string;
  action: 'viewed' | 'dismissed' | 'snoozed' | 'completed';
}

class NotificationService {
  private permission: NotificationPermission = 'default';
  private reminders: Map<number, ReminderSettings> = new Map();
  private timers: Map<number, NodeJS.Timeout> = new Map();
  private history: NotificationHistoryItem[] = [];
  private snoozedReminders: Map<number, Date> = new Map();

  constructor() {
    this.initialize();
  }

  async initialize() {
    // Initialize Capacitor if on mobile
    if (capacitorService.isNative()) {
      await capacitorService.initialize();
    }

    // Check if browser supports notifications (web only)
    if (!capacitorService.isNative() && !('Notification' in window)) {
      console.warn('This browser does not support desktop notifications');
      return;
    }

    // Get current permission
    if (capacitorService.isNative()) {
      const hasPermission = await capacitorService.checkNotificationPermissions();
      this.permission = hasPermission ? 'granted' : 'default';
    } else {
      this.permission = Notification.permission;
    }

    // Load reminders from localStorage
    this.loadReminders();
    this.loadHistory();

    // Start checking for due reminders
    this.startReminderCheck();

    // Listen for visibility changes (re-check when tab becomes visible)
    document.addEventListener('visibilitychange', () => {
      if (!document.hidden) {
        this.checkDueReminders();
      }
    });
  }

  async requestPermission(): Promise<boolean> {
    // Handle mobile platforms
    if (capacitorService.isNative()) {
      const granted = await capacitorService.requestNotificationPermissions();
      this.permission = granted ? 'granted' : 'denied';
      return granted;
    }

    // Handle web
    if (!('Notification' in window)) {
      return false;
    }

    if (this.permission === 'granted') {
      return true;
    }

    try {
      const permission = await Notification.requestPermission();
      this.permission = permission;
      return permission === 'granted';
    } catch (error) {
      console.error('Error requesting notification permission:', error);
      return false;
    }
  }

  getPermission(): NotificationPermission {
    return this.permission;
  }

  // Save reminder settings
  saveReminder(settings: ReminderSettings): void {
    this.reminders.set(settings.habitId, settings);
    this.saveReminders();
    this.scheduleReminder(settings);
  }

  // Get reminder for habit
  getReminder(habitId: number): ReminderSettings | undefined {
    return this.reminders.get(habitId);
  }

  // Get all reminders
  getAllReminders(): ReminderSettings[] {
    return Array.from(this.reminders.values());
  }

  // Delete reminder
  deleteReminder(habitId: number): void {
    this.reminders.delete(habitId);
    this.cancelTimer(habitId);
    this.saveReminders();
  }

  // Schedule reminder check
  private scheduleReminder(settings: ReminderSettings): void {
    if (!settings.enabled) {
      this.cancelTimer(settings.habitId);
      return;
    }

    // Cancel existing timer
    this.cancelTimer(settings.habitId);

    // Check immediately if due
    this.checkIfDue(settings);
  }

  // Check if reminder is due
  private checkIfDue(settings: ReminderSettings): void {
    const now = new Date();
    const today = now.getDay();

    // Check if today is a scheduled day
    if (!settings.days.includes(today)) {
      return;
    }

    // Check if time matches (within 1 minute)
    const [targetHour, targetMinute] = settings.time.split(':').map(Number);
    const currentHour = now.getHours();
    const currentMinute = now.getMinutes();

    if (currentHour === targetHour && currentMinute === targetMinute) {
      // Check if not snoozed
      const snoozedUntil = this.snoozedReminders.get(settings.habitId);
      if (snoozedUntil && now < snoozedUntil) {
        return; // Still snoozed
      }

      this.showNotification(settings);
    }
  }

  // Start periodic reminder check (every minute)
  private startReminderCheck(): void {
    setInterval(() => {
      this.checkDueReminders();
    }, 60000); // Check every minute

    // Initial check
    this.checkDueReminders();
  }

  // Check all reminders for due times
  private checkDueReminders(): void {
    for (const settings of this.reminders.values()) {
      if (settings.enabled) {
        this.checkIfDue(settings);
      }
    }
  }

  // Show notification using the appropriate method
  private async showNotification(settings: ReminderSettings): Promise<void> {
    console.log('Showing notification for:', settings.habitName);

    // Mobile (Capacitor)
    if (capacitorService.isNative()) {
      try {
        await capacitorService.scheduleNotification({
          id: settings.habitId,
          title: `⏰ ${settings.habitName}`,
          body: 'Time to complete your habit!',
          extra: {
            habitId: settings.habitId,
            habitName: settings.habitName,
          },
        });

        // Vibrate if enabled
        if (settings.vibrate) {
          await capacitorService.hapticsMedium();
        }

        this.addToHistory({
          id: Date.now(),
          habitId: settings.habitId,
          habitName: settings.habitName,
          timestamp: new Date().toISOString(),
          action: 'viewed',
        });

        return;
      } catch (error) {
        console.error('Error showing mobile notification:', error);
        return;
      }
    }

    // Desktop (Electron)
    if (electronService.isElectron()) {
      electronService.showNotification(
        `⏰ ${settings.habitName}`,
        'Time to complete your habit!',
        '/icon.png'
      );

      this.addToHistory({
        id: Date.now(),
        habitId: settings.habitId,
        habitName: settings.habitName,
        timestamp: new Date().toISOString(),
        action: 'viewed',
      });
      return;
    }

    // Fallback to browser notifications
    if (this.permission !== 'granted') {
      const granted = await this.requestPermission();
      if (!granted) return;
    }

    try {
      const notification = new Notification(`⏰ ${settings.habitName}`, {
        body: `Time to complete your habit!`,
        icon: '/icon.png', // Optional: Add app icon
        badge: '/badge.png', // Optional: Add badge icon
        tag: `habit-${settings.habitId}`, // Prevent duplicates
        requireInteraction: true, // Don't auto-dismiss
        silent: !settings.sound,
        data: {
          habitId: settings.habitId,
          habitName: settings.habitName,
        },
      });

      // Handle notification click
      notification.onclick = () => {
        window.focus();
        this.addToHistory({
          id: Date.now(),
          habitId: settings.habitId,
          habitName: settings.habitName,
          timestamp: new Date().toISOString(),
          action: 'viewed',
        });
        notification.close();
        
        // Navigate to Today view
        window.location.href = '/today';
      };

      // Handle notification close
      notification.onclose = () => {
        this.addToHistory({
          id: Date.now(),
          habitId: settings.habitId,
          habitName: settings.habitName,
          timestamp: new Date().toISOString(),
          action: 'dismissed',
        });
      };
    } catch (error) {
      console.error('Error showing notification:', error);
    }
  }

  // Snooze reminder (15 minutes)
  snoozeReminder(habitId: number, minutes: number = 15): void {
    const snoozedUntil = new Date();
    snoozedUntil.setMinutes(snoozedUntil.getMinutes() + minutes);
    this.snoozedReminders.set(habitId, snoozedUntil);

    const settings = this.reminders.get(habitId);
    if (settings) {
      this.addToHistory({
        id: Date.now(),
        habitId,
        habitName: settings.habitName,
        timestamp: new Date().toISOString(),
        action: 'snoozed',
      });
    }
  }

  // Cancel timer for habit
  private cancelTimer(habitId: number): void {
    const timer = this.timers.get(habitId);
    if (timer) {
      clearTimeout(timer);
      this.timers.delete(habitId);
    }
  }

  // Notification history
  getHistory(): NotificationHistoryItem[] {
    return this.history.slice().reverse(); // Most recent first
  }

  clearHistory(): void {
    this.history = [];
    localStorage.removeItem('notificationHistory');
  }

  private addToHistory(item: NotificationHistoryItem): void {
    this.history.push(item);
    
    // Keep only last 100 items
    if (this.history.length > 100) {
      this.history = this.history.slice(-100);
    }
    
    this.saveHistory();
  }

  // LocalStorage persistence
  private saveReminders(): void {
    const data = Array.from(this.reminders.values());
    localStorage.setItem('habitReminders', JSON.stringify(data));
  }

  private loadReminders(): void {
    try {
      const data = localStorage.getItem('habitReminders');
      if (data) {
        const reminders: ReminderSettings[] = JSON.parse(data);
        reminders.forEach(reminder => {
          this.reminders.set(reminder.habitId, reminder);
        });
      }
    } catch (error) {
      console.error('Error loading reminders:', error);
    }
  }

  private saveHistory(): void {
    localStorage.setItem('notificationHistory', JSON.stringify(this.history));
  }

  private loadHistory(): void {
    try {
      const data = localStorage.getItem('notificationHistory');
      if (data) {
        this.history = JSON.parse(data);
      }
    } catch (error) {
      console.error('Error loading notification history:', error);
    }
  }

  // Test notification
  async testNotification(): Promise<void> {
    if (this.permission !== 'granted') {
      const granted = await this.requestPermission();
      if (!granted) return;
    }

    const notification = new Notification('🎉 Test Notification', {
      body: 'Notifications are working! You\'ll receive reminders for your habits.',
      icon: '/icon.png',
      requireInteraction: false,
    });

    setTimeout(() => notification.close(), 5000);
  }
}

// Singleton instance
export const notificationService = new NotificationService();

// Format time for display
export function formatTime(time: string): string {
  const [hour, minute] = time.split(':').map(Number);
  const ampm = hour >= 12 ? 'PM' : 'AM';
  const displayHour = hour % 12 || 12;
  return `${displayHour}:${minute.toString().padStart(2, '0')} ${ampm}`;
}

// Get day name
export function getDayName(day: number): string {
  const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  return days[day];
}

// Get day abbreviation
export function getDayAbbr(day: number): string {
  const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  return days[day];
}
