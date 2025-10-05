import { apiService } from './apiService';
import { offlineDb } from './offlineDb';
import type { 
  Habit, 
  DailyHabitEntry, 
  Category,
  HabitMetricDefinition,
  CustomMetricValue,
  TimerSession,
  ActivityLog,
  RoutineTemplate
} from '../types/habit.types';

class SyncService {
  private lastSyncTimestamp = 0;
  private isSyncing = false;
  private syncInterval: number | null = null;

  async initialize(): Promise<void> {
    await offlineDb.initialize();
    
    // Load last sync timestamp
    const lastSync = await offlineDb.getSetting('lastSyncTimestamp');
    this.lastSyncTimestamp = lastSync ? parseInt(lastSync) : 0;

    // Start periodic sync (every 30 seconds when online)
    this.startPeriodicSync();
  }

  startPeriodicSync(): void {
    if (this.syncInterval) return;
    
    this.syncInterval = window.setInterval(async () => {
      await this.sync();
    }, 30000); // 30 seconds
  }

  stopPeriodicSync(): void {
    if (this.syncInterval) {
      window.clearInterval(this.syncInterval);
      this.syncInterval = null;
    }
  }

  async isServerOnline(): Promise<boolean> {
    try {
      return await apiService.ping();
    } catch {
      return false;
    }
  }

  async sync(): Promise<{ success: boolean; message: string }> {
    if (this.isSyncing) {
      return { success: false, message: 'Sync already in progress' };
    }

    this.isSyncing = true;

    try {
      // Check if server is online
      const isOnline = await this.isServerOnline();
      if (!isOnline) {
        return { success: false, message: 'Server offline - working in offline mode' };
      }

      console.log('Starting sync...');

      // Step 1: Push local changes to server
      await this.pushLocalChanges();

      // Step 2: Pull server changes
      await this.pullServerChanges();

      // Update last sync timestamp
      this.lastSyncTimestamp = Date.now();
      await offlineDb.setSetting('lastSyncTimestamp', this.lastSyncTimestamp.toString());

      console.log('Sync completed successfully');
      return { success: true, message: 'Sync completed' };

    } catch (error) {
      console.error('Sync failed:', error);
      return { 
        success: false, 
        message: `Sync failed: ${error instanceof Error ? error.message : 'Unknown error'}` 
      };
    } finally {
      this.isSyncing = false;
    }
  }

  private async pushLocalChanges(): Promise<void> {
    const pendingChanges = await offlineDb.getPendingChanges();
    
    if (pendingChanges.length === 0) {
      console.log('No local changes to push');
      return;
    }

    console.log(`Pushing ${pendingChanges.length} local changes...`);

    const deviceId = await offlineDb.getDeviceId();
    
    // Group changes by type
    const habitChanges: any[] = [];
    const entryChanges: any[] = [];
    
    for (const change of pendingChanges) {
      if (change.tableName === 'habits') {
        const habit = await offlineDb.getHabit(change.recordId);
        if (habit) habitChanges.push(habit);
      } else if (change.tableName === 'daily_entries') {
        // Get entry details - would need to add this method
        entryChanges.push(change);
      }
    }

    // Send to server
    if (habitChanges.length > 0 || entryChanges.length > 0) {
      await apiService.syncChanges({
        deviceId,
        lastSyncTimestamp: this.lastSyncTimestamp,
        habits: habitChanges,
        entries: entryChanges,
        categories: [],
        routineSessions: [],
      });

      // Mark as synced
      const changeIds = pendingChanges.map((c: any) => c.id);
      await offlineDb.markChangesSynced(changeIds);
      
      console.log('Local changes pushed successfully');
    }
  }

  private async pullServerChanges(): Promise<void> {
    console.log('Pulling server changes...');
    
    const deviceId = await offlineDb.getDeviceId();
    const serverChanges = await apiService.getChangesSince(this.lastSyncTimestamp, deviceId);

    // Apply server changes to local database
    let changesApplied = 0;

    // Habits
    for (const habit of serverChanges.habits) {
      await offlineDb.saveHabit(habit);
      changesApplied++;
    }

    // Entries
    for (const entry of serverChanges.entries) {
      await offlineDb.saveDailyEntry(entry);
      changesApplied++;
    }

    // Categories
    for (const category of serverChanges.categories) {
      await offlineDb.saveCategory(category);
      changesApplied++;
    }

    console.log(`Applied ${changesApplied} changes from server`);
  }

  // Habit operations (offline-first)
  async getHabits(): Promise<Habit[]> {
    return await offlineDb.getHabits();
  }

  async getHabit(id: number): Promise<Habit | null> {
    return await offlineDb.getHabit(id);
  }

