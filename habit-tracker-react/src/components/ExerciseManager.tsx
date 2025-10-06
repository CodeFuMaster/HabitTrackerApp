import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Paper,
  Divider,
} from '@mui/material';
import { Add, DragIndicator } from '@mui/icons-material';
import ExerciseCard from './ExerciseCard';
import ExerciseForm from './ExerciseForm';
import { Exercise } from '../types/habit.types';
import { offlineDb } from '../services/offlineDb';

interface ExerciseManagerProps {
  habitId: number;
  habitName: string;
  onClose?: () => void;
}

export default function ExerciseManager({ habitId, habitName, onClose }: ExerciseManagerProps) {
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editingExercise, setEditingExercise] = useState<Exercise | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);

  // Load exercises
  useEffect(() => {
    loadExercises();
  }, [habitId]);

  const loadExercises = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await offlineDb.getExercisesForHabit(habitId);
      setExercises(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load exercises');
    } finally {
      setLoading(false);
    }
  };

  const handleAdd = () => {
    setEditingExercise(null);
    setFormOpen(true);
  };

  const handleEdit = (exercise: Exercise) => {
    setEditingExercise(exercise);
    setFormOpen(true);
  };

  const handleSave = async (exercise: Exercise) => {
    try {
      // saveExercise handles both create and update
      await offlineDb.saveExercise(exercise);
      await loadExercises(); // Reload list
      setFormOpen(false);
    } catch (err: any) {
      throw new Error(err.message || 'Failed to save exercise');
    }
  };

  const handleDelete = async (exerciseId: number) => {
    try {
      await offlineDb.deleteExercise(exerciseId);
      await loadExercises(); // Reload list
      setDeleteConfirm(null);
    } catch (err: any) {
      setError(err.message || 'Failed to delete exercise');
    }
  };

  const maxOrderIndex = exercises.length > 0 
    ? Math.max(...exercises.map(e => e.orderIndex)) 
    : 0;

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h5" component="h2" fontWeight="bold">
            {habitName} - Exercises
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {exercises.length} {exercises.length === 1 ? 'exercise' : 'exercises'}
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleAdd}
        >
          Add Exercise
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Exercises List */}
      {exercises.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="body1" color="text.secondary" gutterBottom>
            No exercises yet for this habit.
          </Typography>
          <Button
            variant="outlined"
            startIcon={<Add />}
            onClick={handleAdd}
            sx={{ mt: 2 }}
          >
            Add Your First Exercise
          </Button>
        </Paper>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Paper sx={{ p: 2, bgcolor: 'info.light', color: 'info.contrastText' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <DragIndicator />
              <Typography variant="body2">
                Drag exercises to reorder them (drag-drop coming soon!)
              </Typography>
            </Box>
          </Paper>

          {exercises.map((exercise) => (
            <ExerciseCard
              key={exercise.id}
              exercise={exercise}
              onEdit={handleEdit}
              onDelete={(id) => setDeleteConfirm(id)}
            />
          ))}
        </Box>
      )}

      <Divider sx={{ my: 4 }} />

      {/* Close Button */}
      {onClose && (
        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
          <Button onClick={onClose} variant="outlined">
            Close
          </Button>
        </Box>
      )}

      {/* Exercise Form Dialog */}
      <ExerciseForm
        open={formOpen}
        exercise={editingExercise}
        habitId={habitId}
        maxOrderIndex={maxOrderIndex}
        onClose={() => setFormOpen(false)}
        onSave={handleSave}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteConfirm !== null}
        onClose={() => setDeleteConfirm(null)}
      >
        <DialogTitle>Delete Exercise?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this exercise? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button onClick={() => deleteConfirm && handleDelete(deleteConfirm)} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
