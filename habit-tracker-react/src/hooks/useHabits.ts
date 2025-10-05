import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useMemo, useCallback } from 'react';
import { syncService } from '../services/syncService';
import type { Habit, HabitWithStatus, Category } from '../types/habit.types';
import { format } from 'date-fns';

// Custom hook for habits
export function useHabits() {
  const queryClient = useQueryClient();

  const { data: habits = [], isLoading, error } = useQuery({
    queryKey: ['habits'],
    queryFn: () => syncService.getHabits(),
  });

  const createHabit = useMutation({
    mutationFn: (habit: Habit) => syncService.saveHabit(habit),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  const updateHabit = useMutation({
    mutationFn: (habit: Habit) => syncService.saveHabit(habit),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    habits,
    isLoading,
    error,
    createHabit: createHabit.mutate,
    updateHabit: updateHabit.mutate,
  };
}

// Custom hook for today's habits with completion status
export function useHabitsForToday(targetDate?: string) {
  const queryClient = useQueryClient();
  const today = targetDate || format(new Date(), 'yyyy-MM-dd');

  const { data: habits = [] } = useQuery({
    queryKey: ['habits'],
    queryFn: () => syncService.getHabits(),
  });

  const { data: entries = [] } = useQuery({
    queryKey: ['daily-entries', today],
    queryFn: () => syncService.getDailyEntries(today),
  });

  const toggleCompletion = useMutation({
    mutationFn: ({ habitId }: { habitId: number }) =>
      syncService.toggleHabitCompletion(habitId, today),
    onMutate: async ({ habitId }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['daily-entries', today] });
      
      // Snapshot previous value
      const previousEntries = queryClient.getQueryData(['daily-entries', today]);
      
      // Optimistically update cache
      queryClient.setQueryData(['daily-entries', today], (old: any[] = []) => {
        const existing = old.find(e => e.habitId === habitId);
        if (existing) {
          return old.map(e => 
            e.habitId === habitId 
              ? { ...e, isCompleted: !e.isCompleted, completedAt: !e.isCompleted ? new Date().toISOString() : undefined }
              : e
          );
        } else {
          return [...old, { habitId, date: today, isCompleted: true, completedAt: new Date().toISOString() }];
        }
      });
      
      return { previousEntries };
    },
    onError: (_err, _variables, context) => {
      // Rollback on error
      if (context?.previousEntries) {
        queryClient.setQueryData(['daily-entries', today], context.previousEntries);
      }
    },
    // Don't invalidate at all - rely on optimistic updates and periodic background sync
  });

  // Combine habits with today's completion status
  const habitsWithStatus: HabitWithStatus[] = useMemo(() => {
    return habits.map((habit) => {
      const todayEntry = entries.find((e) => e.habitId === habit.id);
      const isScheduledForToday = checkIfScheduledForToday(habit);

      return {
        ...habit,
        isCompletedToday: todayEntry?.isCompleted || false,
        isScheduledForToday,
        todayEntry,
        scheduledTime: habit.timeOfDay || undefined,
      };
    }).filter(h => h.isScheduledForToday); // Only show habits scheduled for today
  }, [habits, entries]);

  const toggleCompleteCallback = useCallback(
    (habitId: number) => toggleCompletion.mutate({ habitId }),
    [toggleCompletion]
  );

  return {
    habits: habitsWithStatus,
    toggleComplete: toggleCompleteCallback,
    isLoading: toggleCompletion.isPending,
  };
}

// Helper function to check if habit is scheduled for today
function checkIfScheduledForToday(habit: Habit): boolean {
  if (!habit.isActive) return false;

  const today = new Date();
  const dayOfWeek = today.getDay(); // 0 = Sunday

  switch (habit.recurrenceType) {
    case 0: // Daily
      return true;

    case 1: // Weekly
      return habit.specificDaysOfWeek?.includes(dayOfWeek) || false;

    case 2: // Monthly
      const dayOfMonth = today.getDate();
      return habit.specificDaysOfMonth?.includes(dayOfMonth) || false;

    case 3: // SpecificDays
      return habit.specificDaysOfWeek?.includes(dayOfWeek) || false;

    default:
      return true;
  }
}

// Custom hook for daily entries
export function useDailyEntries(date: string) {
  const { data: entries = [], isLoading } = useQuery({
    queryKey: ['daily-entries', date],
    queryFn: () => syncService.getDailyEntries(date),
  });

  return {
    entries,
    isLoading,
  };
}

// Custom hook for week entries (multiple dates)
export function useWeekEntries(dates: string[]) {
  const { data: allEntries = [], isLoading } = useQuery({
    queryKey: ['week-entries', dates],
    queryFn: async () => {
      const entriesPromises = dates.map(date => syncService.getDailyEntries(date));
      const entriesArrays = await Promise.all(entriesPromises);
      return entriesArrays.flat();
    },
    enabled: dates.length > 0,
  });

  return {
    entries: allEntries,
    isLoading,
    getEntriesForDate: (date: string) => allEntries.filter(e => e.date === date),
    isHabitCompletedOnDate: (habitId: number, date: string) => {
      const entry = allEntries.find(e => e.habitId === habitId && e.date === date);
      return entry?.isCompleted || false;
    },
  };
}

// Custom hook for toggling habit completion
export function useToggleHabitCompletion() {
  const queryClient = useQueryClient();

  const toggleMutation = useMutation({
    mutationFn: async ({ habitId, date }: { habitId: number; date: string }) => {
      await syncService.toggleHabitCompletion(habitId, date);
    },
    onMutate: async ({ habitId, date }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: ['daily-entries', date] });
      await queryClient.cancelQueries({ queryKey: ['week-entries'] });
      
      // Snapshot previous values
      const previousEntries = queryClient.getQueryData(['daily-entries', date]);
      const previousWeek = queryClient.getQueryData(['week-entries']);
      
      // Optimistically update the cache
      queryClient.setQueryData(['daily-entries', date], (old: any[] = []) => {
        const existing = old.find(e => e.habitId === habitId);
        if (existing) {
          return old.map(e => 
            e.habitId === habitId 
              ? { ...e, isCompleted: !e.isCompleted, completedAt: !e.isCompleted ? new Date().toISOString() : undefined }
              : e
          );
        } else {
          return [...old, { habitId, date, isCompleted: true, completedAt: new Date().toISOString() }];
        }
      });
      
      return { previousEntries, previousWeek, date };
    },
    onError: (_err, _variables, context) => {
      // Rollback on error
      if (context?.previousEntries) {
        queryClient.setQueryData(['daily-entries', context.date], context.previousEntries);
      }
      if (context?.previousWeek) {
        queryClient.setQueryData(['week-entries'], context.previousWeek);
      }
    },
    // Don't invalidate at all - rely on optimistic updates and periodic background sync
  });

  return {
    toggleCompletion: toggleMutation.mutate,
    isLoading: toggleMutation.isPending,
  };
}

