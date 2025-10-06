import type { Habit } from '../types/habit.types';

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'] as const;
const DAY_NAME_TO_INDEX: Record<string, number> = DAY_NAMES.reduce((acc, day, index) => {
  acc[day.toLowerCase()] = index;
  return acc;
}, {} as Record<string, number>);

function normalizeTimeString(time?: string | null): string | undefined {
  if (!time) return undefined;
  const trimmed = time.trim();
  if (!trimmed) return undefined;
  if (trimmed.length === 8 && trimmed.includes(':')) {
    return `${trimmed.slice(0, 2)}:${trimmed.slice(3, 5)}`;
  }
  return trimmed;
}

function weeklyDaysStringToNumbers(weeklyDays?: string | null): number[] | undefined {
  if (!weeklyDays) return undefined;
  const normalized = weeklyDays
    .split(',')
    .map(day => day.trim())
    .filter(Boolean)
    .map(day => DAY_NAME_TO_INDEX[day.toLowerCase()])
    .filter((value): value is number => typeof value === 'number');
  if (normalized.length === 0) return undefined;
  return Array.from(new Set(normalized)).sort((a, b) => a - b);
}

function numbersToWeeklyDaysString(days?: number[]): string | undefined {
  if (!days || days.length === 0) return undefined;
  const entries = Array.from(new Set(days))
    .filter(day => day >= 0 && day < DAY_NAMES.length)
    .sort((a, b) => a - b)
    .map(day => DAY_NAMES[day]);
  return entries.length > 0 ? entries.join(',') : undefined;
}

function parseMonthlyDays(monthlyDays?: string | null): number[] | undefined {
  if (!monthlyDays) return undefined;
  const values = monthlyDays
    .split(',')
    .map(day => parseInt(day.trim(), 10))
    .filter(day => !Number.isNaN(day));
  if (values.length === 0) return undefined;
  return Array.from(new Set(values)).sort((a, b) => a - b);
}

function monthlyDaysToString(days?: number[]): string | undefined {
  if (!days || days.length === 0) return undefined;
  const values = Array.from(new Set(days))
    .filter(day => day >= 1 && day <= 31)
    .sort((a, b) => a - b);
  return values.length > 0 ? values.join(',') : undefined;
}

function normalizeTags(tags?: string | string[] | null): string[] | undefined {
  if (!tags) return undefined;
  if (Array.isArray(tags)) {
    const normalized = tags.map(tag => tag.trim()).filter(Boolean);
    return normalized.length > 0 ? normalized : undefined;
  }
  const normalized = tags
    .split(',')
    .map(tag => tag.trim())
    .filter(Boolean);
  return normalized.length > 0 ? normalized : undefined;
}

export function normalizeHabitFromServer(serverHabit: any): Habit {
  const isDeleted = Boolean(serverHabit?.isDeleted);
  const isActive = serverHabit?.isActive !== undefined ? Boolean(serverHabit.isActive) : !isDeleted;

  const specificDaysOfWeek = Array.isArray(serverHabit?.specificDaysOfWeek)
    ? serverHabit.specificDaysOfWeek
        .map((day: number) => Number(day))
        .filter((day: number) => !Number.isNaN(day))
    : weeklyDaysStringToNumbers(serverHabit?.weeklyDays);

  const specificDaysOfMonth = Array.isArray(serverHabit?.specificDaysOfMonth)
    ? serverHabit.specificDaysOfMonth
        .map((day: number) => Number(day))
        .filter((day: number) => !Number.isNaN(day))
    : parseMonthlyDays(serverHabit?.monthlyDays);

  const normalizedTags = normalizeTags(serverHabit?.tags);

  return {
    id: serverHabit.id,
    name: serverHabit.name,
    description: serverHabit.description ?? undefined,
    shortDescription: serverHabit.shortDescription ?? undefined,
    categoryId: serverHabit.categoryId ?? undefined,
    recurrenceType: serverHabit.recurrenceType ?? 0,
    recurrenceInterval: serverHabit.recurrenceInterval ?? undefined,
    specificDaysOfWeek,
    specificDaysOfMonth,
    timeOfDay: normalizeTimeString(serverHabit.timeOfDay),
    timeOfDayEnd: normalizeTimeString(serverHabit.timeOfDayEnd),
    duration: serverHabit.duration ?? undefined,
    habitType: serverHabit.habitType ?? 0,
    isActive,
    color: serverHabit.color ?? undefined,
    icon: serverHabit.icon ?? undefined,
    imageUrl: serverHabit.imageUrl ?? undefined,
    reminderEnabled: serverHabit.reminderEnabled ?? false,
    reminderTime: normalizeTimeString(serverHabit.reminderTime),
    tags: normalizedTags,
    createdDate: serverHabit.createdDate ?? new Date().toISOString(),
    lastModifiedDate: serverHabit.lastModifiedDate ?? undefined,
    deviceId: serverHabit.deviceId ?? undefined,
    syncStatus: serverHabit.syncStatus ?? 'synced',
    exercises: serverHabit.exercises ?? undefined,
  };
}

export function mapHabitToServer(habit: Habit): any {
  const weeklyDays = numbersToWeeklyDaysString(habit.specificDaysOfWeek);
  const monthlyDays = monthlyDaysToString(habit.specificDaysOfMonth);
  const tags = habit.tags && habit.tags.length > 0 ? habit.tags.join(',') : null;

  return {
    id: habit.id,
    name: habit.name,
    description: habit.description ?? null,
    shortDescription: habit.shortDescription ?? null,
    categoryId: habit.categoryId ?? null,
    habitType: habit.habitType ?? 0,
    recurrenceType: habit.recurrenceType,
    recurrenceInterval: habit.recurrenceInterval ?? null,
    weeklyDays: weeklyDays ?? null,
    monthlyDays: monthlyDays ?? null,
    specificDaysOfWeek: habit.specificDaysOfWeek ?? null,
    specificDaysOfMonth: habit.specificDaysOfMonth ?? null,
    timeOfDay: habit.timeOfDay ?? null,
    timeOfDayEnd: habit.timeOfDayEnd ?? null,
    duration: habit.duration ?? null,
    isDeleted: habit.isActive === false,
    isActive: habit.isActive ?? true,
    imageUrl: habit.imageUrl ?? null,
    color: habit.color ?? null,
    icon: habit.icon ?? null,
    reminderEnabled: habit.reminderEnabled ?? false,
    reminderTime: habit.reminderTime ?? null,
    tags,
    createdDate: habit.createdDate ?? new Date().toISOString(),
    lastModifiedDate: habit.lastModifiedDate ?? new Date().toISOString(),
    deviceId: habit.deviceId ?? null,
    syncStatus: habit.syncStatus ?? null,
  };
}
