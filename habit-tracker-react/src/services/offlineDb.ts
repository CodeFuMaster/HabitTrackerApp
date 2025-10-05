import initSqlJs, { Database } from 'sql.js';
import localforage from 'localforage';
import type { 
  Habit, 
  DailyHabitEntry, 
  Category,
  HabitMetricDefinition,
  CustomMetricValue,
  TimerSession,
  ActivityLog,
  RoutineTemplate,
  RoutineStep
} from '../types/habit.types';

class OfflineDatabase {
  private db: Database | null = null;
  private initialized = false;

  async initialize(): Promise<void> {
    if (this.initialized) return;

    try {
      console.log('Initializing offline database...');
      
      // Initialize SQL.js
      const SQL = await initSqlJs({
        locateFile: (file: string) => `https://sql.js.org/dist/${file}`,
      });
      
      console.log('SQL.js initialized successfully');

      // Try to load existing database from storage
      const savedDb = await localforage.getItem<ArrayBuffer>('habit-tracker-db');
      
      if (savedDb) {
        this.db = new SQL.Database(new Uint8Array(savedDb));
        console.log('Loaded existing database from storage');
      } else {
        this.db = new SQL.Database();
        await this.createTables();
        console.log('Created new database');
      }

      this.initialized = true;
      console.log('Offline database initialized successfully');
    } catch (error) {
      console.error('Failed to initialize database:', error);
      throw error;
    }
  }

  private async createTables(): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      CREATE TABLE IF NOT EXISTS habits (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        description TEXT,
        categoryId INTEGER,
        recurrenceType INTEGER NOT NULL,
        recurrenceInterval INTEGER,
        specificDaysOfWeek TEXT,
        specificDaysOfMonth TEXT,
        timeOfDay TEXT,
        duration INTEGER,
        isActive BOOLEAN DEFAULT 1,
        color TEXT,
        icon TEXT,
        imageUrl TEXT,
        reminderEnabled BOOLEAN DEFAULT 0,
        reminderTime TEXT,
        tags TEXT,
        createdDate TEXT NOT NULL,
        lastModifiedDate TEXT,
        deviceId TEXT,
        syncStatus TEXT DEFAULT 'pending'
      );

      CREATE TABLE IF NOT EXISTS daily_entries (
        id INTEGER PRIMARY KEY,
        habitId INTEGER NOT NULL,
        date TEXT NOT NULL,
        isCompleted BOOLEAN DEFAULT 0,
        completedAt TEXT,
        notes TEXT,
        rating INTEGER,
        deviceId TEXT,
        syncStatus TEXT DEFAULT 'pending',
        FOREIGN KEY (habitId) REFERENCES habits(id),
        UNIQUE(habitId, date)
      );

      CREATE TABLE IF NOT EXISTS categories (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        description TEXT,
        color TEXT,
        icon TEXT
      );

      CREATE TABLE IF NOT EXISTS sync_log (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        tableName TEXT NOT NULL,
        recordId INTEGER NOT NULL,
        operation TEXT NOT NULL,
        timestamp INTEGER NOT NULL,
        data TEXT,
        deviceId TEXT,
        synced BOOLEAN DEFAULT 0
      );

      CREATE TABLE IF NOT EXISTS settings (
        key TEXT PRIMARY KEY,
        value TEXT
      );

      CREATE TABLE IF NOT EXISTS habit_metric_definitions (
        id INTEGER PRIMARY KEY,
        habitId INTEGER NOT NULL,
        name TEXT NOT NULL,
        type TEXT NOT NULL,
        unit TEXT,
        defaultValue TEXT,
        isRequired INTEGER DEFAULT 0,
        orderIndex INTEGER DEFAULT 0,
        options TEXT,
        FOREIGN KEY (habitId) REFERENCES habits(id)
      );

      CREATE TABLE IF NOT EXISTS custom_metric_values (
        id INTEGER PRIMARY KEY,
        entryId INTEGER NOT NULL,
        metricDefinitionId INTEGER NOT NULL,
        numericValue REAL,
        textValue TEXT,
        booleanValue INTEGER,
        timestamp TEXT NOT NULL,
        FOREIGN KEY (entryId) REFERENCES daily_entries(id),
        FOREIGN KEY (metricDefinitionId) REFERENCES habit_metric_definitions(id)
      );

