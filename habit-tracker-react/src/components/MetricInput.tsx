import React, { useState } from 'react';
import {
  Box,
  TextField,
  Switch,
  FormControlLabel,
  Rating,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  Typography,
  InputAdornment,
  Chip,
} from '@mui/material';
import {
  Numbers as NumbersIcon,
  TextFields as TextIcon,
  CheckBox as BooleanIcon,
  Star as RatingIcon,
  Timer as TimeIcon,
  Straighten as DistanceIcon,
  Scale as WeightIcon,
  FitnessCenter as RepsIcon,
} from '@mui/icons-material';
import { HabitMetricDefinition, CustomMetricValue, MetricType } from '../types/habit.types';

interface MetricInputProps {
  metric: HabitMetricDefinition;
  value?: CustomMetricValue;
  onChange: (value: CustomMetricValue) => void;
}

const MetricInput: React.FC<MetricInputProps> = ({ metric, value, onChange }) => {
  // Local state for input values
  const [numericValue, setNumericValue] = useState<number | undefined>(value?.numericValue);
  const [textValue, setTextValue] = useState<string>(value?.textValue || '');
  const [booleanValue, setBooleanValue] = useState<boolean>(value?.booleanValue || false);

  // Get icon for metric type
  const getIcon = (type: MetricType) => {
    const icons: Record<MetricType, React.ReactElement> = {
      [MetricType.Number]: <NumbersIcon fontSize="small" />,
      [MetricType.Text]: <TextIcon fontSize="small" />,
      [MetricType.Boolean]: <BooleanIcon fontSize="small" />,
      [MetricType.Rating]: <RatingIcon fontSize="small" />,
      [MetricType.Time]: <TimeIcon fontSize="small" />,
      [MetricType.Distance]: <DistanceIcon fontSize="small" />,
      [MetricType.Weight]: <WeightIcon fontSize="small" />,
      [MetricType.Reps]: <RepsIcon fontSize="small" />,
      [MetricType.Sets]: <RepsIcon fontSize="small" />,
      [MetricType.Select]: <TextIcon fontSize="small" />,
    };
    return icons[type];
  };

  // Handle numeric input change
  const handleNumericChange = (newValue: number | undefined) => {
    setNumericValue(newValue);
    onChange({
      id: value?.id || Date.now(),
      entryId: value?.entryId || 0,
      metricDefinitionId: metric.id,
      numericValue: newValue,
      textValue: undefined,
      booleanValue: undefined,
      timestamp: new Date().toISOString(),
    });
  };

  // Handle text input change
  const handleTextChange = (newValue: string) => {
    setTextValue(newValue);
    onChange({
      id: value?.id || Date.now(),
      entryId: value?.entryId || 0,
      metricDefinitionId: metric.id,
      numericValue: undefined,
      textValue: newValue,
      booleanValue: undefined,
      timestamp: new Date().toISOString(),
    });
  };

  // Handle boolean input change
  const handleBooleanChange = (newValue: boolean) => {
    setBooleanValue(newValue);
    onChange({
      id: value?.id || Date.now(),
      entryId: value?.entryId || 0,
      metricDefinitionId: metric.id,
      numericValue: undefined,
      textValue: undefined,
      booleanValue: newValue,
      timestamp: new Date().toISOString(),
    });
  };

  // Render input based on metric type
  const renderInput = () => {
    switch (metric.type) {
      case MetricType.Number:
      case MetricType.Distance:
      case MetricType.Weight:
      case MetricType.Reps:
      case MetricType.Sets:
        return (
          <TextField
            type="number"
            value={numericValue ?? ''}
            onChange={(e) => handleNumericChange(e.target.value ? parseFloat(e.target.value) : undefined)}
            fullWidth
            size="small"
            required={metric.isRequired}
            placeholder={`Enter ${metric.name.toLowerCase()}`}
            InputProps={{
              endAdornment: metric.unit ? (
                <InputAdornment position="end">
                  <Typography variant="body2" color="text.secondary">
                    {metric.unit}
                  </Typography>
                </InputAdornment>
              ) : undefined,
            }}
          />
        );

      case MetricType.Text:
        return (
          <TextField
            value={textValue}
            onChange={(e) => handleTextChange(e.target.value)}
            fullWidth
            multiline
            rows={2}
            size="small"
            required={metric.isRequired}
            placeholder={`Enter ${metric.name.toLowerCase()}`}
          />
        );

      case MetricType.Boolean:
        return (
          <FormControlLabel
            control={
              <Switch
                checked={booleanValue}
                onChange={(e) => handleBooleanChange(e.target.checked)}
                color="primary"
              />
            }
            label={booleanValue ? 'Yes' : 'No'}
          />
        );

      case MetricType.Rating:
        return (
          <Box>
            <Rating
              value={numericValue ?? 0}
              onChange={(_, newValue) => handleNumericChange(newValue ?? undefined)}
              size="large"
            />
            {numericValue !== undefined && (
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {numericValue} / 5 stars
              </Typography>
            )}
          </Box>
        );

      case MetricType.Time:
        return (
          <TextField
            type="time"
            value={textValue}
            onChange={(e) => handleTextChange(e.target.value)}
            fullWidth
            size="small"
            required={metric.isRequired}
            InputLabelProps={{
              shrink: true,
            }}
          />
        );

      case MetricType.Select:
        return (
          <FormControl fullWidth size="small" required={metric.isRequired}>
            <InputLabel>{metric.name}</InputLabel>
            <Select
              value={textValue}
              onChange={(e) => handleTextChange(e.target.value)}
              label={metric.name}
            >
              {metric.options?.map((option) => (
                <MenuItem key={option} value={option}>
                  {option}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );

      default:
        return null;
    }
  };

  return (
    <Box sx={{ mb: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        {getIcon(metric.type)}
        <Typography variant="body2" fontWeight={500}>
          {metric.name}
          {metric.isRequired && (
            <Typography component="span" color="error" sx={{ ml: 0.5 }}>
              *
            </Typography>
          )}
        </Typography>
        <Chip
          label={metric.type}
          size="small"
          variant="outlined"
          sx={{ ml: 'auto', textTransform: 'capitalize' }}
        />
      </Box>
      {renderInput()}
    </Box>
  );
};

export default MetricInput;
