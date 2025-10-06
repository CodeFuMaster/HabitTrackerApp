import { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Typography,
  Alert,
  Stack,
} from '@mui/material';
import { Exercise, ExerciseType } from '../types/habit.types';

interface ExerciseFormProps {
  open: boolean;
  exercise: Exercise | null;
  habitId: number;
  maxOrderIndex: number;
  onClose: () => void;
  onSave: (exercise: Exercise) => Promise<void>;
}

export default function ExerciseForm({
  open,
  exercise,
  habitId,
  maxOrderIndex,
  onClose,
  onSave,
}: ExerciseFormProps) {
  const [formData, setFormData] = useState<Partial<Exercise>>({
    habitId,
    name: '',
    orderIndex: maxOrderIndex + 1,
    exerciseType: ExerciseType.Strength,
    isActive: true,
  });
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (exercise) {
      setFormData(exercise);
    } else {
      setFormData({
        habitId,
        name: '',
        orderIndex: maxOrderIndex + 1,
        exerciseType: ExerciseType.Strength,
        isActive: true,
      });
    }
    setError(null);
  }, [exercise, habitId, maxOrderIndex, open]);

  const handleChange = (field: keyof Exercise, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async () => {
    // Validation
    if (!formData.name?.trim()) {
      setError('Exercise name is required');
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const exerciseToSave: Exercise = {
        id: exercise?.id || 0,
        habitId: habitId,
        name: formData.name!,
        orderIndex: formData.orderIndex || maxOrderIndex + 1,
        exerciseType: formData.exerciseType || ExerciseType.Strength,
        targetSets: formData.targetSets,
        targetReps: formData.targetReps,
        targetWeight: formData.targetWeight,
        targetDuration: formData.targetDuration,
        targetRPE: formData.targetRPE,
        restSeconds: formData.restSeconds,
        muscleGroups: formData.muscleGroups,
        equipment: formData.equipment,
        notes: formData.notes,
        imageUrl: formData.imageUrl,
        videoUrl: formData.videoUrl,
        isActive: formData.isActive ?? true,
        createdDate: exercise?.createdDate || new Date().toISOString(),
        syncStatus: 'pending',
      };

      await onSave(exerciseToSave);
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to save exercise');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {exercise ? 'Edit Exercise' : 'Add New Exercise'}
      </DialogTitle>
      
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Stack spacing={2} sx={{ mt: 1 }}>
          {/* Basic Info */}
          <Typography variant="h6" sx={{ mt: 1 }}>Basic Information</Typography>
          
          <TextField
            label="Exercise Name"
            value={formData.name || ''}
            onChange={(e) => handleChange('name', e.target.value)}
            required
            fullWidth
            autoFocus
          />

          <Box sx={{ display: 'flex', gap: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Exercise Type</InputLabel>
              <Select
                value={formData.exerciseType || ExerciseType.Strength}
                onChange={(e) => handleChange('exerciseType', e.target.value)}
                label="Exercise Type"
              >
                <MenuItem value={ExerciseType.Strength}>Strength</MenuItem>
                <MenuItem value={ExerciseType.Cardio}>Cardio</MenuItem>
                <MenuItem value={ExerciseType.Flexibility}>Flexibility</MenuItem>
                <MenuItem value={ExerciseType.Mobility}>Mobility</MenuItem>
              </Select>
            </FormControl>

            <TextField
              label="Order"
              type="number"
              value={formData.orderIndex || ''}
              onChange={(e) => handleChange('orderIndex', parseInt(e.target.value))}
              fullWidth
            />
          </Box>

          {/* Target Metrics */}
          <Typography variant="h6" sx={{ mt: 2 }}>Target Metrics</Typography>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Sets"
              type="number"
              value={formData.targetSets || ''}
              onChange={(e) => handleChange('targetSets', e.target.value ? parseInt(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 0 }}
            />

            <TextField
              label="Reps"
              type="number"
              value={formData.targetReps || ''}
              onChange={(e) => handleChange('targetReps', e.target.value ? parseInt(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 0 }}
            />

            <TextField
              label="Weight (lbs)"
              type="number"
              value={formData.targetWeight || ''}
              onChange={(e) => handleChange('targetWeight', e.target.value ? parseFloat(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 0, step: 2.5 }}
            />

            <TextField
              label="Duration (s)"
              type="number"
              value={formData.targetDuration || ''}
              onChange={(e) => handleChange('targetDuration', e.target.value ? parseInt(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 0 }}
            />
          </Box>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="RPE (1-10)"
              type="number"
              value={formData.targetRPE || ''}
              onChange={(e) => handleChange('targetRPE', e.target.value ? parseInt(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 1, max: 10 }}
              helperText="Rate of Perceived Exertion"
            />

            <TextField
              label="Rest (seconds)"
              type="number"
              value={formData.restSeconds || ''}
              onChange={(e) => handleChange('restSeconds', e.target.value ? parseInt(e.target.value) : undefined)}
              fullWidth
              inputProps={{ min: 0 }}
            />
          </Box>

          {/* Additional Details */}
          <Typography variant="h6" sx={{ mt: 2 }}>Additional Details</Typography>

          <TextField
            label="Muscle Groups"
            value={formData.muscleGroups || ''}
            onChange={(e) => handleChange('muscleGroups', e.target.value)}
            fullWidth
            placeholder="e.g., Quads, Glutes, Core"
            helperText="Comma-separated list"
          />

          <TextField
            label="Equipment"
            value={formData.equipment || ''}
            onChange={(e) => handleChange('equipment', e.target.value)}
            fullWidth
            placeholder="e.g., Barbell, Bench, Dumbbells"
            helperText="Comma-separated list"
          />

          <TextField
            label="Notes / Instructions"
            value={formData.notes || ''}
            onChange={(e) => handleChange('notes', e.target.value)}
            fullWidth
            multiline
            rows={3}
            placeholder="Form cues, tips, progressions..."
          />

          {/* Media */}
          <Typography variant="h6" sx={{ mt: 2 }}>Media</Typography>

          <TextField
            label="Image URL"
            value={formData.imageUrl || ''}
            onChange={(e) => handleChange('imageUrl', e.target.value)}
            fullWidth
            placeholder="https://example.com/image.jpg"
          />

          <TextField
            label="Video URL"
            value={formData.videoUrl || ''}
            onChange={(e) => handleChange('videoUrl', e.target.value)}
            fullWidth
            placeholder="https://youtube.com/watch?v=..."
          />

          {/* Active Status */}
          <FormControlLabel
            control={
              <Switch
                checked={formData.isActive ?? true}
                onChange={(e) => handleChange('isActive', e.target.checked)}
              />
            }
            label="Active"
          />
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={isSaving}>
          Cancel
        </Button>
        <Button onClick={handleSubmit} variant="contained" disabled={isSaving}>
          {isSaving ? 'Saving...' : exercise ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
