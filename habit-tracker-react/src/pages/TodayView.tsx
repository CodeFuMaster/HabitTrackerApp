import { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  IconButton,
  Chip,
  CircularProgress,
  Button,
  Drawer,
  Dialog,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Badge,
  Tooltip,
  ButtonGroup,
} from '@mui/material';
import {
  CheckCircle,
  RadioButtonUnchecked,
  Schedule,
  Refresh,
  NoteAdd,
  Timer,
  EmojiEmotions,
  BatteryChargingFull,
  ExpandMore,
  ShowChart,
  PlayArrow,
  // Notifications,
  NotificationsActive,
  Snooze,
  ChevronLeft,
  ChevronRight,
  Today,
  FitnessCenter,
} from '@mui/icons-material';
import { format, addDays, subDays, isToday } from 'date-fns';
import { useQueryClient } from '@tanstack/react-query';
import { useHabitsForToday, useSync } from '../hooks/useHabits';
import { syncService } from '../services/syncService';
import type { HabitWithStatus, DailyHabitEntry, Habit, TimerSession, HabitMetricDefinition, CustomMetricValue, RoutineTemplate } from '../types/habit.types';
import ActivityLogger from '../components/ActivityLogger';
import HabitTimer from '../components/HabitTimer';
import MetricValuesDisplay from '../components/MetricValuesDisplay';
import RoutineSessionView from '../components/RoutineSessionView';
import { notificationService, formatTime } from '../services/notificationService';
import NotificationHistory from '../components/NotificationHistory';
import ATGChecklistModal from '../components/ATGChecklistModal';