      CREATE TABLE IF NOT EXISTS timer_sessions (
        id INTEGER PRIMARY KEY,
        habitId INTEGER NOT NULL,
        entryId INTEGER,
        startTime TEXT NOT NULL,
        endTime TEXT,
        duration INTEGER,
        isPaused INTEGER DEFAULT 0,
        pausedAt TEXT,
        totalPausedTime INTEGER DEFAULT 0,
        type TEXT DEFAULT 'timer',
        FOREIGN KEY (habitId) REFERENCES habits(id),
        FOREIGN KEY (entryId) REFERENCES daily_entries(id)
      );

      CREATE TABLE IF NOT EXISTS activity_logs (
        id INTEGER PRIMARY KEY,
        entryId INTEGER NOT NULL,
        timestamp TEXT NOT NULL,
        type TEXT NOT NULL,
        description TEXT,
        metadata TEXT,
        FOREIGN KEY (entryId) REFERENCES daily_entries(id)
      );

      CREATE TABLE IF NOT EXISTS routine_templates (
        id INTEGER PRIMARY KEY,
        habitId INTEGER NOT NULL,
        name TEXT NOT NULL,
        description TEXT,
        estimatedDuration INTEGER,
        isActive INTEGER DEFAULT 1,
        FOREIGN KEY (habitId) REFERENCES habits(id)
      );

      CREATE TABLE IF NOT EXISTS routine_steps (
        id INTEGER PRIMARY KEY,
        templateId INTEGER NOT NULL,
        name TEXT NOT NULL,
        description TEXT,
        duration INTEGER,
        orderIndex INTEGER NOT NULL,
        isOptional INTEGER DEFAULT 0,
        FOREIGN KEY (templateId) REFERENCES routine_templates(id)
      );

