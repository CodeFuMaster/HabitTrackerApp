// Core Habit Types matching your MVC server models

export interface Habit {
  id: number;
  name: string;
  description?: string;
  categoryId?: number;
  recurrenceType: RecurrenceType;
  recurrenceInterval?: number;
  specificDaysOfWeek?: number[]; // 0=Sunday, 1=Monday, etc.
  specificDaysOfMonth?: number[];
  timeOfDay?: string; // HH:mm format
  duration?: number; // minutes
  isActive: boolean;
  color?: string;
  icon?: string;
  imageUrl?: string;
  reminderEnabled: boolean;
  reminderTime?: string;
  tags?: string[];
  createdDate: string;
  lastModifiedDate?: string;
  deviceId?: string;
  syncStatus?: 'pending' | 'synced' | 'conflict';
}

export enum RecurrenceType {
  Daily = 0,
  Weekly = 1,
  Monthly = 2,
  SpecificDays = 3,
  Custom = 4
}

export interface DailyHabitEntry {
  id: number;
  habitId: number;
  date: string;
  isCompleted: boolean;
  completedAt?: string;
  notes?: string;
  rating?: number;
  mood?: number; // 1-5 scale
  energyLevel?: number; // 1-5 scale
  photoUrls?: string[];
  deviceId?: string;
  syncStatus?: 'pending' | 'synced' | 'conflict';
  customMetrics?: CustomMetricValue[];
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
}

export interface RoutineSession {
  id: number;
  habitId: number;
  date: string;
  isCompleted: boolean;
  startedAt?: string;
  completedAt?: string;
  notes?: string;
  rating?: number;
  activities: SessionActivity[];
}

export interface SessionActivity {
  id: number;
  routineSessionId: number;
  activityTemplateId?: number;
  name: string;
  type: ActivityType;
  isCompleted: boolean;
  duration?: number; // seconds
  startedAt?: string;
  completedAt?: string;
  order: number;
  notes?: string;
  metrics: ActivityMetric[];
}

export enum ActivityType {
  Strength = 0,
  Cardio = 1,
  Flexibility = 2,
  Breathing = 3,
  Meditation = 4,
  Other = 5
}

export interface ActivityMetric {
  id: number;
  sessionActivityId: number;
  name: string;
  numericValue?: number;
  textValue?: string;
  timeValue?: number; // seconds
  unit?: string;
  setNumber?: number;
}

// Sync types
export interface SyncChanges {
  habits: Habit[];
  entries: DailyHabitEntry[];
  categories: Category[];
  routineSessions: RoutineSession[];
  serverTimestamp: number;
}

export interface ClientChanges {
  deviceId: string;
  lastSyncTimestamp: number;
  habits: Habit[];
  entries: DailyHabitEntry[];
  categories: Category[];
  routineSessions: RoutineSession[];
}

export interface SyncResponse {
  success: boolean;
  conflicts?: any[];
  serverTimestamp: number;
}

// UI Helper types
export interface HabitWithStatus extends Habit {
  isCompletedToday: boolean;
  isScheduledForToday: boolean;
  todayEntry?: DailyHabitEntry;
  scheduledTime?: string;
}

// Custom Metrics Types
export interface HabitMetricDefinition {
  id: number;
  habitId: number;
  name: string;
  type: MetricType;
  unit?: string;
  defaultValue?: number | string | boolean;
  isRequired: boolean;
  order: number;
  options?: string[]; // For select/radio types
}

export enum MetricType {
  Number = 'number',
  Text = 'text',
  Boolean = 'boolean',
  Select = 'select',
  Rating = 'rating', // 1-5 stars
  Time = 'time', // Duration in seconds
  Distance = 'distance', // km or miles
  Weight = 'weight', // kg or lbs
  Reps = 'reps', // Count
  Sets = 'sets', // Count
}

export interface CustomMetricValue {
  id: number;
  entryId: number;
  metricDefinitionId: number;
  numericValue?: number;
  textValue?: string;
  booleanValue?: boolean;
  timestamp?: string;
}

// Timer/Stopwatch Types
export interface TimerSession {
  id: number;
  habitId: number;
  entryId?: number;
  startTime: string;
  endTime?: string;
  duration: number; // seconds
  isPaused: boolean;
  pausedAt?: string;
  totalPausedTime: number; // seconds
  type: 'timer' | 'stopwatch';
}

// Activity Logging Types
export interface ActivityLog {
  id: number;
  entryId: number;
  timestamp: string;
  type: ActivityLogType;
  description: string;
  metadata?: Record<string, any>;
}

export enum ActivityLogType {
  Started = 'started',
  Paused = 'paused',
  Resumed = 'resumed',
  Completed = 'completed',
  Note = 'note',
  Photo = 'photo',
  Metric = 'metric',
}

// Routine Template Types
export interface RoutineTemplate {
  id: number;
  habitId: number;
  name: string;
  description?: string;
  steps: RoutineStep[];
  estimatedDuration?: number; // minutes
  isActive: boolean;
}

export interface RoutineStep {
  id: number;
  templateId: number;
  name: string;
  description?: string;
  duration?: number; // minutes
  order: number;
  isOptional: boolean;
  metrics?: HabitMetricDefinition[];
}