export default function TodayView() {
  const queryClient = useQueryClient();
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const formattedDate = format(selectedDate, 'yyyy-MM-dd');
  
  const { habits, toggleComplete, isLoading } = useHabitsForToday(formattedDate);
  const { fullSyncNow, isSyncing } = useSync();
  const [selectedHabit, setSelectedHabit] = useState<HabitWithStatus | null>(null);
  
  // Activity Logger state
  const [activityLoggerOpen, setActivityLoggerOpen] = useState(false);
  const [loggerHabit, setLoggerHabit] = useState<Habit | null>(null);
  const [loggerEntry, setLoggerEntry] = useState<DailyHabitEntry | null>(null);
  
  // Timer state
  const [timerOpen, setTimerOpen] = useState(false);
  const [timerHabit, setTimerHabit] = useState<Habit | null>(null);
  
  // Metric history state
  const [metricDefinitions, setMetricDefinitions] = useState<HabitMetricDefinition[]>([]);
  const [metricValues, setMetricValues] = useState<CustomMetricValue[]>([]);
  const [loadingMetrics, setLoadingMetrics] = useState(false);
  
  // Routine state
  const [routineOpen, setRoutineOpen] = useState(false);
  const [routineHabit, setRoutineHabit] = useState<Habit | null>(null);
  const [activeRoutine, setActiveRoutine] = useState<RoutineTemplate | null>(null);
  const [_routineTemplates, setRoutineTemplates] = useState<RoutineTemplate[]>([]);

  // ATG Checklist state
  const [checklistOpen, setChecklistOpen] = useState(false);
  const [checklistHabit, setChecklistHabit] = useState<Habit | null>(null);

  // Notification state
  const [reminders, setReminders] = useState<Map<number, any>>(new Map());

  const completedCount = habits.filter((h) => h.isCompletedToday).length;
  const totalCount = habits.length;
  const completionPercentage = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

  // Initialize notification service and load reminders
  useEffect(() => {
    notificationService.initialize();
    
    // Load all reminders
    const allReminders = notificationService.getAllReminders();
    const reminderMap = new Map();
    allReminders.forEach(reminder => {
      reminderMap.set(reminder.habitId, reminder);
    });
    setReminders(reminderMap);
  }, []);

  // Load metric data and routines when a habit is selected
  useEffect(() => {
    const loadMetricData = async () => {
      if (selectedHabit?.id) {
        setLoadingMetrics(true);
        try {
          const definitions = await syncService.getMetricDefinitions(selectedHabit.id);
          const values = await syncService.getMetricValuesForHabit(selectedHabit.id);
          const templates = await syncService.getRoutineTemplates(selectedHabit.id);
          setMetricDefinitions(definitions);
          setMetricValues(values);
          setRoutineTemplates(templates.filter((t: RoutineTemplate) => t.isActive));
        } catch (error) {
          console.error('Failed to load metric data:', error);
        } finally {
          setLoadingMetrics(false);
        }
      } else {
        setMetricDefinitions([]);
        setMetricValues([]);
        setRoutineTemplates([]);
      }
    };
    loadMetricData();
  }, [selectedHabit]);

  // Handler to open activity logger
  const handleOpenActivityLogger = (habit: HabitWithStatus, e: React.MouseEvent) => {
    e.stopPropagation();
    
    // Create or get today's entry
    const entry: DailyHabitEntry = habit.todayEntry || {
      id: Date.now(),
      habitId: habit.id,
      date: format(new Date(), 'yyyy-MM-dd'),
      isCompleted: habit.isCompletedToday,
      completedAt: habit.isCompletedToday ? new Date().toISOString() : undefined,
    };
    
    setLoggerHabit(habit);
    setLoggerEntry(entry);
    setActivityLoggerOpen(true);
  };

  // Handler to save activity log
  const handleSaveActivity = async (updates: Partial<DailyHabitEntry>) => {
    if (loggerEntry) {
      await syncService.updateDailyEntry(loggerEntry.id, {
        ...loggerEntry,
        ...updates,
      });
      setActivityLoggerOpen(false);
      // Invalidate queries to refresh data
      const today = format(new Date(), 'yyyy-MM-dd');
      queryClient.invalidateQueries({ queryKey: ['daily-entries', today] });
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    }
  };

  // Handler to open timer
  const handleOpenTimer = (habit: HabitWithStatus, e: React.MouseEvent) => {
    e.stopPropagation();
    setTimerHabit(habit);
    setTimerOpen(true);
  };

  // Handler when timer completes
  const handleTimerComplete = async (durationSeconds: number) => {
    if (timerHabit) {
      const session: TimerSession = {
        id: Date.now(),
        habitId: timerHabit.id,
        startTime: new Date(Date.now() - durationSeconds * 1000).toISOString(),
        endTime: new Date().toISOString(),
        duration: durationSeconds,
        isPaused: false,
        totalPausedTime: 0,
        type: 'timer',
      };
      
      await syncService.saveTimerSession(session);
      setTimerOpen(false);
    }
  };

  // Handler to open routine
  const handleOpenRoutine = (habit: HabitWithStatus, template: RoutineTemplate, e: React.MouseEvent) => {
    e.stopPropagation();
    setRoutineHabit(habit);
    setActiveRoutine(template);
    setRoutineOpen(true);
  };

  // Handler when routine completes
  const handleRoutineComplete = async (sessionData: {
    completedSteps: number[];
    totalDuration: number;
    metrics: Record<number, any>;
  }) => {
    if (routineHabit && activeRoutine) {
      // Create/update today's entry
      const entry: DailyHabitEntry = {
        id: Date.now(),
        habitId: routineHabit.id,
        date: format(new Date(), 'yyyy-MM-dd'),
        isCompleted: true,
        completedAt: new Date().toISOString(),
        notes: `Completed routine: ${activeRoutine.name} (${Math.floor(sessionData.totalDuration / 60)} min)`,
      };

      await syncService.updateDailyEntry(entry.id, entry);
      
      // Mark habit as complete
      await toggleComplete(routineHabit.id);
      
      setRoutineOpen(false);
      setRoutineHabit(null);
      setActiveRoutine(null);
      
      // Invalidate queries to refresh data
      const today = format(new Date(), 'yyyy-MM-dd');
      queryClient.invalidateQueries({ queryKey: ['daily-entries', today] });
      queryClient.invalidateQueries({ queryKey: ['habits'] });
    }
  };

  // Handler to snooze reminder
  const handleSnoozeReminder = (habitId: number, e: React.MouseEvent) => {
    e.stopPropagation();
    notificationService.snoozeReminder(habitId, 15); // Snooze for 15 minutes
  };

  // Get mood emoji
  const getMoodEmoji = (mood?: number) => {
    if (!mood) return null;
    const emojis = ['😢', '😕', '😐', '😊', '😄'];
    return emojis[mood - 1] || null;
  };

  // Get energy emoji
  const getEnergyEmoji = (energy?: number) => {
    if (!energy) return null;
    const labels = ['Very Low', 'Low', 'Medium', 'High', 'Very High'];
    return labels[energy - 1] || null;
  };

  // Date navigation handlers
  const handlePreviousDay = () => {
    setSelectedDate(prev => subDays(prev, 1));
  };

  const handleNextDay = () => {
    setSelectedDate(prev => addDays(prev, 1));
  };

  const handleToday = () => {
    setSelectedDate(new Date());
  };

  const isTodaySelected = isToday(selectedDate);

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 3 }}>
      <Container maxWidth="lg">
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignments: 'center', mb: 2, flexWrap: 'wrap', gap: 2 }}>
            <Typography variant="h4" component="h1" fontWeight="bold">
              {isTodaySelected ? 'Today' : 'Day View'}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <NotificationHistory />
              <Tooltip title="Full Sync (Reset & Pull All Data)">
                <IconButton onClick={() => fullSyncNow()} disabled={isSyncing}>
                  <Refresh sx={{ animation: isSyncing ? 'spin 1s linear infinite' : 'none' }} />
                </IconButton>
              </Tooltip>
            </Box>
          </Box>

          {/* Date Navigation */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
            <ButtonGroup variant="outlined" size="small">
              <Button onClick={handlePreviousDay} startIcon={<ChevronLeft />}>
                Previous
              </Button>
              <Button 
                onClick={handleToday} 
                disabled={isTodaySelected}
                startIcon={<Today />}
              >
                Today
              </Button>
              <Button onClick={handleNextDay} endIcon={<ChevronRight />}>
                Next
              </Button>
            </ButtonGroup>
            <Typography variant="h6" color="text.secondary">
              {format(selectedDate, 'EEEE, MMMM d, yyyy')}
            </Typography>
          </Box>

          {/* Progress Summary */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 2 }}>
            <Box sx={{ position: 'relative', display: 'inline-flex' }}>
              <CircularProgress
                variant="determinate"
                value={completionPercentage}
                size={60}
                thickness={4}
              />
              <Box
                sx={{
                  top: 0,
                  left: 0,
                  bottom: 0,
                  right: 0,
                  position: 'absolute',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Typography variant="caption" component="div" fontWeight="bold">
                  {completedCount}/{totalCount}
                </Typography>
              </Box>
            </Box>
            <Box>
              <Typography variant="body1" fontWeight="medium">
                {completedCount} of {totalCount} completed
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {completionPercentage.toFixed(0)}% done
              </Typography>
            </Box>
          </Box>
        </Box>

        {/* Habits Grid */}
        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : habits.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <Typography variant="h6" color="text.secondary">
              No habits scheduled for today
            </Typography>
            <Button variant="contained" sx={{ mt: 2 }}>
              Create Your First Habit
            </Button>
          </Box>
        ) : (
          <Grid container spacing={3}>
            {habits.map((habit) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={habit.id}>
                <Card
                  sx={{
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    cursor: 'pointer',
                    transition: 'all 0.2s',
                    borderLeft: `4px solid ${habit.color || '#6366F1'}`,
                    '&:hover': {
                      transform: 'translateY(-4px)',
                      boxShadow: 6,
                    },
                    opacity: habit.isCompletedToday ? 0.7 : 1,
                  }}
                  onClick={() => setSelectedHabit(habit)}
                >
                  <CardContent sx={{ flexGrow: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 2 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flex: 1 }}>
                        <Typography variant="h6" component="h2" fontWeight="medium">
                          {habit.name}
                        </Typography>
                        {/* Notification indicator */}
                        {reminders.has(habit.id) && reminders.get(habit.id).enabled && (
                          <Tooltip title={`Reminder set for ${formatTime(reminders.get(habit.id).time)}`}>
                            <Badge color="primary" variant="dot">
                              <NotificationsActive sx={{ fontSize: 18, color: 'primary.main' }} />
                            </Badge>
                          </Tooltip>
                        )}
                      </Box>
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          toggleComplete(habit.id);
                        }}
                      >
                        {habit.isCompletedToday ? (
                          <CheckCircle color="success" />
                        ) : (
                          <RadioButtonUnchecked color="action" />
                        )}
                      </IconButton>
                    </Box>

                    {habit.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        {habit.description}
                      </Typography>
                    )}

                    {habit.scheduledTime && (
                      <Chip
                        icon={<Schedule />}
                        label={habit.scheduledTime}
                        size="small"
                        sx={{ mb: 1 }}
                      />
                    )}

                    {habit.duration && (
                      <Chip
                        label={`${habit.duration} min`}
                        size="small"
                        variant="outlined"
                        sx={{ mb: 1, ml: 1 }}
                      />
                    )}

                    {habit.tags && habit.tags.length > 0 && (
                      <Box sx={{ mt: 2, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                        {habit.tags.map((tag) => (
                          <Chip key={tag} label={tag} size="small" variant="outlined" />
                        ))}
                      </Box>
                    )}

                    {/* Activity indicators */}
                    {habit.isCompletedToday && habit.todayEntry && (
                      <Box sx={{ mt: 2, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                        {habit.todayEntry.mood && (
                          <Chip
                            icon={<EmojiEmotions />}
                            label={getMoodEmoji(habit.todayEntry.mood)}
                            size="small"
                            color="primary"
                            variant="outlined"
                          />
                        )}
                        {habit.todayEntry.energyLevel && (
                          <Chip
                            icon={<BatteryChargingFull />}
                            label={`Energy: ${getEnergyEmoji(habit.todayEntry.energyLevel)}`}
                            size="small"
                            color="secondary"
                            variant="outlined"
                          />
                        )}
                        {habit.todayEntry.rating && (
                          <Chip
                            label={`⭐ ${habit.todayEntry.rating}/5`}
                            size="small"
                            variant="outlined"
                          />
                        )}
                      </Box>
                    )}
                  </CardContent>

                  <CardActions sx={{ px: 2, pb: 2, justifyContent: 'space-between' }}>
                    {habit.isCompletedToday && habit.todayEntry?.completedAt ? (
                      <Typography variant="caption" color="success.main">
                        ✓ Completed at {format(new Date(habit.todayEntry.completedAt), 'h:mm a')}
                      </Typography>
                    ) : (
                      <Box />
                    )}
                    
                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                      {/* Snooze button for habits with reminders */}
                      {reminders.has(habit.id) && reminders.get(habit.id).enabled && !habit.isCompletedToday && (
                        <Tooltip title="Snooze for 15 minutes">
                          <IconButton
                            size="small"
                            onClick={(e) => handleSnoozeReminder(habit.id, e)}
                          >
                            <Snooze fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )}
                      <IconButton
                        size="small"
                        onClick={(e) => {
                          e.stopPropagation();
                          setChecklistHabit(habit);
                          setChecklistOpen(true);
                        }}
                        title="Exercise Checklist"
                      >
                        <FitnessCenter fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={(e) => handleOpenActivityLogger(habit, e)}
                        title="Log Activity Details"
                      >
                        <NoteAdd fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={(e) => handleOpenTimer(habit, e)}
                        title="Start Timer"
                      >
                        <Timer fontSize="small" />
                      </IconButton>
                      {(habit as any).routineTemplates && (habit as any).routineTemplates.length > 0 && (
                        <IconButton
                          size="small"
                          onClick={(e) => {
                            const templates = (habit as any).routineTemplates.filter((t: RoutineTemplate) => t.isActive);
                            if (templates.length > 0) {
                              handleOpenRoutine(habit, templates[0], e);
                            }
                          }}
                          title="Start Routine"
                        >
                          <PlayArrow fontSize="small" />
                        </IconButton>
                      )}
                    </Box>
                  </CardActions>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
      </Container>

      {/* Activity Panel Drawer */}
      <Drawer
        anchor="right"
        open={!!selectedHabit}
        onClose={() => setSelectedHabit(null)}
        sx={{
          '& .MuiDrawer-paper': {
            width: { xs: '100%', sm: 500 },
            p: 3,
          },
        }}
      >
        {selectedHabit && (
          <Box>
            <Typography variant="h5" gutterBottom>
              {selectedHabit.name}
            </Typography>
            <Typography variant="body1" color="text.secondary" paragraph>
              {selectedHabit.description}
            </Typography>

            <Box sx={{ mt: 3 }}>
              <Button
                variant="contained"
                fullWidth
                size="large"
                startIcon={selectedHabit.isCompletedToday ? <CheckCircle /> : <RadioButtonUnchecked />}
                onClick={() => {
                  toggleComplete(selectedHabit.id);
                  setSelectedHabit(null);
                }}
                color={selectedHabit.isCompletedToday ? 'success' : 'primary'}
              >
                {selectedHabit.isCompletedToday ? 'Mark as Incomplete' : 'Mark as Complete'}
              </Button>
            </Box>

            <Divider sx={{ my: 3 }} />

            {/* Today's Activity */}
            {selectedHabit.todayEntry && (
              <Box sx={{ mb: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Today's Activity
                </Typography>
                
                {selectedHabit.todayEntry.notes && (
                  <Box sx={{ mb: 2 }}>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Notes
                    </Typography>
                    <Typography variant="body2">{selectedHabit.todayEntry.notes}</Typography>
                  </Box>
                )}

                {(selectedHabit.todayEntry.mood || selectedHabit.todayEntry.energyLevel || selectedHabit.todayEntry.rating) && (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
                    {selectedHabit.todayEntry.mood && (
                      <Chip
                        icon={<EmojiEmotions />}
                        label={`Mood: ${getMoodEmoji(selectedHabit.todayEntry.mood)}`}
                        size="small"
                        color="primary"
                      />
                    )}
                    {selectedHabit.todayEntry.energyLevel && (
                      <Chip
                        icon={<BatteryChargingFull />}
                        label={`Energy: ${getEnergyEmoji(selectedHabit.todayEntry.energyLevel)}`}
                        size="small"
                        color="secondary"
                      />
                    )}
                    {selectedHabit.todayEntry.rating && (
                      <Chip
                        label={`⭐ ${selectedHabit.todayEntry.rating}/5`}
                        size="small"
                      />
                    )}
                  </Box>
                )}
              </Box>
            )}

            {/* Custom Metrics History */}
            {metricDefinitions.length > 0 && (
              <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <ShowChart color="primary" />
                    <Typography variant="h6">Metric History</Typography>
                    {metricValues.length > 0 && (
                      <Chip label={`${metricValues.length} records`} size="small" />
                    )}
                  </Box>
                </AccordionSummary>
                <AccordionDetails>
                  {loadingMetrics ? (
                    <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                      <CircularProgress size={24} />
                    </Box>
                  ) : (
                    <MetricValuesDisplay
                      metrics={metricDefinitions}
                      values={metricValues}
                      compact={false}
                    />
                  )}
                </AccordionDetails>
              </Accordion>
            )}

            {metricDefinitions.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 3 }}>
                <ShowChart sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
                <Typography variant="body2" color="text.secondary">
                  No custom metrics defined for this habit
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Add metrics in the Habits management page
                </Typography>
              </Box>
            )}
          </Box>
        )}
      </Drawer>

      {/* Activity Logger Dialog */}
      <Dialog 
        open={activityLoggerOpen} 
        onClose={() => setActivityLoggerOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        {loggerHabit && loggerEntry && (
          <ActivityLogger
            habit={loggerHabit}
            entry={loggerEntry}
            onSave={handleSaveActivity}
            onClose={() => setActivityLoggerOpen(false)}
          />
        )}
      </Dialog>

      {/* Timer Dialog */}
      <Dialog 
        open={timerOpen} 
        onClose={() => setTimerOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        {timerHabit && (
          <HabitTimer
            habit={timerHabit}
            onComplete={handleTimerComplete}
          />
        )}
      </Dialog>

      {/* Routine Session Dialog */}
      <Dialog 
        open={routineOpen} 
        onClose={() => setRoutineOpen(false)}
        maxWidth="md"
        fullWidth
      >
        {routineHabit && activeRoutine && (
          <RoutineSessionView
            habit={routineHabit}
            template={activeRoutine}
            onComplete={handleRoutineComplete}
            onClose={() => setRoutineOpen(false)}
          />
        )}
      </Dialog>

      {/* ATG Checklist Modal */}
      {checklistHabit && (
        <ATGChecklistModal
          open={checklistOpen}
          habitId={checklistHabit.id}
          habitName={checklistHabit.name}
          date={formattedDate}
          onClose={() => {
            setChecklistOpen(false);
            setChecklistHabit(null);
          }}
          onComplete={() => {
            // Refresh habits list after completing exercises
            queryClient.invalidateQueries({ queryKey: ['habits', 'today', formattedDate] });
          }}
        />
      )}
    </Box>
  );
}