      CREATE INDEX IF NOT EXISTS idx_daily_entries_date ON daily_entries(date);
      CREATE INDEX IF NOT EXISTS idx_daily_entries_habit ON daily_entries(habitId);
      CREATE INDEX IF NOT EXISTS idx_sync_log_synced ON sync_log(synced);
      CREATE INDEX IF NOT EXISTS idx_metric_definitions_habit ON habit_metric_definitions(habitId);
      CREATE INDEX IF NOT EXISTS idx_metric_values_entry ON custom_metric_values(entryId);
      CREATE INDEX IF NOT EXISTS idx_timer_sessions_habit ON timer_sessions(habitId);
      CREATE INDEX IF NOT EXISTS idx_activity_logs_entry ON activity_logs(entryId);
      CREATE INDEX IF NOT EXISTS idx_routine_steps_template ON routine_steps(templateId);
    `);

    await this.saveToStorage();
  }

  private async saveToStorage(): Promise<void> {
    if (!this.db) return;
    const data = this.db.export();
    await localforage.setItem('habit-tracker-db', data.buffer);
  }

  // Habits CRUD
  async getHabits(): Promise<Habit[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT * FROM habits WHERE isActive = 1');
    if (results.length === 0) return [];
    
    return this.formatResults<Habit>(results[0]);
  }

  async getHabit(id: number): Promise<Habit | null> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT * FROM habits WHERE id = ?', [id]);
    if (results.length === 0 || results[0].values.length === 0) return null;
    
    const habits = this.formatResults<Habit>(results[0]);
    return habits[0] || null;
  }

  async saveHabit(habit: Habit): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    const exists = await this.getHabit(habit.id);
    
    if (exists) {
      // Update
      this.db.run(`
        UPDATE habits SET
          name = ?, description = ?, categoryId = ?, recurrenceType = ?,
          timeOfDay = ?, duration = ?, isActive = ?, color = ?,
          imageUrl = ?, reminderEnabled = ?, tags = ?,
          lastModifiedDate = ?, syncStatus = 'pending'
        WHERE id = ?
      `, [
        habit.name, habit.description ?? null, habit.categoryId ?? null, habit.recurrenceType,
        habit.timeOfDay ?? null, habit.duration ?? null, habit.isActive ? 1 : 0, habit.color ?? null,
        habit.imageUrl ?? null, habit.reminderEnabled ? 1 : 0, JSON.stringify(habit.tags ?? []),
        new Date().toISOString(), habit.id
      ]);
    } else {
      // Insert
      this.db.run(`
        INSERT INTO habits (
          id, name, description, categoryId, recurrenceType,
          timeOfDay, duration, isActive, color, imageUrl,
          reminderEnabled, tags, createdDate, syncStatus
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 'synced')
      `, [
        habit.id, habit.name, habit.description ?? null, habit.categoryId ?? null, habit.recurrenceType,
        habit.timeOfDay ?? null, habit.duration ?? null, habit.isActive ? 1 : 0, habit.color ?? null,
        habit.imageUrl ?? null, habit.reminderEnabled ? 1 : 0, JSON.stringify(habit.tags ?? []),
        habit.createdDate ?? new Date().toISOString()
      ]);
    }

    await this.saveToStorage();
  }

  // Daily Entries
  async getDailyEntries(date: string): Promise<DailyHabitEntry[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT * FROM daily_entries WHERE date = ?', [date]);
    if (results.length === 0) return [];
    
    return this.formatResults<DailyHabitEntry>(results[0]);
  }

  async saveDailyEntry(entry: DailyHabitEntry): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO daily_entries 
      (id, habitId, date, isCompleted, completedAt, notes, rating, syncStatus)
      VALUES (?, ?, ?, ?, ?, ?, ?, 'pending')
    `, [
      entry.id, entry.habitId, entry.date, entry.isCompleted ? 1 : 0,
      entry.completedAt ?? null, entry.notes ?? null, entry.rating ?? null
    ]);

    // Save custom metric values if present
    if (entry.customMetrics && entry.customMetrics.length > 0) {
      for (const metric of entry.customMetrics) {
        await this.saveMetricValue(metric);
      }
    }

    await this.logChange('daily_entries', entry.id, entry.id ? 'UPDATE' : 'INSERT');
    await this.saveToStorage();
  }

  // Categories
  async getCategories(): Promise<Category[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT * FROM categories');
    if (results.length === 0) return [];
    
    return this.formatResults<Category>(results[0]);
  }

  async saveCategory(category: Category): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO categories (id, name, description, color, icon)
      VALUES (?, ?, ?, ?, ?)
    `, [category.id, category.name, category.description ?? null, category.color ?? null, category.icon ?? null]);

    await this.saveToStorage();
  }

  // Sync helpers
  async logChange(tableName: string, recordId: number, operation: string): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT INTO sync_log (tableName, recordId, operation, timestamp, deviceId, synced)
      VALUES (?, ?, ?, ?, ?, 0)
    `, [tableName, recordId, operation, Date.now(), await this.getDeviceId()]);

    await this.saveToStorage();
  }

  async getPendingChanges(): Promise<any[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT * FROM sync_log WHERE synced = 0');
    if (results.length === 0) return [];
    
    return this.formatResults(results[0]);
  }

  async markChangesSynced(changeIds: number[]): Promise<void> {
    if (!this.db || changeIds.length === 0) return;

    const placeholders = changeIds.map(() => '?').join(',');
    this.db.run(`UPDATE sync_log SET synced = 1 WHERE id IN (${placeholders})`, changeIds);
    
    await this.saveToStorage();
  }

  // Settings
  async getSetting(key: string): Promise<string | null> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec('SELECT value FROM settings WHERE key = ?', [key]);
    if (results.length === 0 || results[0].values.length === 0) return null;
    
    return results[0].values[0][0] as string;
  }

  async setSetting(key: string, value: string): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run('INSERT OR REPLACE INTO settings (key, value) VALUES (?, ?)', [key, value]);
    await this.saveToStorage();
  }

  async getDeviceId(): Promise<string> {
    let deviceId = await this.getSetting('deviceId');
    if (!deviceId) {
      deviceId = `device_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
      await this.setSetting('deviceId', deviceId);
    }
    return deviceId;
  }

  // Helper to format SQL results
  private formatResults<T>(result: { columns: string[]; values: any[][] }): T[] {
    return result.values.map((row) => {
      const obj: any = {};
      result.columns.forEach((col, index) => {
        let value = row[index];
        
        // Parse JSON strings
        if (col === 'tags' || col === 'specificDaysOfWeek' || col === 'specificDaysOfMonth' || 
            col === 'options' || col === 'metadata') {
          try {
            value = value ? JSON.parse(value) : null;
          } catch {
            value = null;
          }
        }
        
        // Convert boolean values
        if (col === 'isActive' || col === 'isCompleted' || col === 'reminderEnabled' || 
            col === 'synced' || col === 'isPaused' || col === 'isRequired' || 
            col === 'isOptional' || col === 'booleanValue') {
          value = value === 1;
        }
        
        // Rename orderIndex to order for consistency
        if (col === 'orderIndex') {
          obj['order'] = value;
        } else {
          obj[col] = value;
        }
      });
      return obj as T;
    });
  }

  async clearAll(): Promise<void> {
    if (!this.db) return;
    this.db.run('DROP TABLE IF EXISTS habits');
    this.db.run('DROP TABLE IF EXISTS daily_entries');
    this.db.run('DROP TABLE IF EXISTS categories');
    this.db.run('DROP TABLE IF EXISTS sync_log');
    this.db.run('DROP TABLE IF EXISTS settings');
    this.db.run('DROP TABLE IF EXISTS habit_metric_definitions');
    this.db.run('DROP TABLE IF EXISTS custom_metric_values');
    this.db.run('DROP TABLE IF EXISTS timer_sessions');
    this.db.run('DROP TABLE IF EXISTS activity_logs');
    this.db.run('DROP TABLE IF EXISTS routine_templates');
    this.db.run('DROP TABLE IF EXISTS routine_steps');
    await this.createTables();
  }

  // ===== ENHANCED TRACKING METHODS =====

  // Habit Metric Definitions
  async getMetricDefinitions(habitId: number): Promise<HabitMetricDefinition[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM habit_metric_definitions WHERE habitId = ? ORDER BY orderIndex',
      [habitId]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<HabitMetricDefinition>(results[0]);
  }

  async saveMetricDefinition(metric: HabitMetricDefinition): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO habit_metric_definitions 
      (id, habitId, name, type, unit, defaultValue, isRequired, orderIndex, options)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `, [
      metric.id,
      metric.habitId,
      metric.name,
      metric.type,
      metric.unit ?? null,
      metric.defaultValue !== undefined ? String(metric.defaultValue) : null,
      metric.isRequired ? 1 : 0,
      metric.order,
      metric.options ? JSON.stringify(metric.options) : null
    ]);

    await this.saveToStorage();
  }

  async deleteMetricDefinition(id: number): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');
    
    // Delete associated metric values first
    this.db.run('DELETE FROM custom_metric_values WHERE metricDefinitionId = ?', [id]);
    
    // Delete the definition
    this.db.run('DELETE FROM habit_metric_definitions WHERE id = ?', [id]);
    
    await this.saveToStorage();
  }

  // Custom Metric Values
  async getMetricValues(entryId: number): Promise<CustomMetricValue[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM custom_metric_values WHERE entryId = ?',
      [entryId]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<CustomMetricValue>(results[0]);
  }

  async saveMetricValue(value: CustomMetricValue): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO custom_metric_values 
      (id, entryId, metricDefinitionId, numericValue, textValue, booleanValue, timestamp)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `, [
      value.id,
      value.entryId,
      value.metricDefinitionId,
      value.numericValue ?? null,
      value.textValue ?? null,
      value.booleanValue !== undefined ? (value.booleanValue ? 1 : 0) : null,
      value.timestamp ?? new Date().toISOString()
    ]);

    await this.saveToStorage();
  }

  async getMetricValuesForHabit(habitId: number): Promise<CustomMetricValue[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(`
      SELECT cmv.* FROM custom_metric_values cmv
      JOIN daily_entries de ON cmv.entryId = de.id
      WHERE de.habitId = ?
      ORDER BY cmv.timestamp DESC
    `, [habitId]);
    
    if (results.length === 0) return [];
    
    return this.formatResults<CustomMetricValue>(results[0]);
  }

  // Timer Sessions
  async getTimerSessions(habitId: number): Promise<TimerSession[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM timer_sessions WHERE habitId = ? ORDER BY startTime DESC',
      [habitId]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<TimerSession>(results[0]);
  }

  async saveTimerSession(session: TimerSession): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO timer_sessions 
      (id, habitId, entryId, startTime, endTime, duration, isPaused, pausedAt, totalPausedTime, type)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    `, [
      session.id,
      session.habitId,
      session.entryId ?? null,
      session.startTime,
      session.endTime ?? null,
      session.duration,
      session.isPaused ? 1 : 0,
      session.pausedAt ?? null,
      session.totalPausedTime,
      session.type
    ]);

    await this.saveToStorage();
  }

  async getRecentTimerSessions(limit: number = 10): Promise<TimerSession[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM timer_sessions ORDER BY startTime DESC LIMIT ?',
      [limit]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<TimerSession>(results[0]);
  }

  // Activity Logs
  async getActivityLogs(entryId: number): Promise<ActivityLog[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM activity_logs WHERE entryId = ? ORDER BY timestamp',
      [entryId]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<ActivityLog>(results[0]);
  }

  async saveActivityLog(log: ActivityLog): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO activity_logs 
      (id, entryId, timestamp, type, description, metadata)
      VALUES (?, ?, ?, ?, ?, ?)
    `, [
      log.id,
      log.entryId,
      log.timestamp,
      log.type,
      log.description ?? null,
      log.metadata ? JSON.stringify(log.metadata) : null
    ]);

    await this.saveToStorage();
  }

  async getActivityLogsForHabit(habitId: number, limit?: number): Promise<ActivityLog[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    let query = `
      SELECT al.* FROM activity_logs al
      JOIN daily_entries de ON al.entryId = de.id
      WHERE de.habitId = ?
      ORDER BY al.timestamp DESC
    `;
    
    const params: any[] = [habitId];
    
    if (limit) {
      query += ' LIMIT ?';
      params.push(limit);
    }
    
    const results = this.db.exec(query, params);
    if (results.length === 0) return [];
    
    return this.formatResults<ActivityLog>(results[0]);
  }

  // Routine Templates
  async getRoutineTemplates(habitId: number): Promise<RoutineTemplate[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM routine_templates WHERE habitId = ? AND isActive = 1',
      [habitId]
    );
    if (results.length === 0) return [];
    
    const templates = this.formatResults<RoutineTemplate>(results[0]);
    
    // Load steps for each template
    for (const template of templates) {
      template.steps = await this.getRoutineSteps(template.id);
    }
    
    return templates;
  }

  async saveRoutineTemplate(template: RoutineTemplate): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO routine_templates 
      (id, habitId, name, description, estimatedDuration, isActive)
      VALUES (?, ?, ?, ?, ?, ?)
    `, [
      template.id,
      template.habitId,
      template.name,
      template.description ?? null,
      template.estimatedDuration ?? null,
      template.isActive ? 1 : 0
    ]);

    // Save steps
    if (template.steps && template.steps.length > 0) {
      // Delete existing steps
      this.db.run('DELETE FROM routine_steps WHERE templateId = ?', [template.id]);
      
      // Insert new steps
      for (const step of template.steps) {
        await this.saveRoutineStep(step);
      }
    }

    await this.saveToStorage();
  }

  async deleteRoutineTemplate(id: number): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');
    
    // Delete steps first
    this.db.run('DELETE FROM routine_steps WHERE templateId = ?', [id]);
    
    // Delete template
    this.db.run('DELETE FROM routine_templates WHERE id = ?', [id]);
    
    await this.saveToStorage();
  }

  // Routine Steps
  async getRoutineSteps(templateId: number): Promise<RoutineStep[]> {
    if (!this.db) throw new Error('Database not initialized');
    
    const results = this.db.exec(
      'SELECT * FROM routine_steps WHERE templateId = ? ORDER BY orderIndex',
      [templateId]
    );
    if (results.length === 0) return [];
    
    return this.formatResults<RoutineStep>(results[0]);
  }

  async saveRoutineStep(step: RoutineStep): Promise<void> {
    if (!this.db) throw new Error('Database not initialized');

    this.db.run(`
      INSERT OR REPLACE INTO routine_steps 
      (id, templateId, name, description, duration, orderIndex, isOptional)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `, [
      step.id,
      step.templateId,
      step.name,
      step.description ?? null,
      step.duration ?? null,
      step.order,
      step.isOptional ? 1 : 0
    ]);

    await this.saveToStorage();
  }
}

// Export singleton instance
export const offlineDb = new OfflineDatabase();
