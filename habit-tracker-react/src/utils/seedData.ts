import { offlineDb } from '../services/offlineDb';
import type { Habit, Category } from '../types/habit.types';
import { RecurrenceType } from '../types/habit.types';

export async function seedSampleData() {
  // Check if we already have data
  const existingHabits = await offlineDb.getHabits();
  if (existingHabits.length > 0) {
    console.log('Database already has data, skipping seed');
    return;
  }

  console.log('Seeding sample data...');

  // Create categories
  const categories: Partial<Category>[] = [
    {
      id: 1,
      name: 'Health & Fitness',
      description: 'Physical health and exercise',
      color: '#10B981',
      icon: '🏃',
    },
    {
      id: 2,
      name: 'Productivity',
      description: 'Work and personal productivity',
      color: '#6366F1',
      icon: '📝',
    },
    {
      id: 3,
      name: 'Mindfulness',
      description: 'Mental health and meditation',
      color: '#8B5CF6',
      icon: '🧘',
    },
  ];

  for (const category of categories) {
    await offlineDb.saveCategory(category as Category);
  }

  // Create sample habits
  const habits: Habit[] = [
    {
      id: 1,
      name: 'Morning Exercise',
      description: 'Start the day with 30 minutes of exercise',
      categoryId: 1,
      recurrenceType: RecurrenceType.Daily,
      timeOfDay: '07:00',
      duration: 30,
      isActive: true,
      color: '#10B981',
      reminderEnabled: true,
      tags: ['morning', 'fitness', 'energy'],
    } as Habit,
    {
      id: 2,
      name: 'Read for 20 Minutes',
      description: 'Read a book or educational material',
      categoryId: 2,
      recurrenceType: RecurrenceType.Daily,
      timeOfDay: '20:00',
      duration: 20,
      isActive: true,
      color: '#6366F1',
      reminderEnabled: true,
      tags: ['learning', 'evening', 'growth'],
    } as Habit,
    {
      id: 3,
      name: 'Meditation',
      description: 'Practice mindfulness meditation',
      categoryId: 3,
      recurrenceType: RecurrenceType.Daily,
      timeOfDay: '06:30',
      duration: 15,
      isActive: true,
      color: '#8B5CF6',
      reminderEnabled: false,
      tags: ['mindfulness', 'morning', 'calm'],
    } as Habit,
    {
      id: 4,
      name: 'Drink 8 Glasses of Water',
      description: 'Stay hydrated throughout the day',
      categoryId: 1,
      recurrenceType: RecurrenceType.Daily,
      isActive: true,
      color: '#06B6D4',
      reminderEnabled: true,
      tags: ['health', 'hydration'],
    } as Habit,
    {
      id: 5,
      name: 'Weekly Planning',
      description: 'Plan and organize the week ahead',
      categoryId: 2,
      recurrenceType: RecurrenceType.Weekly,
      timeOfDay: '09:00',
      duration: 60,
      isActive: true,
      color: '#F59E0B',
      reminderEnabled: true,
      tags: ['planning', 'productivity', 'sunday'],
    } as Habit,
  ];

  for (const habit of habits) {
    await offlineDb.saveHabit(habit);
  }

  console.log('Sample data seeded successfully!');
  console.log(`- ${categories.length} categories`);
  console.log(`- ${habits.length} habits`);
}
