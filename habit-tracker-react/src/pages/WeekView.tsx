import { useState } from 'react';
import {
  Box,
  Container,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  CircularProgress,
} from '@mui/material';
import {
  ChevronLeft,
  ChevronRight,
  CheckCircle,
  RadioButtonUnchecked,
  Today as TodayIcon,
} from '@mui/icons-material';
import {
  format,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  addWeeks,
  subWeeks,
  isSameDay,
} from 'date-fns';
import { useHabits, useWeekEntries, useToggleHabitCompletion } from '../hooks/useHabits';
import type { Habit } from '../types/habit.types';

export default function WeekView() {
  const [currentWeekStart, setCurrentWeekStart] = useState(
    startOfWeek(new Date(), { weekStartsOn: 1 }) // Monday
  );

  const { habits, isLoading: habitsLoading } = useHabits();
  const { toggleCompletion } = useToggleHabitCompletion();

  const weekEnd = endOfWeek(currentWeekStart, { weekStartsOn: 1 });
  const weekDays = eachDayOfInterval({ start: currentWeekStart, end: weekEnd });
  
  // Format dates for the week
  const weekDateStrings = weekDays.map(day => format(day, 'yyyy-MM-dd'));
  
  // Load all entries for the week
  const { isLoading: entriesLoading, isHabitCompletedOnDate } = useWeekEntries(weekDateStrings);

  const goToPreviousWeek = () => setCurrentWeekStart(subWeeks(currentWeekStart, 1));
  const goToNextWeek = () => setCurrentWeekStart(addWeeks(currentWeekStart, 1));
  const goToCurrentWeek = () => setCurrentWeekStart(startOfWeek(new Date(), { weekStartsOn: 1 }));

  const toggleDayCompletion = async (habitId: number, date: Date) => {
    const dateStr = format(date, 'yyyy-MM-dd');
    toggleCompletion({ habitId, date: dateStr });
  };

  // Calculate streak for each habit (consecutive days completed)
  const calculateStreak = (habit: Habit): number => {
    let streak = 0;
    const today = new Date();
    
    // Count backwards from today
    for (let i = 0; i < 365; i++) {
      const checkDate = new Date(today);
      checkDate.setDate(today.getDate() - i);
      const dateStr = format(checkDate, 'yyyy-MM-dd');
      
      if (isHabitCompletedOnDate(habit.id, dateStr)) {
        streak++;
      } else if (i > 0) {
        // Break if we hit a non-completed day (but not on the first day)
        break;
      }
    }
    
    return streak;
  };

  // Calculate completion percentage for the week
  const getWeekCompletionPercentage = (): number => {
    const totalSlots = habits.length * weekDays.length;
    if (totalSlots === 0) return 0;

    let completed = 0;
    habits.forEach(habit => {
      weekDateStrings.forEach(dateStr => {
        if (isHabitCompletedOnDate(habit.id, dateStr)) {
          completed++;
        }
      });
    });

    return Math.round((completed / totalSlots) * 100);
  };

  // Find the best day of the week
  const getBestDay = (): Date => {
    const dayCompletions = weekDays.map(day => {
      const dateStr = format(day, 'yyyy-MM-dd');
      let count = 0;
      habits.forEach(habit => {
        if (isHabitCompletedOnDate(habit.id, dateStr)) {
          count++;
        }
      });
      return { day, count };
    });

    const best = dayCompletions.reduce((max, current) => 
      current.count > max.count ? current : max
    , dayCompletions[0]);

    return best.day;
  };

  const isLoading = habitsLoading || entriesLoading;

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  const isCurrentWeek = isSameDay(currentWeekStart, startOfWeek(new Date(), { weekStartsOn: 1 }));

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 3 }}>
      <Container maxWidth="xl">
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h4" component="h1" fontWeight="bold">
              Week View
            </Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <IconButton onClick={goToPreviousWeek}>
                <ChevronLeft />
              </IconButton>
              <IconButton onClick={goToCurrentWeek} disabled={isCurrentWeek} color="primary">
                <TodayIcon />
              </IconButton>
              <IconButton onClick={goToNextWeek}>
                <ChevronRight />
              </IconButton>
            </Box>
          </Box>

          <Typography variant="h6" color="text.secondary" gutterBottom>
            {format(currentWeekStart, 'MMMM d')} - {format(weekEnd, 'MMMM d, yyyy')}
          </Typography>

          {/* Week Summary */}
          <Box sx={{ display: 'flex', gap: 3, mt: 2 }}>
            <Chip
              label={`${habits.length} habits`}
              color="primary"
              variant="outlined"
            />
            <Chip
              label={`${getWeekCompletionPercentage()}% complete`}
              color={getWeekCompletionPercentage() >= 70 ? 'success' : 'default'}
            />
          </Box>
        </Box>

        {/* Week Table */}
        <TableContainer component={Paper} sx={{ boxShadow: 3 }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold', minWidth: 200 }}>
                  Habit
                </TableCell>
                {weekDays.map((day) => {
                  const isToday = isSameDay(day, new Date());
                  return (
                    <TableCell
                      key={day.toISOString()}
                      align="center"
                      sx={{
                        minWidth: 80,
                        bgcolor: isToday ? 'primary.light' : 'inherit',
                        color: isToday ? 'primary.contrastText' : 'inherit',
                        fontWeight: isToday ? 'bold' : 'normal',
                      }}
                    >
                      <Box>
                        <Typography variant="caption" display="block">
                          {format(day, 'EEE')}
                        </Typography>
                        <Typography variant="body2">
                          {format(day, 'd')}
                        </Typography>
                      </Box>
                    </TableCell>
                  );
                })}
                <TableCell align="center" sx={{ fontWeight: 'bold' }}>
                  Streak
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {habits.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={weekDays.length + 2} align="center" sx={{ py: 8 }}>
                    <Typography variant="h6" color="text.secondary">
                      No habits found. Create your first habit!
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                habits.map((habit) => (
                  <TableRow
                    key={habit.id}
                    hover
                    sx={{
                      '&:hover': { bgcolor: 'action.hover' },
                    }}
                  >
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box
                          sx={{
                            width: 4,
                            height: 40,
                            bgcolor: habit.color || 'primary.main',
                            borderRadius: 1,
                          }}
                        />
                        <Box>
                          <Typography variant="body1" fontWeight="medium">
                            {habit.name}
                          </Typography>
                          {habit.timeOfDay && (
                            <Typography variant="caption" color="text.secondary">
                              {habit.timeOfDay}
                            </Typography>
                          )}
                        </Box>
                      </Box>
                    </TableCell>

                    {weekDays.map((day) => {
                      const dateStr = format(day, 'yyyy-MM-dd');
                      const isCompleted = isHabitCompletedOnDate(habit.id, dateStr);
                      const isPastDay = day < new Date() && !isSameDay(day, new Date());

                      return (
                        <TableCell key={day.toISOString()} align="center">
                          <IconButton
                            size="small"
                            onClick={() => toggleDayCompletion(habit.id, day)}
                            sx={{
                              color: isCompleted ? 'success.main' : 'action.disabled',
                              opacity: isPastDay && !isCompleted ? 0.3 : 1,
                            }}
                          >
                            {isCompleted ? (
                              <CheckCircle fontSize="medium" />
                            ) : (
                              <RadioButtonUnchecked fontSize="medium" />
                            )}
                          </IconButton>
                        </TableCell>
                      );
                    })}

                    <TableCell align="center">
                      <Chip
                        label={`${calculateStreak(habit)} days`}
                        size="small"
                        color={calculateStreak(habit) >= 7 ? 'success' : 'default'}
                      />
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {/* Week Stats */}
        {habits.length > 0 && (
          <Box sx={{ mt: 4, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Paper sx={{ p: 3, flex: 1, minWidth: 200 }}>
              <Typography variant="h6" gutterBottom>
                Completion Rate
              </Typography>
              <Typography variant="h3" color="primary.main">
                {getWeekCompletionPercentage()}%
              </Typography>
              <Typography variant="body2" color="text.secondary">
                of this week
              </Typography>
            </Paper>

            <Paper sx={{ p: 3, flex: 1, minWidth: 200 }}>
              <Typography variant="h6" gutterBottom>
                Total Habits
              </Typography>
              <Typography variant="h3" color="secondary.main">
                {habits.length}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                active habits
              </Typography>
            </Paper>

            <Paper sx={{ p: 3, flex: 1, minWidth: 200 }}>
              <Typography variant="h6" gutterBottom>
                Best Day
              </Typography>
              <Typography variant="h3" color="success.main">
                {habits.length > 0 ? format(getBestDay(), 'EEE') : '-'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                most completions
              </Typography>
            </Paper>
          </Box>
        )}
      </Container>
    </Box>
  );
}
