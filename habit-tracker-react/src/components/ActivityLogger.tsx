import { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Rating,
  Chip,
  IconButton,
  Divider,
  Avatar,
  Stack,
} from '@mui/material';
import {
  Note,
  Mood,
  BatteryChargingFull,
  Check,
  Close,
  ShowChart,
} from '@mui/icons-material';
import type { DailyHabitEntry, Habit, CustomMetricValue } from '../types/habit.types';
import { format } from 'date-fns';
import { syncService } from '../services/syncService';
import MetricInput from './MetricInput';

interface ActivityLoggerProps {
  habit: Habit;
  entry: DailyHabitEntry;
  onSave: (data: Partial<DailyHabitEntry>) => void;
  onClose: () => void;
}

export default function ActivityLogger({ habit, entry, onSave, onClose }: ActivityLoggerProps) {
  const [notes, setNotes] = useState(entry?.notes || '');
  const [rating, setRating] = useState<number | null>(entry.rating || null);
  const [mood, setMood] = useState<number | null>(entry.mood || null);
  const [energyLevel, setEnergyLevel] = useState<number | null>(entry.energyLevel || null);
  const [customMetrics, setCustomMetrics] = useState<Record<number, CustomMetricValue>>({});
  const [metricDefinitions, setMetricDefinitions] = useState<any[]>([]);

  // Load metric definitions for this habit
  useEffect(() => {
    const loadMetricDefinitions = async () => {
      if (habit.id) {
        const metrics = await syncService.getMetricDefinitions(habit.id);
        setMetricDefinitions(metrics);
      }
    };
    loadMetricDefinitions();
  }, [habit.id]);

  // Load existing metric values if editing an entry
  useEffect(() => {
    if (entry?.customMetrics) {
      const metricsMap: Record<number, CustomMetricValue> = {};
      entry.customMetrics.forEach((metric: CustomMetricValue) => {
        if (metric.metricDefinitionId) {
          metricsMap[metric.metricDefinitionId] = metric;
        }
      });
      setCustomMetrics(metricsMap);
    }
  }, [entry]);

  const handleMetricChange = (metricDefinitionId: number, value: CustomMetricValue) => {
    setCustomMetrics(prev => ({
      ...prev,
      [metricDefinitionId]: value,
    }));
  };

  const handleSave = () => {
    const customMetricsArray = Object.values(customMetrics).filter(m => 
      m.numericValue !== undefined || m.textValue !== undefined || m.booleanValue !== undefined
    );
    
    onSave({
      notes,
      rating: rating || undefined,
      mood: mood || undefined,
      energyLevel: energyLevel || undefined,
      customMetrics: customMetricsArray.length > 0 ? customMetricsArray : undefined,
    });
  };

  const moodLabels = ['😢 Awful', '😕 Bad', '😐 Okay', '😊 Good', '😄 Great'];
  const energyLabels = ['⚡ Very Low', '🔋 Low', '🔌 Medium', '⚡ High', '⚡⚡ Very High'];

  return (
    <Card sx={{ maxWidth: 600, margin: 'auto' }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Avatar sx={{ bgcolor: habit.color || 'primary.main', width: 48, height: 48 }}>
              {habit.icon || habit.name.charAt(0)}
            </Avatar>
            <Box>
              <Typography variant="h6">{habit.name}</Typography>
              <Typography variant="caption" color="text.secondary">
                {format(new Date(entry.date), 'EEEE, MMM d, yyyy')}
              </Typography>
            </Box>
          </Box>
          <IconButton onClick={onClose} size="small">
            <Close />
          </IconButton>
        </Box>

        <Divider sx={{ mb: 3 }} />

        {/* Notes */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <Note fontSize="small" color="action" />
            <Typography variant="subtitle2" color="text.secondary">
              Notes
            </Typography>
          </Box>
          <TextField
            fullWidth
            multiline
            rows={3}
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="Add notes about this habit..."
            variant="outlined"
            size="small"
          />
        </Box>

        {/* Rating */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <Typography variant="subtitle2" color="text.secondary">
              Rating
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Rating
              value={rating}
              onChange={(_, value) => setRating(value)}
              size="large"
            />
            {rating && <Typography variant="body2" color="text.secondary">{rating} / 5 stars</Typography>}
          </Box>
        </Box>

        {/* Mood */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <Mood fontSize="small" color="action" />
            <Typography variant="subtitle2" color="text.secondary">
              How did you feel?
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            {moodLabels.map((label, index) => (
              <Chip
                key={index}
                label={label}
                onClick={() => setMood(index + 1)}
                color={mood === index + 1 ? 'primary' : 'default'}
                variant={mood === index + 1 ? 'filled' : 'outlined'}
              />
            ))}
          </Stack>
        </Box>

        {/* Energy Level */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
            <BatteryChargingFull fontSize="small" color="action" />
            <Typography variant="subtitle2" color="text.secondary">
              Energy Level
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            {energyLabels.map((label, index) => (
              <Chip
                key={index}
                label={label}
                onClick={() => setEnergyLevel(index + 1)}
                color={energyLevel === index + 1 ? 'primary' : 'default'}
                variant={energyLevel === index + 1 ? 'filled' : 'outlined'}
              />
            ))}
          </Stack>
        </Box>

        {/* Custom Metrics */}
        {metricDefinitions.length > 0 && (
          <Box sx={{ mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <ShowChart fontSize="small" color="action" />
              <Typography variant="subtitle2" color="text.secondary">
                Custom Metrics
              </Typography>
            </Box>
            <Stack spacing={2}>
              {metricDefinitions.map((metricDef) => (
                <MetricInput
                  key={metricDef.id}
                  metric={metricDef}
                  value={customMetrics[metricDef.id!]}
                  onChange={(value) => handleMetricChange(metricDef.id!, value)}
                />
              ))}
            </Stack>
          </Box>
        )}

        <Divider sx={{ mb: 2 }} />

        {/* Action Buttons */}
        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="contained"
            startIcon={<Check />}
            onClick={handleSave}
          >
            Save Activity Log
          </Button>
        </Box>

        {/* Summary */}
        {(rating || mood || energyLevel || notes) && (
          <>
            <Divider sx={{ my: 2 }} />
            <Box>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
                Summary
              </Typography>
              <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                {rating && <Chip size="small" label={`⭐ ${rating}/5`} />}
                {mood && <Chip size="small" label={moodLabels[mood - 1]} />}
                {energyLevel && <Chip size="small" label={energyLabels[energyLevel - 1]} />}
                {notes && <Chip size="small" label={`📝 ${notes.length} characters`} />}
                {Object.keys(customMetrics).length > 0 && (
                  <Chip size="small" label={`📊 ${Object.keys(customMetrics).length} metrics`} />
                )}
              </Stack>
            </Box>
          </>
        )}
      </CardContent>
    </Card>
  );
}