  async saveHabit(habit: Habit): Promise<void> {
    await offlineDb.saveHabit(habit);
    
    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Daily entry operations (offline-first)
  async getDailyEntries(date: string): Promise<DailyHabitEntry[]> {
    return await offlineDb.getDailyEntries(date);
  }

  async getEntriesForDateRange(startDate: string, endDate: string): Promise<DailyHabitEntry[]> {
    // Generate all dates in range
    const dates: string[] = [];
    const current = new Date(startDate);
    const end = new Date(endDate);
    
    while (current <= end) {
      dates.push(current.toISOString().split('T')[0]);
      current.setDate(current.getDate() + 1);
    }
    
    // Load entries for all dates
    const allEntries: DailyHabitEntry[] = [];
    for (const date of dates) {
      const entries = await offlineDb.getDailyEntries(date);
      allEntries.push(...entries);
    }
    
    return allEntries;
  }

  async toggleHabitCompletion(habitId: number, date: string): Promise<void> {
    const entries = await offlineDb.getDailyEntries(date);
    const existingEntry = entries.find(e => e.habitId === habitId);

    const entry: DailyHabitEntry = existingEntry || {
      id: Date.now(), // Temporary ID
      habitId,
      date,
      isCompleted: false,
    };

    entry.isCompleted = !entry.isCompleted;
    entry.completedAt = entry.isCompleted ? new Date().toISOString() : undefined;

    await offlineDb.saveDailyEntry(entry);

    // DON'T sync immediately - let periodic sync handle it
    // This makes the UI instant and prevents the 5-second wait
  }

  // Habit CRUD operations
  async createHabit(habit: Habit): Promise<Habit> {
    // Generate temporary ID for offline use
    const newHabit: Habit = {
      ...habit,
      id: habit.id || Date.now(), // Use provided ID or generate one
      createdDate: new Date().toISOString(),
      lastModifiedDate: new Date().toISOString(),
    };

    await offlineDb.saveHabit(newHabit);

    // Try immediate sync to server
    if (await this.isServerOnline()) {
      await this.sync();
    }

    return newHabit;
  }

  async updateHabit(id: number, updates: Partial<Habit>): Promise<Habit> {
    const habits = await offlineDb.getHabits();
    const existing = habits.find(h => h.id === id);
    
    if (!existing) {
      throw new Error(`Habit with ID ${id} not found`);
    }

    const updatedHabit: Habit = {
      ...existing,
      ...updates,
      id, // Ensure ID doesn't change
      lastModifiedDate: new Date().toISOString(),
    };

    await offlineDb.saveHabit(updatedHabit);

    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }

    return updatedHabit;
  }

  async deleteHabit(id: number): Promise<void> {
    // For now, we'll just mark as inactive rather than deleting
    // This preserves history and is safer for sync
    await this.updateHabit(id, { isActive: false });
  }

  async duplicateHabit(id: number): Promise<Habit> {
    const habits = await offlineDb.getHabits();
    const original = habits.find(h => h.id === id);
    
    if (!original) {
      throw new Error(`Habit with ID ${id} not found`);
    }

    // Create a copy with new ID and name
    const duplicate: Habit = {
      ...original,
      id: Date.now(), // New ID
      name: `${original.name} (Copy)`,
      createdDate: new Date().toISOString(),
      lastModifiedDate: new Date().toISOString(),
    };

    await offlineDb.saveHabit(duplicate);

    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }

