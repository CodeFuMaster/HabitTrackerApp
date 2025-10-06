import { useState } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  CardActions,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  FormGroup,
  InputLabel,
  Switch,
  FormControlLabel,
  Grid,
  Alert,
  CircularProgress,
  Divider,
  Checkbox,
} from '@mui/material';
import {
  Add,
  Edit,
  Delete,
  ContentCopy,
  Archive,
  Unarchive,
  FitnessCenter,
} from '@mui/icons-material';
import { 
  useHabits, 
  useCategories,
  useCreateHabit,
  useUpdateHabit,
  useDeleteHabit,
  useDuplicateHabit,
} from '../hooks/useHabits';
import type { Habit, RecurrenceType, HabitMetricDefinition, RoutineTemplate } from '../types/habit.types';
import { RecurrenceType as RecurrenceEnum } from '../types/habit.types';
import CustomMetricsManager from '../components/CustomMetricsManager';
import RoutineTemplateManager from '../components/RoutineTemplateManager';
import ReminderManager from '../components/ReminderManager';
import ExerciseManager from '../components/ExerciseManager';

export default function HabitsView() {
  const { habits, isLoading } = useHabits();
  const { categories } = useCategories();
  const { createHabitAsync, isCreating } = useCreateHabit();
  const { updateHabitAsync, isUpdating } = useUpdateHabit();
  const { deleteHabitAsync, isDeleting } = useDeleteHabit();
  const { duplicateHabitAsync, isDuplicating } = useDuplicateHabit();
  
  const [openDialog, setOpenDialog] = useState(false);
  const [editingHabit, setEditingHabit] = useState<Habit | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);
  const [exerciseManagerOpen, setExerciseManagerOpen] = useState(false);
  const [selectedHabitForExercises, setSelectedHabitForExercises] = useState<Habit | null>(null);
  const [monthlyDaysInput, setMonthlyDaysInput] = useState('');

  // Form state
  const [formData, setFormData] = useState<Partial<Habit>>({
    name: '',
    description: '',
    shortDescription: '',
    categoryId: undefined,
    recurrenceType: RecurrenceEnum.Daily,
    recurrenceInterval: 1,
    specificDaysOfWeek: [],
    specificDaysOfMonth: [],
    timeOfDay: '',
    timeOfDayEnd: '',
    duration: undefined,
    isActive: true,
    color: '#6366F1',
    reminderEnabled: false,
    reminderTime: '',
    tags: [],
    icon: '',
    imageUrl: '',
  });

  const dayOptions = [
    { label: 'Sun', value: 0 },
    { label: 'Mon', value: 1 },
    { label: 'Tue', value: 2 },
    { label: 'Wed', value: 3 },
    { label: 'Thu', value: 4 },
    { label: 'Fri', value: 5 },
    { label: 'Sat', value: 6 },
  ];

  const handleToggleDay = (day: number) => {
    const current = formData.specificDaysOfWeek || [];
    const exists = current.includes(day);
    const updated = exists ? current.filter((d) => d !== day) : [...current, day];
    updated.sort((a, b) => a - b);
    setFormData({ ...formData, specificDaysOfWeek: updated });
  };

  const handleMonthlyDaysChange = (rawValue: string) => {
    setMonthlyDaysInput(rawValue);
    const values = rawValue
      .split(',')
      .map((value) => parseInt(value.trim(), 10))
      .filter((value) => !Number.isNaN(value) && value >= 1 && value <= 31);
    const uniqueSorted = Array.from(new Set(values)).sort((a, b) => a - b);
    setFormData({ ...formData, specificDaysOfMonth: uniqueSorted });
  };

  const handleRecurrenceIntervalChange = (value: string) => {
    const parsed = parseInt(value, 10);
    if (Number.isNaN(parsed) || parsed <= 0) {
      setFormData({ ...formData, recurrenceInterval: undefined });
    } else {
      setFormData({ ...formData, recurrenceInterval: parsed });
    }
  };

  const handleOpenNew = () => {
    setEditingHabit(null);
    setFormData({
      name: '',
      description: '',
      shortDescription: '',
      categoryId: undefined,
      recurrenceType: RecurrenceEnum.Daily,
      recurrenceInterval: 1,
      specificDaysOfWeek: [],
      specificDaysOfMonth: [],
      timeOfDay: '',
      timeOfDayEnd: '',
      duration: undefined,
      isActive: true,
      color: '#6366F1',
      reminderEnabled: false,
      reminderTime: '',
      tags: [],
      icon: '',
      imageUrl: '',
    });
    setMonthlyDaysInput('');
    setOpenDialog(true);
  };

  const handleEdit = (habit: Habit) => {
    setEditingHabit(habit);
    setFormData(habit);
    setMonthlyDaysInput(habit.specificDaysOfMonth?.join(',') || '');
    setOpenDialog(true);
  };

  const handleClose = () => {
    setOpenDialog(false);
    setEditingHabit(null);
  };

  const handleSave = async () => {
    try {
      if (editingHabit) {
        // Update existing habit
        await updateHabitAsync({ id: editingHabit.id, updates: formData });
      } else {
        // Create new habit
        const newHabit: Habit = {
          id: 0, // Will be assigned by sync service
          name: formData.name || '',
          description: formData.description,
          shortDescription: formData.shortDescription,
          categoryId: formData.categoryId,
          recurrenceType: formData.recurrenceType || RecurrenceEnum.Daily,
          recurrenceInterval: formData.recurrenceInterval,
          specificDaysOfWeek: formData.specificDaysOfWeek?.length ? formData.specificDaysOfWeek : undefined,
          specificDaysOfMonth: formData.specificDaysOfMonth?.length ? formData.specificDaysOfMonth : undefined,
          timeOfDay: formData.timeOfDay,
          timeOfDayEnd: formData.timeOfDayEnd,
          duration: formData.duration,
          isActive: formData.isActive ?? true,
          color: formData.color || '#6366F1',
          icon: formData.icon,
          imageUrl: formData.imageUrl,
          reminderEnabled: formData.reminderEnabled ?? false,
          reminderTime: formData.reminderTime || undefined,
          tags: formData.tags || [],
          createdDate: new Date().toISOString(),
        };
        await createHabitAsync(newHabit);
      }
      handleClose();
    } catch (error) {
      console.error('Failed to save habit:', error);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteHabitAsync(id);
      setDeleteConfirm(null);
    } catch (error) {
      console.error('Failed to delete habit:', error);
    }
  };

  const handleDuplicate = async (habit: Habit) => {
    try {
      await duplicateHabitAsync(habit.id);
    } catch (error) {
      console.error('Failed to duplicate habit:', error);
    }
  };

  const handleManageExercises = (habit: Habit) => {
    setSelectedHabitForExercises(habit);
    setExerciseManagerOpen(true);
  };

  if (isLoading || isCreating || isUpdating || isDeleting || isDuplicating) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>
          {isLoading && 'Loading habits...'}
          {isCreating && 'Creating habit...'}
          {isUpdating && 'Updating habit...'}
          {isDeleting && 'Deleting habit...'}
          {isDuplicating && 'Duplicating habit...'}
        </Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" fontWeight="bold">
          My Habits
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleOpenNew}
        >
          Add Habit
        </Button>
      </Box>

      {/* Habits List */}
      <Grid container spacing={2}>
        {habits.length === 0 && (
          <Grid size={{ xs: 12 }}>
            <Alert severity="info">
              No habits yet. Click "Add Habit" to create your first habit!
            </Alert>
          </Grid>
        )}
        
        {habits.map((habit) => (
          <Grid size={{ xs: 12, md: 6, lg: 4 }} key={habit.id}>
            <Card
              sx={{
                borderLeft: `4px solid ${habit.color}`,
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
              }}
            >
              <CardContent sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="h6" component="h2">
                    {habit.name}
                  </Typography>
                  <Chip
                    label={habit.isActive ? 'Active' : 'Inactive'}
                    color={habit.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </Box>
                
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  {habit.description}
                </Typography>

                <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 1 }}>
                  {habit.timeOfDay && (
                    <Chip label={habit.timeOfDay} size="small" variant="outlined" />
                  )}
                  {habit.duration && (
                    <Chip label={`${habit.duration} min`} size="small" variant="outlined" />
                  )}
                  <Chip
                    label={RecurrenceEnum[habit.recurrenceType]}
                    size="small"
                    variant="outlined"
                  />
                </Box>

                {habit.tags && habit.tags.length > 0 && (
                  <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                    {habit.tags.map((tag, idx) => (
                      <Chip key={idx} label={tag} size="small" />
                    ))}
                  </Box>
                )}
              </CardContent>

              <CardActions>
                <IconButton size="small" onClick={() => handleEdit(habit)} title="Edit">
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => handleManageExercises(habit)} title="Manage Exercises">
                  <FitnessCenter fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => handleDuplicate(habit)} title="Duplicate">
                  <ContentCopy fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => setDeleteConfirm(habit.id)} title="Delete">
                  <Delete fontSize="small" />
                </IconButton>
                <IconButton size="small" title={habit.isActive ? 'Archive' : 'Unarchive'}>
                  {habit.isActive ? <Archive fontSize="small" /> : <Unarchive fontSize="small" />}
                </IconButton>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Add/Edit Dialog */}
      <Dialog open={openDialog} onClose={handleClose} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingHabit ? 'Edit Habit' : 'Add New Habit'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            <TextField
              label="Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              fullWidth
              required
            />

            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              fullWidth
              multiline
              rows={3}
            />

            <TextField
              label="Short Description"
              value={formData.shortDescription || ''}
              onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
              fullWidth
              helperText="Optional subtitle shown in overview cards"
            />

            <FormControl fullWidth>
              <InputLabel>Category</InputLabel>
              <Select
                value={formData.categoryId || ''}
                onChange={(e) => setFormData({ ...formData, categoryId: Number(e.target.value) })}
                label="Category"
              >
                {categories.map((cat) => (
                  <MenuItem key={cat.id} value={cat.id}>
                    {cat.icon} {cat.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel>Recurrence</InputLabel>
              <Select
                value={formData.recurrenceType}
                onChange={(e) => setFormData({ ...formData, recurrenceType: e.target.value as RecurrenceType })}
                label="Recurrence"
              >
                <MenuItem value={RecurrenceEnum.Daily}>Daily</MenuItem>
                <MenuItem value={RecurrenceEnum.Weekly}>Weekly</MenuItem>
                <MenuItem value={RecurrenceEnum.Monthly}>Monthly</MenuItem>
                <MenuItem value={RecurrenceEnum.SpecificDays}>Specific Days</MenuItem>
                <MenuItem value={RecurrenceEnum.Custom}>Custom</MenuItem>
              </Select>
            </FormControl>

            <TextField
              label="Repeat Every"
              type="number"
              value={formData.recurrenceInterval ?? ''}
              onChange={(e) => handleRecurrenceIntervalChange(e.target.value)}
              fullWidth
              helperText="Number of days/weeks/months between occurrences"
            />

            {(formData.recurrenceType === RecurrenceEnum.Weekly || formData.recurrenceType === RecurrenceEnum.SpecificDays) && (
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Days of Week
                </Typography>
                <FormGroup row>
                  {dayOptions.map((day) => (
                    <FormControlLabel
                      key={day.value}
                      control={
                        <Checkbox
                          checked={formData.specificDaysOfWeek?.includes(day.value) || false}
                          onChange={() => handleToggleDay(day.value)}
                        />
                      }
                      label={day.label}
                    />
                  ))}
                </FormGroup>
              </Box>
            )}

            {formData.recurrenceType === RecurrenceEnum.Monthly && (
              <TextField
                label="Days of Month"
                value={monthlyDaysInput}
                onChange={(e) => handleMonthlyDaysChange(e.target.value)}
                fullWidth
                helperText="Comma-separated values (e.g., 1, 15, 30)"
              />
            )}

            <Grid container spacing={2}>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="Time of Day"
                  type="time"
                  value={formData.timeOfDay || ''}
                  onChange={(e) => setFormData({ ...formData, timeOfDay: e.target.value })}
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="End Time"
                  type="time"
                  value={formData.timeOfDayEnd || ''}
                  onChange={(e) => setFormData({ ...formData, timeOfDayEnd: e.target.value })}
                  fullWidth
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
            </Grid>

            <Grid container spacing={2}>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="Duration (minutes)"
                  type="number"
                  value={formData.duration ?? ''}
                  onChange={(e) => {
                    const raw = e.target.value;
                    setFormData({
                      ...formData,
                      duration: raw === '' ? undefined : Number(raw),
                    });
                  }}
                  fullWidth
                />
              </Grid>
              <Grid size={{ xs: 6 }}>
                <TextField
                  label="Icon"
                  value={formData.icon || ''}
                  onChange={(e) => setFormData({ ...formData, icon: e.target.value })}
                  fullWidth
                  helperText="Emoji or short code, e.g., 🏃‍♀️"
                />
              </Grid>
            </Grid>

            <TextField
              label="Color"
              type="color"
              value={formData.color}
              onChange={(e) => setFormData({ ...formData, color: e.target.value })}
              fullWidth
            />

            <TextField
              label="Image URL"
              value={formData.imageUrl || ''}
              onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
              fullWidth
              helperText="Optional cover image"
            />

            <TextField
              label="Tags (comma separated)"
              value={formData.tags?.join(', ') || ''}
              onChange={(e) => setFormData({
                ...formData,
                tags: e.target.value.split(',').map(t => t.trim()).filter(t => t)
              })}
              fullWidth
              helperText="e.g., morning, fitness, health"
            />

            <FormControlLabel
              control={
                <Switch
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                />
              }
              label="Active"
            />

            <FormControlLabel
              control={
                <Switch
                  checked={formData.reminderEnabled}
                  onChange={(e) => setFormData({ ...formData, reminderEnabled: e.target.checked })}
                />
              }
              label="Enable Reminder"
            />

            {formData.reminderEnabled && (
              <TextField
                label="Reminder Time"
                type="time"
                value={formData.reminderTime || ''}
                onChange={(e) => setFormData({ ...formData, reminderTime: e.target.value })}
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
            )}

            {/* Custom Metrics Section */}
            <Divider sx={{ my: 3 }} />
            <Typography variant="h6" gutterBottom>
              Custom Metrics
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Define custom trackable fields for this habit (e.g., weight lifted, distance, mood, etc.)
            </Typography>
            <CustomMetricsManager
              habitId={formData.id || Date.now()}
              metrics={(formData as any).metricDefinitions || []}
              onSave={(updatedMetrics: HabitMetricDefinition[]) => {
                setFormData({
                  ...formData,
                  metricDefinitions: updatedMetrics
                } as any);
              }}
            />

            {/* Routine Templates Section */}
            <Divider sx={{ my: 3 }} />
            <Typography variant="h6" gutterBottom>
              Routine Templates
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Create multi-step workflows for complex habits (e.g., workout routines, morning rituals, etc.)
            </Typography>
            <RoutineTemplateManager
              habitId={formData.id || Date.now()}
              templates={(formData as any).routineTemplates || []}
              onSave={(updatedTemplates: RoutineTemplate[]) => {
                setFormData({
                  ...formData,
                  routineTemplates: updatedTemplates
                } as any);
              }}
            />

            {/* Reminders Section */}
            {editingHabit && (
              <>
                <Divider sx={{ my: 3 }} />
                <Typography variant="h6" gutterBottom>
                  Reminders & Notifications
                </Typography>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Set up reminders to help you stay on track with this habit
                </Typography>
                <ReminderManager
                  habitId={editingHabit.id}
                  habitName={editingHabit.name}
                />
              </>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">
            {editingHabit ? 'Save Changes' : 'Create Habit'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirm !== null} onClose={() => setDeleteConfirm(null)}>
        <DialogTitle>Delete Habit?</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this habit? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button
            onClick={() => deleteConfirm && handleDelete(deleteConfirm)}
            color="error"
            variant="contained"
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      {/* Exercise Manager Dialog */}
      <Dialog 
        open={exerciseManagerOpen} 
        onClose={() => setExerciseManagerOpen(false)}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>Manage Exercises</DialogTitle>
        <DialogContent>
          {selectedHabitForExercises && (
            <ExerciseManager
              habitId={selectedHabitForExercises.id}
              habitName={selectedHabitForExercises.name}
              onClose={() => setExerciseManagerOpen(false)}
            />
          )}
        </DialogContent>
      </Dialog>
    </Container>
  );
}
