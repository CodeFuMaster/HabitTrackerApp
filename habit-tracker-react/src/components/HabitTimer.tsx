import { useState, useEffect, useRef } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  IconButton,
  ToggleButtonGroup,
  ToggleButton,
  TextField,
  LinearProgress,
  Chip,
} from '@mui/material';
import {
  PlayArrow,
  Pause,
  Stop,
  Replay,
  Timer as TimerIcon,
  Speed,
} from '@mui/icons-material';
import type { Habit } from '../types/habit.types';

interface HabitTimerProps {
  habit: Habit;
  onComplete?: (duration: number) => void;
}

type TimerMode = 'timer' | 'stopwatch';

export default function HabitTimer({ habit, onComplete }: HabitTimerProps) {
  const [mode, setMode] = useState<TimerMode>('timer');
  const [isRunning, setIsRunning] = useState(false);
  const [isPaused, setIsPaused] = useState(false);
  const [seconds, setSeconds] = useState(habit.duration ? habit.duration * 60 : 0);
  const [timerDuration, setTimerDuration] = useState(habit.duration || 25); // minutes
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const startTimeRef = useRef<number>(0);

  // Format seconds to MM:SS
  const formatTime = (totalSeconds: number): string => {
    const mins = Math.floor(Math.abs(totalSeconds) / 60);
    const secs = Math.abs(totalSeconds) % 60;
    const sign = totalSeconds < 0 ? '-' : '';
    return `${sign}${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  // Start timer/stopwatch
  const handleStart = () => {
    setIsRunning(true);
    setIsPaused(false);
    startTimeRef.current = Date.now();

    if (mode === 'timer' && seconds === 0) {
      setSeconds(timerDuration * 60);
    }

    intervalRef.current = setInterval(() => {
      if (mode === 'timer') {
        setSeconds((prev) => {
          const newSeconds = prev - 1;
          if (newSeconds <= 0) {
            handleStop();
            return 0;
          }
          return newSeconds;
        });
      } else {
        setSeconds((prev) => prev + 1);
      }
      setElapsedSeconds((prev) => prev + 1);
    }, 1000);
  };

  // Pause
  const handlePause = () => {
    setIsPaused(true);
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  };

  // Resume
  const handleResume = () => {
    setIsPaused(false);
    handleStart();
  };

  // Stop and complete
  const handleStop = () => {
    setIsRunning(false);
    setIsPaused(false);
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
    
    if (onComplete && elapsedSeconds > 0) {
      onComplete(elapsedSeconds);
    }
  };

  // Reset
  const handleReset = () => {
    setIsRunning(false);
    setIsPaused(false);
    setSeconds(mode === 'timer' ? timerDuration * 60 : 0);
    setElapsedSeconds(0);
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
  };

  // Change mode
  const handleModeChange = (_: React.MouseEvent<HTMLElement>, newMode: TimerMode | null) => {
    if (newMode && !isRunning) {
      setMode(newMode);
      setSeconds(newMode === 'timer' ? timerDuration * 60 : 0);
      setElapsedSeconds(0);
    }
  };

  // Update timer duration
  const handleDurationChange = (minutes: number) => {
    if (!isRunning && mode === 'timer') {
      setTimerDuration(minutes);
      setSeconds(minutes * 60);
    }
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, []);

  // Calculate progress for timer
  const progress = mode === 'timer' 
    ? ((timerDuration * 60 - seconds) / (timerDuration * 60)) * 100
    : 0;

  return (
    <Card sx={{ maxWidth: 500, margin: 'auto' }}>
      <CardContent>
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="h6">{habit.name}</Typography>
          </Box>
          <ToggleButtonGroup
            value={mode}
            exclusive
            onChange={handleModeChange}
            size="small"
            disabled={isRunning}
          >
            <ToggleButton value="timer">
              <TimerIcon fontSize="small" sx={{ mr: 0.5 }} />
              Timer
            </ToggleButton>
            <ToggleButton value="stopwatch">
              <Speed fontSize="small" sx={{ mr: 0.5 }} />
              Stopwatch
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>

        {/* Timer Duration Setting (Timer mode only) */}
        {mode === 'timer' && !isRunning && (
          <Box sx={{ mb: 3 }}>
            <Typography variant="caption" color="text.secondary" gutterBottom>
              Duration (minutes)
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              {[5, 10, 15, 20, 25, 30, 45, 60].map((min) => (
                <Chip
                  key={min}
                  label={`${min}m`}
                  onClick={() => handleDurationChange(min)}
                  color={timerDuration === min ? 'primary' : 'default'}
                  variant={timerDuration === min ? 'filled' : 'outlined'}
                  size="small"
                />
              ))}
            </Box>
            <TextField
              type="number"
              size="small"
              value={timerDuration}
              onChange={(e) => handleDurationChange(Number(e.target.value))}
              sx={{ mt: 2, maxWidth: 120 }}
              inputProps={{ min: 1, max: 999 }}
            />
          </Box>
        )}

        {/* Time Display */}
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography 
            variant="h1" 
            component="div"
            sx={{ 
              fontWeight: 'bold',
              fontSize: { xs: '3rem', sm: '4rem', md: '5rem' },
              fontVariantNumeric: 'tabular-nums',
              color: seconds < 0 ? 'error.main' : 'text.primary',
            }}
          >
            {formatTime(seconds)}
          </Typography>
          
          {mode === 'stopwatch' && elapsedSeconds > 0 && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Elapsed: {formatTime(elapsedSeconds)}
            </Typography>
          )}
        </Box>

        {/* Progress Bar (Timer mode only) */}
        {mode === 'timer' && (
          <LinearProgress 
            variant="determinate" 
            value={progress} 
            sx={{ mb: 3, height: 8, borderRadius: 4 }}
          />
        )}

        {/* Control Buttons */}
        <Box sx={{ display: 'flex', justifyContent: 'center', gap: 2, mb: 2 }}>
          {!isRunning && !isPaused && (
            <IconButton 
              size="large"
              color="primary"
              onClick={handleStart}
              sx={{ 
                bgcolor: 'primary.main', 
                color: 'white',
                width: 64,
                height: 64,
                '&:hover': { bgcolor: 'primary.dark' }
              }}
            >
              <PlayArrow sx={{ fontSize: 40 }} />
            </IconButton>
          )}

          {isRunning && !isPaused && (
            <IconButton 
              size="large"
              color="warning"
              onClick={handlePause}
              sx={{ 
                bgcolor: 'warning.main', 
                color: 'white',
                width: 64,
                height: 64,
                '&:hover': { bgcolor: 'warning.dark' }
              }}
            >
              <Pause sx={{ fontSize: 40 }} />
            </IconButton>
          )}

          {isPaused && (
            <IconButton 
              size="large"
              color="success"
              onClick={handleResume}
              sx={{ 
                bgcolor: 'success.main', 
                color: 'white',
                width: 64,
                height: 64,
                '&:hover': { bgcolor: 'success.dark' }
              }}
            >
              <PlayArrow sx={{ fontSize: 40 }} />
            </IconButton>
          )}

          {(isRunning || isPaused) && (
            <IconButton 
              size="large"
              color="error"
              onClick={handleStop}
              sx={{ 
                bgcolor: 'error.main', 
                color: 'white',
                width: 64,
                height: 64,
                '&:hover': { bgcolor: 'error.dark' }
              }}
            >
              <Stop sx={{ fontSize: 40 }} />
            </IconButton>
          )}

          {!isRunning && !isPaused && seconds !== (mode === 'timer' ? timerDuration * 60 : 0) && (
            <IconButton 
              size="large"
              onClick={handleReset}
              sx={{ 
                border: 1,
                borderColor: 'divider',
                width: 64,
                height: 64,
              }}
            >
              <Replay sx={{ fontSize: 32 }} />
            </IconButton>
          )}
        </Box>

        {/* Stats */}
        {elapsedSeconds > 0 && (
          <Box sx={{ mt: 3, p: 2, bgcolor: 'action.hover', borderRadius: 1 }}>
            <Typography variant="caption" color="text.secondary">
              Session Stats
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, mt: 1, flexWrap: 'wrap' }}>
              <Chip 
                size="small" 
                label={`Total: ${formatTime(elapsedSeconds)}`} 
                color="primary" 
                variant="outlined"
              />
              {isPaused && (
                <Chip 
                  size="small" 
                  label="Paused" 
                  color="warning" 
                  variant="filled"
                />
              )}
            </Box>
          </Box>
        )}
      </CardContent>
    </Card>
  );
}
