import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Rating,
  Grid,
  Divider,
} from '@mui/material';
import {
  TrendingUp as TrendIcon,
  Check as CheckIcon,
  Close as CloseIcon,
} from '@mui/icons-material';
import { format } from 'date-fns';
import { HabitMetricDefinition, CustomMetricValue, MetricType } from '../types/habit.types';

interface MetricValuesDisplayProps {
  metrics: HabitMetricDefinition[];
  values: CustomMetricValue[];
  compact?: boolean;
}

const MetricValuesDisplay: React.FC<MetricValuesDisplayProps> = ({
  metrics,
  values,
  compact = false,
}) => {
  // Get metric definition by ID
  const getMetricDefinition = (metricId: number): HabitMetricDefinition | undefined => {
    return metrics.find((m) => m.id === metricId);
  };

  // Format metric value for display
  const formatValue = (value: CustomMetricValue, metric: HabitMetricDefinition): string | React.ReactElement => {
    switch (metric.type) {
      case MetricType.Number:
      case MetricType.Distance:
      case MetricType.Weight:
      case MetricType.Reps:
      case MetricType.Sets:
        return value.numericValue !== undefined
          ? `${value.numericValue}${metric.unit ? ` ${metric.unit}` : ''}`
          : '-';

      case MetricType.Text:
      case MetricType.Select:
        return value.textValue || '-';

      case MetricType.Boolean:
        return value.booleanValue ? (
          <Chip icon={<CheckIcon />} label="Yes" size="small" color="success" />
        ) : (
          <Chip icon={<CloseIcon />} label="No" size="small" />
        );

      case MetricType.Rating:
        return value.numericValue !== undefined ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Rating value={value.numericValue} readOnly size="small" />
            <Typography variant="caption" color="text.secondary">
              ({value.numericValue}/5)
            </Typography>
          </Box>
        ) : (
          '-'
        );

      case MetricType.Time:
        return value.textValue || '-';

      default:
        return '-';
    }
  };

  // Calculate statistics for numeric metrics
  const getMetricStats = (metricId: number) => {
    const metricValues = values.filter((v) => v.metricDefinitionId === metricId);
    const numericValues = metricValues
      .map((v) => v.numericValue)
      .filter((v): v is number => v !== undefined);

    if (numericValues.length === 0) return null;

    const avg = numericValues.reduce((sum, val) => sum + val, 0) / numericValues.length;
    const min = Math.min(...numericValues);
    const max = Math.max(...numericValues);
    const latest = numericValues[numericValues.length - 1];
    const previous = numericValues.length > 1 ? numericValues[numericValues.length - 2] : null;
    const trend = previous !== null ? latest - previous : 0;

    return { avg, min, max, latest, trend };
  };

  // Group values by date
  const valuesByDate = values.reduce((acc, value) => {
    if (!value.timestamp) return acc;
    const date = format(new Date(value.timestamp), 'yyyy-MM-dd');
    if (!acc[date]) acc[date] = [];
    acc[date].push(value);
    return acc;
  }, {} as Record<string, CustomMetricValue[]>);

  const dates = Object.keys(valuesByDate).sort().reverse();

  if (values.length === 0) {
    return (
      <Card variant="outlined">
        <CardContent>
          <Typography variant="body2" color="text.secondary" textAlign="center">
            No metric values recorded yet
          </Typography>
        </CardContent>
      </Card>
    );
  }

  // Compact view - show only latest values
  if (compact) {
    return (
      <Box>
        {metrics.map((metric) => {
          const latestValue = values
            .filter((v) => v.metricDefinitionId === metric.id && v.timestamp)
            .sort((a, b) => new Date(b.timestamp!).getTime() - new Date(a.timestamp!).getTime())[0];

          if (!latestValue) return null;

          const stats = getMetricStats(metric.id);

          return (
            <Box key={metric.id} sx={{ mb: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
                <Typography variant="body2" fontWeight={500}>
                  {metric.name}
                </Typography>
                {stats && stats.trend !== 0 && (
                  <Chip
                    icon={<TrendIcon sx={{ transform: stats.trend > 0 ? 'rotate(0deg)' : 'rotate(180deg)' }} />}
                    label={`${stats.trend > 0 ? '+' : ''}${stats.trend.toFixed(1)}`}
                    size="small"
                    color={stats.trend > 0 ? 'success' : 'error'}
                    variant="outlined"
                  />
                )}
              </Box>
              <Typography variant="h6">{formatValue(latestValue, metric)}</Typography>
              {stats && (
                <Typography variant="caption" color="text.secondary">
                  Avg: {stats.avg.toFixed(1)} | Min: {stats.min} | Max: {stats.max}
                </Typography>
              )}
            </Box>
          );
        })}
      </Box>
    );
  }

  // Full view - show all values by date
  return (
    <Box>
      {dates.map((date) => {
        const dateValues = valuesByDate[date];
        const displayDate = format(new Date(date), 'EEEE, MMM d, yyyy');

        return (
          <Card key={date} variant="outlined" sx={{ mb: 2 }}>
            <CardContent>
              <Typography variant="subtitle2" color="primary" gutterBottom>
                {displayDate}
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                {dateValues.map((value) => {
                  const metric = getMetricDefinition(value.metricDefinitionId);
                  if (!metric) return null;

                  return (
                    <Grid key={value.id} size={{ xs: 12, sm: 6, md: 4 }}>
                      <Box>
                        <Typography variant="caption" color="text.secondary">
                          {metric.name}
                        </Typography>
                        <Typography variant="body1" fontWeight={500}>
                          {formatValue(value, metric)}
                        </Typography>
                      </Box>
                    </Grid>
                  );
                })}
              </Grid>
            </CardContent>
          </Card>
        );
      })}

      {/* Summary Statistics */}
      <Card variant="outlined" sx={{ bgcolor: 'action.hover' }}>
        <CardContent>
          <Typography variant="subtitle2" gutterBottom>
            📊 Summary Statistics
          </Typography>
          <Divider sx={{ my: 1 }} />
          <Grid container spacing={2}>
            {metrics.map((metric) => {
              const stats = getMetricStats(metric.id);
              if (!stats) return null;

              return (
                <Grid key={metric.id} size={{ xs: 12, sm: 6 }}>
                  <Typography variant="caption" color="text.secondary" display="block">
                    {metric.name}
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 2, mt: 0.5 }}>
                    <Chip label={`Avg: ${stats.avg.toFixed(1)}`} size="small" />
                    <Chip label={`Min: ${stats.min}`} size="small" />
                    <Chip label={`Max: ${stats.max}`} size="small" />
                  </Box>
                </Grid>
              );
            })}
          </Grid>
        </CardContent>
      </Card>
    </Box>
  );
};

export default MetricValuesDisplay;