// Custom hook for sync status
export function useSync() {
  const queryClient = useQueryClient();

  const syncNow = useMutation({
    mutationFn: () => syncService.sync(),
    onSuccess: () => {
      // Invalidate all queries to refetch data
      queryClient.invalidateQueries();
    },
  });

  return {
    syncNow: syncNow.mutate,
    isSyncing: syncNow.isPending,
  };
}

// Hook for categories
export function useCategories() {
  const { data: categories = [], isLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: () => syncService.getCategories(),
  });

  return {
    categories,
    isLoading,
  };
}

// Hook for creating a habit
export function useCreateHabit() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (habit: Habit) => syncService.createHabit(habit),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    createHabit: mutation.mutate,
    createHabitAsync: mutation.mutateAsync,
    isCreating: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for updating a habit
export function useUpdateHabit() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ id, updates }: { id: number; updates: Partial<Habit> }) =>
      syncService.updateHabit(id, updates),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
      queryClient.invalidateQueries({ queryKey: ['dailyEntries'] });
    },
  });

  return {
    updateHabit: mutation.mutate,
    updateHabitAsync: mutation.mutateAsync,
    isUpdating: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for deleting a habit
export function useDeleteHabit() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (id: number) => syncService.deleteHabit(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    deleteHabit: mutation.mutate,
    deleteHabitAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for duplicating a habit
export function useDuplicateHabit() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (id: number) => syncService.duplicateHabit(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    duplicateHabit: mutation.mutate,
    duplicateHabitAsync: mutation.mutateAsync,
    isDuplicating: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for creating a category
export function useCreateCategory() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (category: Category) => syncService.createCategory(category),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  return {
    createCategory: mutation.mutate,
    createCategoryAsync: mutation.mutateAsync,
    isCreating: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for updating a category
export function useUpdateCategory() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ id, updates }: { id: number; updates: Partial<Category> }) =>
      syncService.updateCategory(id, updates),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    updateCategory: mutation.mutate,
    updateCategoryAsync: mutation.mutateAsync,
    isUpdating: mutation.isPending,
    error: mutation.error,
  };
}

// Hook for deleting a category
export function useDeleteCategory() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (id: number) => syncService.deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    },
  });

  return {
    deleteCategory: mutation.mutate,
    deleteCategoryAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    error: mutation.error,
  };
}