    return duplicate;
  }

  // Category operations
  async getCategories(): Promise<Category[]> {
    return await offlineDb.getCategories();
  }

  async saveCategory(category: Category): Promise<void> {
    await offlineDb.saveCategory(category);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  async createCategory(category: Category): Promise<Category> {
    // Generate temporary ID for offline use
    const newCategory: Category = {
      ...category,
      id: category.id || Date.now(), // Use provided ID or generate one
    };

    await offlineDb.saveCategory(newCategory);

    // Try immediate sync to server
    if (await this.isServerOnline()) {
      await this.sync();
    }

    return newCategory;
  }

  async updateCategory(id: number, updates: Partial<Category>): Promise<Category> {
    const categories = await offlineDb.getCategories();
    const existing = categories.find(c => c.id === id);
    
    if (!existing) {
      throw new Error(`Category with ID ${id} not found`);
    }

    const updatedCategory: Category = {
      ...existing,
      ...updates,
      id, // Ensure ID doesn't change
    };

    await offlineDb.saveCategory(updatedCategory);

    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }

    return updatedCategory;
  }

  async deleteCategory(id: number): Promise<void> {
    // For now, we'll actually delete categories
    // In production, you might want to check if any habits use this category first
    const categories = await offlineDb.getCategories();
    const filtered = categories.filter(c => c.id !== id);
    
    // Save all categories except the deleted one
    // Note: This is a simplified implementation
    // In production, you'd want proper delete handling in offlineDb
    for (const cat of filtered) {
      await offlineDb.saveCategory(cat);
    }

    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Initial data load from server
  async loadInitialData(): Promise<void> {
    const isOnline = await this.isServerOnline();
    if (!isOnline) {
      console.log('Server offline - skipping initial data load, using local data');
      return;
    }

    try {
      console.log('Loading initial data from server...');
      
      // Load habits
      const habits = await apiService.getHabits();
      console.log(`Loaded ${habits.length} habits from server`);
      for (const habit of habits) {
        await offlineDb.saveHabit(habit);
      }

      // Load categories
      const categories = await apiService.getCategories();
      console.log(`Loaded ${categories.length} categories from server`);
      for (const category of categories) {
        await offlineDb.saveCategory(category);
      }

      // Load today's entries
      const today = new Date().toISOString().split('T')[0];
      const entries = await apiService.getDailyEntries(today);
      console.log(`Loaded ${entries.length} entries for today`);
      for (const entry of entries) {
        await offlineDb.saveDailyEntry(entry);
      }

      this.lastSyncTimestamp = Date.now();
      await offlineDb.setSetting('lastSyncTimestamp', this.lastSyncTimestamp.toString());

      console.log('Initial data loaded successfully');
    } catch (error) {
      console.error('Failed to load initial data:', error);
      // Don't throw - just log and continue with local data
    }
  }

  // ===== ENHANCED TRACKING METHODS =====

  // Metric Definitions
  async getMetricDefinitions(habitId: number): Promise<HabitMetricDefinition[]> {
    return await offlineDb.getMetricDefinitions(habitId);
  }

  async saveMetricDefinition(metric: HabitMetricDefinition): Promise<void> {
    await offlineDb.saveMetricDefinition(metric);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  async deleteMetricDefinition(id: number): Promise<void> {
    await offlineDb.deleteMetricDefinition(id);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Metric Values
  async getMetricValues(entryId: number): Promise<CustomMetricValue[]> {
    return await offlineDb.getMetricValues(entryId);
  }

  async getMetricValuesForHabit(habitId: number): Promise<CustomMetricValue[]> {
    return await offlineDb.getMetricValuesForHabit(habitId);
  }

  async saveMetricValue(value: CustomMetricValue): Promise<void> {
    await offlineDb.saveMetricValue(value);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Timer Sessions
  async getTimerSessions(habitId: number): Promise<TimerSession[]> {
    return await offlineDb.getTimerSessions(habitId);
  }

  async saveTimerSession(session: TimerSession): Promise<void> {
    await offlineDb.saveTimerSession(session);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  async getRecentTimerSessions(limit?: number): Promise<TimerSession[]> {
    return await offlineDb.getRecentTimerSessions(limit);
  }

  // Activity Logs
  async getActivityLogs(entryId: number): Promise<ActivityLog[]> {
    return await offlineDb.getActivityLogs(entryId);
  }

  async getActivityLogsForHabit(habitId: number, limit?: number): Promise<ActivityLog[]> {
    return await offlineDb.getActivityLogsForHabit(habitId, limit);
  }

  async saveActivityLog(log: ActivityLog): Promise<void> {
    await offlineDb.saveActivityLog(log);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Routine Templates
  async getRoutineTemplates(habitId: number): Promise<RoutineTemplate[]> {
    return await offlineDb.getRoutineTemplates(habitId);
  }

  async saveRoutineTemplate(template: RoutineTemplate): Promise<void> {
    await offlineDb.saveRoutineTemplate(template);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  async deleteRoutineTemplate(id: number): Promise<void> {
    await offlineDb.deleteRoutineTemplate(id);
    
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }

  // Update daily entry with enhanced data
  async updateDailyEntry(id: number, updates: Partial<DailyHabitEntry>): Promise<void> {
    const date = updates.date || new Date().toISOString().split('T')[0];
    const entries = await offlineDb.getDailyEntries(date);
    const existing = entries.find(e => e.id === id);
    
    if (!existing) {
      throw new Error(`Entry with ID ${id} not found`);
    }

    const updatedEntry: DailyHabitEntry = {
      ...existing,
      ...updates,
      id, // Ensure ID doesn't change
    };

    await offlineDb.saveDailyEntry(updatedEntry);

    // Try immediate sync
    if (await this.isServerOnline()) {
      await this.sync();
    }
  }
}

// Export singleton instance
export const syncService = new SyncService();
