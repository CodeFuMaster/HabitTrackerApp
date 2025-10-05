import axios from 'axios';
import type { Habit, DailyHabitEntry, Category, SyncChanges, ClientChanges, SyncResponse } from '../types/habit.types';

const API_BASE_URL = 'http://localhost:5178/api';

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// API Service
export const apiService = {
  // Health check
  async ping(): Promise<boolean> {
    try {
      const response = await apiClient.get('/EnhancedHabit/ping');
      return response.status === 200;
    } catch {
      return false;
    }
  },

  // Habits
  async getHabits(): Promise<Habit[]> {
    const response = await apiClient.get<Habit[]>('/EnhancedHabit');
    return response.data;
  },

  async getHabit(id: number): Promise<Habit> {
    const response = await apiClient.get<Habit>(`/EnhancedHabit/${id}`);
    return response.data;
  },

  async createHabit(habit: Partial<Habit>): Promise<Habit> {
    const response = await apiClient.post<Habit>('/EnhancedHabit', habit);
    return response.data;
  },

  async updateHabit(id: number, habit: Partial<Habit>): Promise<Habit> {
    const response = await apiClient.put<Habit>(`/EnhancedHabit/${id}`, habit);
    return response.data;
  },

  async deleteHabit(id: number): Promise<void> {
    await apiClient.delete(`/EnhancedHabit/${id}`);
  },

  // Daily Entries
  async getDailyEntries(date: string): Promise<DailyHabitEntry[]> {
    const response = await apiClient.get<DailyHabitEntry[]>(`/EnhancedHabit/entries`, {
      params: { date },
    });
    return response.data;
  },

  async createDailyEntry(entry: Partial<DailyHabitEntry>): Promise<DailyHabitEntry> {
    const response = await apiClient.post<DailyHabitEntry>('/EnhancedHabit/entries', entry);
    return response.data;
  },

  async updateDailyEntry(id: number, entry: Partial<DailyHabitEntry>): Promise<DailyHabitEntry> {
    const response = await apiClient.put<DailyHabitEntry>(`/EnhancedHabit/entries/${id}`, entry);
    return response.data;
  },

  async toggleCompletion(habitId: number, date: string): Promise<DailyHabitEntry> {
    const response = await apiClient.post<DailyHabitEntry>('/EnhancedHabit/toggle', {
      habitId,
      date,
    });
    return response.data;
  },

  // Categories
  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get<Category[]>('/CategoryApi');
    return response.data;
  },

  async createCategory(category: Partial<Category>): Promise<Category> {
    const response = await apiClient.post<Category>('/CategoryApi', category);
    return response.data;
  },

  // Sync endpoints (to be implemented on server)
  async getChangesSince(timestamp: number, deviceId: string): Promise<SyncChanges> {
    const response = await apiClient.get<SyncChanges>('/EnhancedHabit/changes-since', {
      params: { timestamp, deviceId },
    });
    return response.data;
  },

  async syncChanges(changes: ClientChanges): Promise<SyncResponse> {
    const response = await apiClient.post<SyncResponse>('/EnhancedHabit/sync', changes);
    return response.data;
  },
};

export default apiService;
