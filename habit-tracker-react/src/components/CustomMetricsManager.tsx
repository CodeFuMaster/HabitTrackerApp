import { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  IconButton,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Switch,
  FormControlLabel,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Stack,
} from '@mui/material';
import {
  Add,
  Edit,
  Delete,
  DragIndicator,
} from '@mui/icons-material';
import { useNotification } from '../contexts/NotificationContext';
import type { HabitMetricDefinition, MetricType } from '../types/habit.types';

interface CustomMetricsManagerProps {
  habitId: number;
  metrics: HabitMetricDefinition[];
  onSave: (metrics: HabitMetricDefinition[]) => void;
}

export default function CustomMetricsManager({ habitId, metrics, onSave }: CustomMetricsManagerProps) {
  const { showError } = useNotification();
  const [openDialog, setOpenDialog] = useState(false);
  const [editingMetric, setEditingMetric] = useState<HabitMetricDefinition | null>(null);
  const [_confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [formData, setFormData] = useState<Partial<HabitMetricDefinition>>({
    name: '',
    type: 'number' as MetricType,
    unit: '',
    isRequired: false,
    order: metrics.length,
  });

  const metricTypes = [
    { value: 'number', label: 'Number', example: '150' },
    { value: 'text', label: 'Text', example: 'Great workout!' },
    { value: 'boolean', label: 'Yes/No', example: 'Yes' },
    { value: 'rating', label: 'Rating (1-5)', example: '⭐⭐⭐⭐' },
    { value: 'time', label: 'Duration', example: '30:00' },
    { value: 'distance', label: 'Distance', example: '5.2 km' },
    { value: 'weight', label: 'Weight', example: '65 kg' },
    { value: 'reps', label: 'Repetitions', example: '12 reps' },
    { value: 'sets', label: 'Sets', example: '3 sets' },
  ];

  const handleOpenNew = () => {
    setEditingMetric(null);
    setFormData({
      name: '',
      type: 'number' as MetricType,
      unit: '',
      isRequired: false,
      order: metrics.length,
    });
    setOpenDialog(true);
  };

  const handleEdit = (metric: HabitMetricDefinition) => {
    setEditingMetric(metric);
    setFormData({
      name: metric.name,
      type: metric.type,
      unit: metric.unit,
      isRequired: metric.isRequired,
      order: metric.order,
    });
    setOpenDialog(true);
  };

  const handleClose = () => {
    setOpenDialog(false);
    setEditingMetric(null);
  };

  const handleSave = () => {
    if (!formData.name || !formData.type) {
      showError('Please fill in required fields');
      return;
    }

    const newMetric: HabitMetricDefinition = {
      id: editingMetric?.id || Date.now(),
      habitId,
      name: formData.name,
      type: formData.type as MetricType,
      unit: formData.unit,
      isRequired: formData.isRequired || false,
      order: formData.order || metrics.length,
    };

    let updatedMetrics: HabitMetricDefinition[];
    if (editingMetric) {
      updatedMetrics = metrics.map(m => m.id === editingMetric.id ? newMetric : m);
    } else {
      updatedMetrics = [...metrics, newMetric];
    }

    onSave(updatedMetrics);
    handleClose();
  };

  const handleDelete = (id: number) => {
    const updatedMetrics = metrics.filter(m => m.id !== id);
    onSave(updatedMetrics);
    setConfirmDeleteId(null);
  };

  const getMetricTypeIcon = (type: MetricType): string => {
    const icons: Record<MetricType, string> = {
      number: '🔢',
      text: '📝',
      boolean: '✅',
      select: '📋',
      rating: '⭐',
      time: '⏱️',
      distance: '📏',
      weight: '⚖️',
      reps: '💪',
      sets: '🏋️',
    };
    return icons[type] || '📊';
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">Custom Metrics</Typography>
        <Button
          startIcon={<Add />}
          variant="outlined"
          size="small"
          onClick={handleOpenNew}
        >
          Add Metric
        </Button>
      </Box>

      {metrics.length === 0 ? (
        <Card variant="outlined">
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              No custom metrics defined
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Add metrics to track specific data like weight, reps, distance, etc.
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <List>
          {metrics.map((metric) => (
            <ListItem
              key={metric.id}
              sx={{
                border: 1,
                borderColor: 'divider',
                borderRadius: 1,
                mb: 1,
              }}
            >
              <IconButton size="small" sx={{ cursor: 'grab', mr: 1 }}>
                <DragIndicator />
              </IconButton>
              
              <ListItemText
                primary={
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography>{getMetricTypeIcon(metric.type)}</Typography>
                    <Typography>{metric.name}</Typography>
                    {metric.isRequired && (
                      <Chip label="Required" size="small" color="primary" />
                    )}
                  </Box>
                }
                secondary={
                  <Stack direction="row" spacing={1} sx={{ mt: 0.5 }}>
                    <Chip
                      label={metricTypes.find(t => t.value === metric.type)?.label || metric.type}
                      size="small"
                      variant="outlined"
                    />
                    {metric.unit && (
                      <Chip label={`Unit: ${metric.unit}`} size="small" variant="outlined" />
                    )}
                  </Stack>
                }
              />
              
              <ListItemSecondaryAction>
                <IconButton size="small" onClick={() => handleEdit(metric)}>
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => handleDelete(metric.id)} color="error">
                  <Delete fontSize="small" />
                </IconButton>
              </ListItemSecondaryAction>
            </ListItem>
          ))}
        </List>
      )}

      {/* Add/Edit Dialog */}
      <Dialog open={openDialog} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingMetric ? 'Edit Metric' : 'Add Custom Metric'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            <TextField
              label="Metric Name"
              required
              fullWidth
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              placeholder="e.g., Weight Lifted, Distance Run, Mood Rating"
            />

            <FormControl fullWidth required>
              <InputLabel>Type</InputLabel>
              <Select
                value={formData.type}
                label="Type"
                onChange={(e) => setFormData({ ...formData, type: e.target.value as MetricType })}
              >
                {metricTypes.map((type) => (
                  <MenuItem key={type.value} value={type.value}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography>{getMetricTypeIcon(type.value as MetricType)}</Typography>
                      <Box>
                        <Typography>{type.label}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          Example: {type.example}
                        </Typography>
                      </Box>
                    </Box>
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {['number', 'distance', 'weight', 'reps', 'sets'].includes(formData.type || '') && (
              <TextField
                label="Unit (optional)"
                fullWidth
                value={formData.unit}
                onChange={(e) => setFormData({ ...formData, unit: e.target.value })}
                placeholder="e.g., kg, lbs, km, miles"
              />
            )}

            <FormControlLabel
              control={
                <Switch
                  checked={formData.isRequired || false}
                  onChange={(e) => setFormData({ ...formData, isRequired: e.target.checked })}
                />
              }
              label="Required field"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">
            {editingMetric ? 'Save Changes' : 'Add Metric'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
