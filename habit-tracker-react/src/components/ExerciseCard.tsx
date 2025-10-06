import { useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Box,
  Chip,
  IconButton,
  Collapse,
  Button,
} from '@mui/material';
import {
  Edit,
  Delete,
  ExpandMore,
  PlayArrow,
  FitnessCenter,
  AccessTime,
  Speed,
} from '@mui/icons-material';
import { Exercise } from '../types/habit.types';

interface ExerciseCardProps {
  exercise: Exercise;
  onEdit: (exercise: Exercise) => void;
  onDelete: (exerciseId: number) => void;
  isDragging?: boolean;
}

export default function ExerciseCard({ exercise, onEdit, onDelete, isDragging }: ExerciseCardProps) {
  const [expanded, setExpanded] = useState(false);

  const handleExpandClick = () => {
    setExpanded(!expanded);
  };

  return (
    <Card
      sx={{
        opacity: isDragging ? 0.5 : 1,
        cursor: isDragging ? 'grabbing' : 'grab',
        transition: 'opacity 0.2s',
        '&:hover': {
          boxShadow: 3,
        },
      }}
    >
      {/* Exercise Image */}
      {exercise.imageUrl && (
        <CardMedia
          component="img"
          height="140"
          image={exercise.imageUrl}
          alt={exercise.name}
          sx={{ objectFit: 'cover' }}
        />
      )}

      <CardContent>
        {/* Header: Name and Actions */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
              {exercise.orderIndex}. {exercise.name}
            </Typography>
            <Chip
              label={exercise.exerciseType || 'Strength'}
              size="small"
              sx={{ mt: 0.5 }}
            />
          </Box>
          
          <Box>
            <IconButton size="small" onClick={() => onEdit(exercise)} title="Edit">
              <Edit fontSize="small" />
            </IconButton>
            <IconButton size="small" onClick={() => onDelete(exercise.id!)} title="Delete" color="error">
              <Delete fontSize="small" />
            </IconButton>
          </Box>
        </Box>

        {/* Target Metrics */}
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
          {exercise.targetSets && exercise.targetReps && (
            <Chip
              icon={<FitnessCenter />}
              label={`${exercise.targetSets}x${exercise.targetReps}`}
              size="small"
              variant="outlined"
            />
          )}
          
          {exercise.targetWeight && (
            <Chip
              label={`${exercise.targetWeight} lbs`}
              size="small"
              variant="outlined"
            />
          )}
          
          {exercise.targetDuration && (
            <Chip
              icon={<AccessTime />}
              label={`${exercise.targetDuration}s`}
              size="small"
              variant="outlined"
            />
          )}
          
          {exercise.targetRPE && (
            <Chip
              icon={<Speed />}
              label={`RPE ${exercise.targetRPE}`}
              size="small"
              variant="outlined"
              color="primary"
            />
          )}
          
          {exercise.restSeconds && (
            <Chip
              label={`${exercise.restSeconds}s rest`}
              size="small"
              variant="outlined"
            />
          )}
        </Box>

        {/* Muscle Groups & Equipment */}
        {(exercise.muscleGroups || exercise.equipment) && (
          <Box sx={{ mb: 1 }}>
            {exercise.muscleGroups && (
              <Typography variant="caption" display="block" color="text.secondary">
                <strong>Muscles:</strong> {exercise.muscleGroups}
              </Typography>
            )}
            {exercise.equipment && (
              <Typography variant="caption" display="block" color="text.secondary">
                <strong>Equipment:</strong> {exercise.equipment}
              </Typography>
            )}
          </Box>
        )}

        {/* Expand Button */}
        <Button
          onClick={handleExpandClick}
          endIcon={<ExpandMore sx={{ transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)', transition: '0.3s' }} />}
          size="small"
          fullWidth
          sx={{ mt: 1 }}
        >
          {expanded ? 'Show Less' : 'Show More'}
        </Button>

        {/* Expanded Content */}
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Box sx={{ mt: 2, pt: 2, borderTop: 1, borderColor: 'divider' }}>
            {/* Notes */}
            {exercise.notes && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" fontWeight="bold" gutterBottom>
                  Instructions:
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {exercise.notes}
                </Typography>
              </Box>
            )}

            {/* Video Link */}
            {exercise.videoUrl && (
              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle2" fontWeight="bold" gutterBottom>
                  Video Tutorial:
                </Typography>
                <Button
                  variant="outlined"
                  startIcon={<PlayArrow />}
                  href={exercise.videoUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  size="small"
                >
                  Watch Video
                </Button>
              </Box>
            )}

            {/* Metadata */}
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, mt: 2 }}>
              <Typography variant="caption" color="text.secondary">
                <strong>Status:</strong> {exercise.isActive ? 'Active' : 'Inactive'}
              </Typography>
              {exercise.createdDate && (
                <Typography variant="caption" color="text.secondary">
                  <strong>Created:</strong> {new Date(exercise.createdDate).toLocaleDateString()}
                </Typography>
              )}
            </Box>
          </Box>
        </Collapse>
      </CardContent>
    </Card>
  );
}
