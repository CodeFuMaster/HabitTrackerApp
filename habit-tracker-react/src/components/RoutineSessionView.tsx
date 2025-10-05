import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Button,
  LinearProgress,
  Chip,
  IconButton,
  Dialog,
  DialogContent,
  Divider,
  Alert,
} from '@mui/material';
import {
  CheckCircle as CompleteIcon,
  Timer as TimerIcon,
  Close as CloseIcon,
  SkipNext as SkipIcon,
  Refresh as RestartIcon,
} from '@mui/icons-material';
import { Habit, RoutineTemplate } from '../types/habit.types';
import HabitTimer from './HabitTimer';
import MetricInput from './MetricInput';

interface RoutineSessionViewProps {
  habit: Habit;
  template: RoutineTemplate;
  onComplete: (sessionData: {
    completedSteps: number[];
    totalDuration: number;
    metrics: Record<number, any>;
  }) => void;
  onClose: () => void;
}

const RoutineSessionView: React.FC<RoutineSessionViewProps> = ({
  habit,
  template,
  onComplete,
  onClose,
}) => {
  const [activeStep, setActiveStep] = useState(0);
  const [completedSteps, setCompletedSteps] = useState<number[]>([]);
  const [stepStartTime, setStepStartTime] = useState<Date>(new Date());
  const [totalDuration, setTotalDuration] = useState(0);
  const [timerOpen, setTimerOpen] = useState(false);
  const [currentStepMetrics, setCurrentStepMetrics] = useState<Record<number, any>>({});
  const [allMetrics, setAllMetrics] = useState<Record<number, any>>({});

  const sortedSteps = [...template.steps].sort((a, b) => a.order - b.order);
  const currentStep = sortedSteps[activeStep];

  // Calculate progress
  const progress = (completedSteps.length / sortedSteps.length) * 100;
  const estimatedTimeRemaining = sortedSteps
    .slice(activeStep + 1)
    .reduce((sum, step) => sum + (step.duration || 0), 0);

  // Handle step completion
  const handleCompleteStep = () => {
    const stepDuration = Math.floor((new Date().getTime() - stepStartTime.getTime()) / 1000);
    setTotalDuration((prev) => prev + stepDuration);
    setCompletedSteps((prev) => [...prev, currentStep.id]);
    setAllMetrics((prev) => ({ ...prev, ...currentStepMetrics }));
    setCurrentStepMetrics({});

    if (activeStep < sortedSteps.length - 1) {
      setActiveStep((prev) => prev + 1);
      setStepStartTime(new Date());
    } else {
      // Routine complete
      handleCompleteRoutine(stepDuration);
    }
  };

  // Handle skip step
  const handleSkipStep = () => {
    if (currentStep.isOptional) {
      if (activeStep < sortedSteps.length - 1) {
        setActiveStep((prev) => prev + 1);
        setStepStartTime(new Date());
      }
    }
  };

  // Handle routine completion
  const handleCompleteRoutine = (lastStepDuration: number) => {
    const finalDuration = totalDuration + lastStepDuration;
    onComplete({
      completedSteps: [...completedSteps, currentStep.id],
      totalDuration: finalDuration,
      metrics: { ...allMetrics, ...currentStepMetrics },
    });
  };

  // Handle restart
  const handleRestart = () => {
    setActiveStep(0);
    setCompletedSteps([]);
    setStepStartTime(new Date());
    setTotalDuration(0);
    setCurrentStepMetrics({});
    setAllMetrics({});
  };

  // Format time (seconds to MM:SS)
  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  // Format estimated time
  const formatEstimatedTime = (minutes: number): string => {
    if (minutes < 60) return `${minutes} min`;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  };

  return (
    <Card sx={{ maxWidth: 800, mx: 'auto' }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
          <Box>
            <Typography variant="h6" gutterBottom>
              {template.name}
            </Typography>
            {template.description && (
              <Typography variant="body2" color="text.secondary">
                {template.description}
              </Typography>
            )}
          </Box>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>

        {/* Progress Bar */}
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Progress: {completedSteps.length} / {sortedSteps.length} steps
            </Typography>
            {estimatedTimeRemaining > 0 && (
              <Typography variant="body2" color="text.secondary">
                ~{formatEstimatedTime(estimatedTimeRemaining)} remaining
              </Typography>
            )}
          </Box>
          <LinearProgress
            variant="determinate"
            value={progress}
            sx={{ height: 8, borderRadius: 1 }}
          />
        </Box>

        {/* Stepper */}
        <Stepper activeStep={activeStep} orientation="vertical">
          {sortedSteps.map((step, index) => {
            const isCompleted = completedSteps.includes(step.id);
            const isCurrent = index === activeStep;

            return (
              <Step key={step.id} completed={isCompleted}>
                <StepLabel
                  optional={
                    step.isOptional ? (
                      <Typography variant="caption">Optional</Typography>
                    ) : null
                  }
                  StepIconProps={{
                    icon: isCompleted ? <CompleteIcon color="success" /> : undefined,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography fontWeight={isCurrent ? 600 : 400}>
                      {step.name}
                    </Typography>
                    {step.duration && (
                      <Chip
                        icon={<TimerIcon />}
                        label={`${step.duration} min`}
                        size="small"
                        variant="outlined"
                      />
                    )}
                  </Box>
                </StepLabel>
                <StepContent>
                  {step.description && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                      {step.description}
                    </Typography>
                  )}

                  {/* Step Metrics */}
                  {step.metrics && step.metrics.length > 0 && (
                    <Box sx={{ mb: 2, p: 2, bgcolor: 'background.default', borderRadius: 1 }}>
                      <Typography variant="subtitle2" gutterBottom>
                        Track Metrics:
                      </Typography>
                      {step.metrics.map((metric) => (
                        <MetricInput
                          key={metric.id}
                          metric={metric}
                          value={currentStepMetrics[metric.id]}
                          onChange={(value) => {
                            setCurrentStepMetrics((prev) => ({
                              ...prev,
                              [metric.id]: value,
                            }));
                          }}
                        />
                      ))}
                    </Box>
                  )}

                  {/* Step Actions */}
                  <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                    {step.duration && (
                      <Button
                        variant="outlined"
                        startIcon={<TimerIcon />}
                        onClick={() => setTimerOpen(true)}
                        size="small"
                      >
                        Start Timer
                      </Button>
                    )}
                    <Button
                      variant="contained"
                      startIcon={<CompleteIcon />}
                      onClick={handleCompleteStep}
                      color="primary"
                    >
                      {index === sortedSteps.length - 1 ? 'Complete Routine' : 'Complete Step'}
                    </Button>
                    {step.isOptional && (
                      <Button
                        variant="outlined"
                        startIcon={<SkipIcon />}
                        onClick={handleSkipStep}
                        size="small"
                      >
                        Skip
                      </Button>
                    )}
                  </Box>
                </StepContent>
              </Step>
            );
          })}
        </Stepper>

        {/* Completion Summary */}
        {activeStep >= sortedSteps.length && (
          <Box sx={{ mt: 3 }}>
            <Alert severity="success" sx={{ mb: 2 }}>
              <Typography variant="subtitle1" gutterBottom>
                🎉 Routine Complete!
              </Typography>
              <Typography variant="body2">
                Total time: {formatTime(totalDuration)}
              </Typography>
            </Alert>
            <Button
              variant="outlined"
              startIcon={<RestartIcon />}
              onClick={handleRestart}
              fullWidth
            >
              Start Again
            </Button>
          </Box>
        )}

        {/* Session Stats */}
        <Divider sx={{ my: 2 }} />
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          <Chip
            label={`Session time: ${formatTime(totalDuration)}`}
            size="small"
            variant="outlined"
          />
          {template.estimatedDuration && (
            <Chip
              label={`Estimated: ${formatEstimatedTime(template.estimatedDuration)}`}
              size="small"
              variant="outlined"
            />
          )}
        </Box>
      </CardContent>

      {/* Timer Dialog */}
      {currentStep && (
        <Dialog open={timerOpen} onClose={() => setTimerOpen(false)} maxWidth="sm" fullWidth>
          <DialogContent>
            <HabitTimer
              habit={habit}
              onComplete={() => {
                setTimerOpen(false);
                // Timer completed, user can now complete the step
              }}
            />
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
};

export default RoutineSessionView;
