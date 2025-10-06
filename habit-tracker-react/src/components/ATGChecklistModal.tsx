import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box,
  Typography,
  Checkbox,
  Button,
  LinearProgress,
  Paper,
  Chip,
  Divider,
  Alert,
  CircularProgress,
  IconButton,
} from '@mui/material';
import {
  CheckCircle,
  RadioButtonUnchecked,
  Close,
  PlayArrow,
  FitnessCenter,
} from '@mui/icons-material';
import { format } from 'date-fns';
import { Exercise, ExerciseLog, ExerciseWithLogs } from '../types/habit.types';
import { offlineDb } from '../services/offlineDb';

interface ATGChecklistModalProps {
  open: boolean;
  habitId: number;
  habitName: string;
  date: string; // yyyy-MM-dd format
  onClose: () => void;
  onComplete?: () => void;
}

export default function ATGChecklistModal({
  open,
  habitId,
  habitName,
  date,
  onClose,
  onComplete,
}: ATGChecklistModalProps) {
  const [exercises, setExercises] = useState<ExerciseWithLogs[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load exercises and their logs for the date
  useEffect(() => {
    if (open) {
      loadExercises();
    }
  }, [open, habitId, date]);

  const loadExercises = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await offlineDb.getExercisesWithLogsForDate(habitId, date);
      setExercises(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load exercises');
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = async (exercise: Exercise, exerciseWithLogs: ExerciseWithLogs) => {
    try {
      const isCurrentlyCompleted = exerciseWithLogs.logs && exerciseWithLogs.logs.length > 0;

      if (isCurrentlyCompleted) {
        // Delete all logs for this exercise on this date
        for (const log of exerciseWithLogs.logs || []) {
          if (log.id) {
            await offlineDb.deleteExerciseLog(log.id);
          }
        }
      } else {
        // Create a simple completion log
        const log: ExerciseLog = {
          id: 0,
          exerciseId: exercise.id!,
          dailyHabitEntryId: 0,
          date: date,
          setNumber: 1,
          actualReps: exercise.targetReps,
          actualWeight: exercise.targetWeight,
          actualDuration: exercise.targetDuration,
          actualRPE: exercise.targetRPE,
          completedAt: new Date().toISOString(),
          syncStatus: 'pending',
        };
        await offlineDb.saveExerciseLog(log);
      }

      // Reload to show updated state
      await loadExercises();
    } catch (err: any) {
      setError(err.message || 'Failed to toggle exercise');
    }
  };

  const handleCompleteAll = async () => {
    setSaving(true);
    setError(null);
    try {
      const logsToCreate: ExerciseLog[] = [];

      for (const exerciseWithLogs of exercises) {
        const isAlreadyCompleted = exerciseWithLogs.logs && exerciseWithLogs.logs.length > 0;

        if (!isAlreadyCompleted) {
          // Create log for uncompleted exercises
          const log: ExerciseLog = {
            id: 0,
            exerciseId: exerciseWithLogs.id!,
            dailyHabitEntryId: 0,
            date: date,
            setNumber: 1,
            actualReps: exerciseWithLogs.targetReps,
            actualWeight: exerciseWithLogs.targetWeight,
            actualDuration: exerciseWithLogs.targetDuration,
            actualRPE: exerciseWithLogs.targetRPE,
            completedAt: new Date().toISOString(),
            syncStatus: 'pending',
          };
          logsToCreate.push(log);
        }
      }

      // Batch save all logs
      if (logsToCreate.length > 0) {
        await offlineDb.batchSaveExerciseLogs(logsToCreate);
      }

      // Reload to show all completed
      await loadExercises();

      // Notify parent
      if (onComplete) {
        onComplete();
      }
    } catch (err: any) {
      setError(err.message || 'Failed to complete all exercises');
    } finally {
      setSaving(false);
    }
  };

  const completedCount = exercises.filter(e => e.logs && e.logs.length > 0).length;
  const totalCount = exercises.length;
  const progress = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;
  const allCompleted = completedCount === totalCount && totalCount > 0;

  const getLastPerformance = (exerciseWithLogs: ExerciseWithLogs): string => {
    const logs = exerciseWithLogs.logs;
    if (!logs || logs.length === 0) return '';

    const log = logs[0]; // Most recent log
    const parts: string[] = [];

    if (log.actualReps) {
      parts.push(`${log.actualReps} reps`);
    }
    if (log.actualWeight) {
      parts.push(`${log.actualWeight} lbs`);
    }
    if (log.actualDuration) {
      parts.push(`${log.actualDuration}s`);
    }
    if (log.actualRPE) {
      parts.push(`RPE ${log.actualRPE}`);
    }

    return parts.length > 0 ? `Last: ${parts.join(', ')}` : '';
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <FitnessCenter color="primary" />
            <Typography variant="h6" component="span">
              {habitName}
            </Typography>
          </Box>
          <IconButton size="small" onClick={onClose}>
            <Close />
          </IconButton>
        </Box>
        <Typography variant="caption" color="text.secondary">
          {format(new Date(date), 'EEEE, MMMM d, yyyy')}
        </Typography>
      </DialogTitle>

      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Progress Bar */}
        <Paper sx={{ p: 2, mb: 2, bgcolor: 'primary.light', color: 'primary.contrastText' }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2" fontWeight="bold">
              Progress
            </Typography>
            <Typography variant="body2" fontWeight="bold">
              {completedCount} / {totalCount}
            </Typography>
          </Box>
          <LinearProgress
            variant="determinate"
            value={progress}
            sx={{
              height: 8,
              borderRadius: 1,
              bgcolor: 'rgba(255,255,255,0.3)',
              '& .MuiLinearProgress-bar': {
                bgcolor: 'success.main',
              },
            }}
          />
          {allCompleted && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
              <CheckCircle color="success" fontSize="small" />
              <Typography variant="body2" sx={{ color: 'success.main', fontWeight: 'bold' }}>
                All exercises complete! 🎉
              </Typography>
            </Box>
          )}
        </Paper>

        {/* Loading State */}
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
            <CircularProgress />
          </Box>
        )}

        {/* Exercise List */}
        {!loading && exercises.length === 0 && (
          <Alert severity="info">
            No exercises found for this habit. Add exercises in the Manage Exercises section.
          </Alert>
        )}

        {!loading && exercises.length > 0 && (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {exercises.map((exerciseWithLogs) => {
              const isCompleted = exerciseWithLogs.logs && exerciseWithLogs.logs.length > 0;
              const lastPerformance = getLastPerformance(exerciseWithLogs);

              return (
                <Paper
                  key={exerciseWithLogs.id}
                  sx={{
                    p: 2,
                    cursor: 'pointer',
                    transition: 'all 0.2s',
                    border: '2px solid',
                    borderColor: isCompleted ? 'success.main' : 'divider',
                    bgcolor: isCompleted ? 'success.light' : 'background.paper',
                    '&:hover': {
                      boxShadow: 2,
                      borderColor: isCompleted ? 'success.dark' : 'primary.main',
                    },
                  }}
                  onClick={() => handleToggle(exerciseWithLogs, exerciseWithLogs)}
                >
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                    {/* Checkbox */}
                    <Checkbox
                      checked={isCompleted}
                      icon={<RadioButtonUnchecked />}
                      checkedIcon={<CheckCircle />}
                      sx={{ p: 0 }}
                    />

                    {/* Exercise Info */}
                    <Box sx={{ flex: 1 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                        <Typography variant="body1" fontWeight={isCompleted ? 'bold' : 'normal'}>
                          {exerciseWithLogs.orderIndex}. {exerciseWithLogs.name}
                        </Typography>
                        {exerciseWithLogs.videoUrl && (
                          <IconButton
                            size="small"
                            component="a"
                            href={exerciseWithLogs.videoUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            onClick={(e) => e.stopPropagation()}
                            sx={{ p: 0 }}
                          >
                            <PlayArrow fontSize="small" color="primary" />
                          </IconButton>
                        )}
                      </Box>

                      {/* Target Metrics */}
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 0.5 }}>
                        {exerciseWithLogs.targetSets && exerciseWithLogs.targetReps && (
                          <Chip
                            label={`${exerciseWithLogs.targetSets}x${exerciseWithLogs.targetReps}`}
                            size="small"
                            variant="outlined"
                          />
                        )}
                        {exerciseWithLogs.targetWeight && (
                          <Chip label={`${exerciseWithLogs.targetWeight} lbs`} size="small" variant="outlined" />
                        )}
                        {exerciseWithLogs.targetDuration && (
                          <Chip label={`${exerciseWithLogs.targetDuration}s`} size="small" variant="outlined" />
                        )}
                        {exerciseWithLogs.targetRPE && (
                          <Chip label={`RPE ${exerciseWithLogs.targetRPE}`} size="small" variant="outlined" color="primary" />
                        )}
                      </Box>

                      {/* Last Performance */}
                      {lastPerformance && (
                        <Typography variant="caption" color="text.secondary">
                          {lastPerformance}
                        </Typography>
                      )}

                      {/* Notes (collapsed by default) */}
                      {exerciseWithLogs.notes && (
                        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                          {exerciseWithLogs.notes.substring(0, 80)}
                          {exerciseWithLogs.notes.length > 80 && '...'}
                        </Typography>
                      )}
                    </Box>
                  </Box>
                </Paper>
              );
            })}
          </Box>
        )}
      </DialogContent>

      <Divider />

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} variant="outlined">
          Close
        </Button>
        <Button
          onClick={handleCompleteAll}
          variant="contained"
          disabled={allCompleted || saving || loading || exercises.length === 0}
          startIcon={saving ? <CircularProgress size={16} /> : <CheckCircle />}
        >
          {allCompleted ? 'All Complete' : 'Complete All'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
