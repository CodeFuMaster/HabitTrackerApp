import { useState, useMemo } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Grid,
  ToggleButtonGroup,
  ToggleButton,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
  Chip,
} from '@mui/material';
import {
  TrendingUp,
  CalendarToday,
  LocalFireDepartment,
  EmojiEvents,
} from '@mui/icons-material';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { useHabits, useCategories, useWeekEntries } from '../hooks/useHabits';
import { format, subDays, parseISO } from 'date-fns';
import type { DailyHabitEntry } from '../types/habit.types';
import CompletionHeatmap from '../components/CompletionHeatmap';
import StreakVisualizer from '../components/StreakVisualizer';

type DateRange = '7days' | '30days' | '90days' | 'all';

export default function StatsView() {
  const { habits, isLoading: habitsLoading } = useHabits();
  const { categories } = useCategories();
  const [dateRange, setDateRange] = useState<DateRange>('30days');
  const [selectedHabitId, setSelectedHabitId] = useState<number | 'all'>('all');

  // Calculate date range
  const today = new Date();
  const startDate = useMemo(() => {
    switch (dateRange) {
      case '7days':
        return subDays(today, 7);
      case '30days':
        return subDays(today, 30);
      case '90days':
        return subDays(today, 90);
      case 'all':
        return subDays(today, 365); // Last year as "all"
      default:
        return subDays(today, 30);
    }
  }, [dateRange]);

  // Get all dates in range - create array of date strings
  const datesInRange = useMemo(() => {
    const dates: string[] = [];
    let currentDate = new Date(startDate);
    while (currentDate <= today) {
      dates.push(format(currentDate, 'yyyy-MM-dd'));
      currentDate = new Date(currentDate);
      currentDate.setDate(currentDate.getDate() + 1);
    }
    return dates;
  }, [startDate, today]);

  // Fetch entries for all dates
  const { entries: allEntries, isLoading: entriesLoading } = useWeekEntries(datesInRange);

  // Filter active habits
  const activeHabits = useMemo(() => habits.filter(h => h.isActive), [habits]);

  // Calculate summary statistics
  const stats = useMemo(() => {
    const filteredHabits = selectedHabitId === 'all' 
      ? activeHabits 
      : activeHabits.filter(h => h.id === selectedHabitId);

    const habitIds = filteredHabits.map(h => h.id);
    const relevantEntries = allEntries.filter((e: DailyHabitEntry) => habitIds.includes(e.habitId));
    const completedEntries = relevantEntries.filter((e: DailyHabitEntry) => e.isCompleted);

    // Total possible completions (habits × days)
    const totalPossible = habitIds.length * datesInRange.length;
    const totalCompleted = completedEntries.length;
    const completionRate = totalPossible > 0 ? (totalCompleted / totalPossible) * 100 : 0;

    // Calculate streaks
    const streaks = calculateStreaks(relevantEntries, habitIds, datesInRange);

    return {
      totalHabits: filteredHabits.length,
      totalCompleted,
      completionRate,
      currentStreak: streaks.current,
      bestStreak: streaks.best,
    };
  }, [activeHabits, allEntries, datesInRange, selectedHabitId]);

  // Completion rate over time (daily)
  const dailyCompletionData = useMemo(() => {
    const habitIds = selectedHabitId === 'all'
      ? activeHabits.map(h => h.id)
      : [selectedHabitId];

    return datesInRange.map(date => {
      const dayEntries = allEntries.filter((e: DailyHabitEntry) => 
        e.date === date && habitIds.includes(e.habitId)
      );
      const completed = dayEntries.filter((e: DailyHabitEntry) => e.isCompleted).length;
      const total = habitIds.length;
      const rate = total > 0 ? (completed / total) * 100 : 0;

      return {
        date: format(parseISO(date), 'MMM dd'),
        rate: Math.round(rate),
        completed,
        total,
      };
    });
  }, [allEntries, datesInRange, activeHabits, selectedHabitId]);

  // Completions per habit
  const habitCompletionData = useMemo(() => {
    return activeHabits.map(habit => {
      const habitEntries = allEntries.filter((e: DailyHabitEntry) => e.habitId === habit.id);
      const completed = habitEntries.filter((e: DailyHabitEntry) => e.isCompleted).length;
      return {
        name: habit.name.length > 20 ? habit.name.substring(0, 20) + '...' : habit.name,
        completions: completed,
        color: habit.color || '#6366F1',
      };
    }).sort((a, b) => b.completions - a.completions);
  }, [activeHabits, allEntries]);

  // Category breakdown
  const categoryData = useMemo(() => {
    const categoryMap = new Map<number, { name: string; count: number; color: string }>();

    activeHabits.forEach(habit => {
      const categoryId = habit.categoryId || 0;
      const category = categories.find(c => c.id === categoryId);
      const categoryName = category?.name || 'Uncategorized';
      const categoryColor = category?.color || '#9CA3AF';

      const habitEntries = allEntries.filter((e: DailyHabitEntry) => e.habitId === habit.id && e.isCompleted);
      const count = habitEntries.length;

      if (categoryMap.has(categoryId)) {
        const existing = categoryMap.get(categoryId)!;
        existing.count += count;
      } else {
        categoryMap.set(categoryId, {
          name: categoryName,
          count,
          color: categoryColor,
        });
      }
    });

    return Array.from(categoryMap.values()).filter(c => c.count > 0);
  }, [activeHabits, allEntries, categories]);

  // Best day of week
  const bestDayData = useMemo(() => {
    const dayMap = new Map<string, number>();
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    datesInRange.forEach(dateStr => {
      const date = parseISO(dateStr);
      const dayName = days[date.getDay()];
      const dayEntries = allEntries.filter((e: DailyHabitEntry) => e.date === dateStr && e.isCompleted);
      
      dayMap.set(dayName, (dayMap.get(dayName) || 0) + dayEntries.length);
    });

    return days.map(day => ({
      day,
      completions: dayMap.get(day) || 0,
    }));
  }, [datesInRange, allEntries]);

  const bestDay = useMemo(() => {
    if (bestDayData.length === 0) return 'N/A';
    const max = Math.max(...bestDayData.map(d => d.completions));
    const best = bestDayData.find(d => d.completions === max);
    return best?.day || 'N/A';
  }, [bestDayData]);

  // Heatmap data
  const heatmapData = useMemo(() => {
    const habitIds = selectedHabitId === 'all'
      ? activeHabits.map(h => h.id)
      : [selectedHabitId];

    return datesInRange.map(date => {
      const dayEntries = allEntries.filter((e: DailyHabitEntry) => 
        e.date === date && habitIds.includes(e.habitId)
      );
      const completed = dayEntries.filter((e: DailyHabitEntry) => e.isCompleted).length;
      const total = habitIds.length;
      const rate = total > 0 ? Math.round((completed / total) * 100) : 0;

      return {
        date,
        count: completed,
        rate,
      };
    });
  }, [allEntries, datesInRange, activeHabits, selectedHabitId]);

  const maxHeatmapCount = useMemo(() => {
    return Math.max(...heatmapData.map(d => d.count), 1);
  }, [heatmapData]);

  // Streak history calculation
  const streakHistory = useMemo(() => {
    const habitIds = selectedHabitId === 'all'
      ? activeHabits.map(h => h.id)
      : [selectedHabitId];

    const sortedDates = [...datesInRange].sort();
    const streaks: Array<{ startDate: string; endDate: string; length: number }> = [];
    let currentStreakStart: string | null = null;
    let currentStreakLength = 0;

    sortedDates.forEach((date, index) => {
      const dayEntries = allEntries.filter((e: DailyHabitEntry) => 
        e.date === date && habitIds.includes(e.habitId)
      );
      const completed = dayEntries.filter((e: DailyHabitEntry) => e.isCompleted).length;
      const total = habitIds.length;
      const dayComplete = total > 0 && (completed / total) >= 0.5;

      if (dayComplete) {
        if (currentStreakStart === null) {
          currentStreakStart = date;
        }
        currentStreakLength++;
      } else {
        if (currentStreakStart !== null && currentStreakLength >= 3) {
          streaks.push({
            startDate: format(parseISO(currentStreakStart), 'MMM dd'),
            endDate: format(parseISO(sortedDates[index - 1]), 'MMM dd'),
            length: currentStreakLength,
          });
        }
        currentStreakStart = null;
        currentStreakLength = 0;
      }
    });

    // Add final streak if exists
    if (currentStreakStart !== null && currentStreakLength >= 3) {
      streaks.push({
        startDate: format(parseISO(currentStreakStart), 'MMM dd'),
        endDate: format(parseISO(sortedDates[sortedDates.length - 1]), 'MMM dd'),
        length: currentStreakLength,
      });
    }

    return streaks.sort((a, b) => b.length - a.length);
  }, [allEntries, datesInRange, activeHabits, selectedHabitId]);

  if (habitsLoading || entriesLoading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Loading statistics...</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TrendingUp sx={{ fontSize: 32 }} />
          <Typography variant="h4" component="h1">
            Statistics
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <ToggleButtonGroup
            value={dateRange}
            exclusive
            onChange={(_, value) => value && setDateRange(value)}
            size="small"
          >
            <ToggleButton value="7days">7 Days</ToggleButton>
            <ToggleButton value="30days">30 Days</ToggleButton>
            <ToggleButton value="90days">90 Days</ToggleButton>
            <ToggleButton value="all">All Time</ToggleButton>
          </ToggleButtonGroup>

          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Filter by Habit</InputLabel>
            <Select
              value={selectedHabitId}
              label="Filter by Habit"
              onChange={(e) => setSelectedHabitId(e.target.value as number | 'all')}
            >
              <MenuItem value="all">All Habits</MenuItem>
              {activeHabits.map(habit => (
                <MenuItem key={habit.id} value={habit.id}>
                  {habit.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </Box>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <EmojiEvents color="primary" />
                <Typography variant="body2" color="text.secondary">
                  Total Habits
                </Typography>
              </Box>
              <Typography variant="h3" component="div">
                {stats.totalHabits}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Active habits tracked
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <CalendarToday color="success" />
                <Typography variant="body2" color="text.secondary">
                  Completion Rate
                </Typography>
              </Box>
              <Typography variant="h3" component="div">
                {Math.round(stats.completionRate)}%
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {stats.totalCompleted} of {stats.totalHabits * datesInRange.length} possible
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <LocalFireDepartment sx={{ color: '#F59E0B' }} />
                <Typography variant="body2" color="text.secondary">
                  Current Streak
                </Typography>
              </Box>
              <Typography variant="h3" component="div">
                {stats.currentStreak}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {stats.currentStreak === 1 ? 'day' : 'days'} in a row
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <TrendingUp color="error" />
                <Typography variant="body2" color="text.secondary">
                  Best Streak
                </Typography>
              </Box>
              <Typography variant="h3" component="div">
                {stats.bestStreak}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Personal record
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3}>
        {/* Completion Rate Over Time */}
        <Grid size={{ xs: 12, lg: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Completion Rate Over Time
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={dailyCompletionData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" style={{ fontSize: '12px' }} />
                  <YAxis unit="%" style={{ fontSize: '12px' }} />
                  <Tooltip 
                    content={({ active, payload }) => {
                      if (active && payload && payload.length) {
                        return (
                          <Box sx={{ bgcolor: 'background.paper', p: 1, border: 1, borderColor: 'divider', borderRadius: 1 }}>
                            <Typography variant="body2">{payload[0].payload.date}</Typography>
                            <Typography variant="body2" color="primary">
                              Rate: {payload[0].value}%
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {payload[0].payload.completed} of {payload[0].payload.total} completed
                            </Typography>
                          </Box>
                        );
                      }
                      return null;
                    }}
                  />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="rate" 
                    stroke="#6366F1" 
                    strokeWidth={2}
                    name="Completion Rate (%)"
                    dot={{ fill: '#6366F1' }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Category Breakdown */}
        <Grid size={{ xs: 12, lg: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Completions by Category
              </Typography>
              {categoryData.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={categoryData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={(entry: any) => `${entry.name} (${(entry.percent * 100).toFixed(0)}%)`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="count"
                    >
                      {categoryData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: 300 }}>
                  <Typography color="text.secondary">No data available</Typography>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Completions per Habit */}
        <Grid size={{ xs: 12, lg: 8 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Completions per Habit
              </Typography>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={habitCompletionData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" style={{ fontSize: '12px' }} />
                  <YAxis style={{ fontSize: '12px' }} />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="completions" name="Completions">
                    {habitCompletionData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Best Day of Week */}
        <Grid size={{ xs: 12, lg: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Best Day of Week
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                <Box sx={{ textAlign: 'center', py: 3 }}>
                  <Typography variant="h2" component="div" color="primary">
                    {bestDay}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Most productive day
                  </Typography>
                </Box>
                {bestDayData.map((day) => (
                  <Box key={day.day} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="body2">{day.day}</Typography>
                    <Chip 
                      label={day.completions} 
                      size="small" 
                      color={day.day === bestDay ? 'primary' : 'default'}
                    />
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Completion Heatmap (GitHub-style) */}
        <Grid size={{ xs: 12 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Completion Heatmap
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                GitHub-style visualization of your habit completion patterns
              </Typography>
              <CompletionHeatmap data={heatmapData} maxCount={maxHeatmapCount} />
            </CardContent>
          </Card>
        </Grid>

        {/* Streak Visualizer */}
        <Grid size={{ xs: 12, lg: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Streak Analysis
              </Typography>
              <StreakVisualizer
                currentStreak={stats.currentStreak}
                bestStreak={stats.bestStreak}
                streakHistory={streakHistory}
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Motivation & Insights */}
        <Grid size={{ xs: 12, lg: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Insights & Motivation
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                {stats.completionRate >= 80 && (
                  <Box sx={{ p: 2, bgcolor: 'success.light', borderRadius: 1 }}>
                    <Typography variant="body2" fontWeight={600} color="success.dark">
                      🎉 Outstanding Performance!
                    </Typography>
                    <Typography variant="caption" color="success.dark">
                      You're crushing it with an {Math.round(stats.completionRate)}% completion rate!
                    </Typography>
                  </Box>
                )}
                {stats.completionRate >= 50 && stats.completionRate < 80 && (
                  <Box sx={{ p: 2, bgcolor: 'info.light', borderRadius: 1 }}>
                    <Typography variant="body2" fontWeight={600} color="info.dark">
                      💪 Solid Progress!
                    </Typography>
                    <Typography variant="caption" color="info.dark">
                      You're maintaining a {Math.round(stats.completionRate)}% completion rate. Keep building!
                    </Typography>
                  </Box>
                )}
                {stats.completionRate < 50 && stats.completionRate > 0 && (
                  <Box sx={{ p: 2, bgcolor: 'warning.light', borderRadius: 1 }}>
                    <Typography variant="body2" fontWeight={600} color="warning.dark">
                      🌱 Room to Grow!
                    </Typography>
                    <Typography variant="caption" color="warning.dark">
                      Your {Math.round(stats.completionRate)}% completion rate shows potential. Small improvements add up!
                    </Typography>
                  </Box>
                )}
                {stats.currentStreak >= 7 && (
                  <Box sx={{ p: 2, bgcolor: 'error.light', borderRadius: 1 }}>
                    <Typography variant="body2" fontWeight={600} color="error.dark">
                      🔥 {stats.currentStreak} Day Streak!
                    </Typography>
                    <Typography variant="caption" color="error.dark">
                      You're on fire! Don't break the chain now.
                    </Typography>
                  </Box>
                )}
                {stats.bestStreak > stats.currentStreak && stats.currentStreak > 0 && (
                  <Box sx={{ p: 2, bgcolor: 'primary.light', borderRadius: 1 }}>
                    <Typography variant="body2" fontWeight={600} color="primary.dark">
                      🎯 Challenge Ahead!
                    </Typography>
                    <Typography variant="caption" color="primary.dark">
                      You're {stats.bestStreak - stats.currentStreak} days away from beating your record of {stats.bestStreak} days!
                    </Typography>
                  </Box>
                )}
                <Box sx={{ p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
                  <Typography variant="body2" fontWeight={600} color="text.primary">
                    📊 Quick Stats
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, mt: 1 }}>
                    <Typography variant="caption" color="text.secondary">
                      • Total Completions: {stats.totalCompleted}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      • Active Habits: {stats.totalHabits}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      • Best Day: {bestDay}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      • Date Range: {format(startDate, 'MMM dd')} - {format(today, 'MMM dd, yyyy')}
                    </Typography>
                  </Box>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Container>
  );
}

// Helper function to calculate streaks
function calculateStreaks(
  entries: Array<{ habitId: number; date: string; isCompleted: boolean }>,
  habitIds: number[],
  dates: string[]
): { current: number; best: number } {
  let currentStreak = 0;
  let bestStreak = 0;
  let tempStreak = 0;

  // Sort dates in ascending order
  const sortedDates = [...dates].sort();

  // Check each date for completion
  for (let i = sortedDates.length - 1; i >= 0; i--) {
    const date = sortedDates[i];
    const dayEntries = entries.filter(e => e.date === date && habitIds.includes(e.habitId));
    const completed = dayEntries.filter(e => e.isCompleted).length;
    const total = habitIds.length;

    // Consider day complete if at least 50% of habits are done
    const dayComplete = total > 0 && (completed / total) >= 0.5;

    if (dayComplete) {
      tempStreak++;
      if (i === sortedDates.length - 1) {
        currentStreak = tempStreak;
      }
      if (tempStreak > bestStreak) {
        bestStreak = tempStreak;
      }
    } else {
      if (i === sortedDates.length - 1) {
        currentStreak = 0;
      }
      tempStreak = 0;
    }
  }

  return { current: currentStreak, best: bestStreak };
}
