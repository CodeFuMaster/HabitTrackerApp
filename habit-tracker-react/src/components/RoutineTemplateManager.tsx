import React, { useState } from 'react';
import {
  Box,
  Button,
  TextField,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Typography,
  FormControlLabel,
  Switch,
  Divider,
  Alert,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  DragIndicator as DragIcon,
  Timer as TimerIcon,
  CheckCircle as CompleteIcon,
} from '@mui/icons-material';
import { useNotification } from '../contexts/NotificationContext';
import type { RoutineTemplate, RoutineStep } from '../types/habit.types';

interface RoutineTemplateManagerProps {
  habitId: number;
  templates: RoutineTemplate[];
  onSave: (updatedTemplates: RoutineTemplate[]) => void;
}

const RoutineTemplateManager: React.FC<RoutineTemplateManagerProps> = ({
  habitId,
  templates: initialTemplates,
  onSave,
}) => {
  const { showError } = useNotification();
  const [templates, setTemplates] = useState<RoutineTemplate[]>(initialTemplates);
  const [openDialog, setOpenDialog] = useState(false);
  const [_confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [editingTemplate, setEditingTemplate] = useState<RoutineTemplate | null>(null);
  const [openStepDialog, setOpenStepDialog] = useState(false);
  const [editingStep, setEditingStep] = useState<RoutineStep | null>(null);
  
  // Form state for template
  const [templateForm, setTemplateForm] = useState({
    name: '',
    description: '',
    isActive: true,
  });

  // Form state for step
  const [stepForm, setStepForm] = useState({
    name: '',
    description: '',
    duration: 0,
    isOptional: false,
  });

  const [currentTemplateSteps, setCurrentTemplateSteps] = useState<RoutineStep[]>([]);

  // Open new template dialog
  const handleAddTemplate = () => {
    setEditingTemplate(null);
    setTemplateForm({ name: '', description: '', isActive: true });
    setCurrentTemplateSteps([]);
    setOpenDialog(true);
  };

  // Open edit template dialog
  const handleEditTemplate = (template: RoutineTemplate) => {
    setEditingTemplate(template);
    setTemplateForm({
      name: template.name,
      description: template.description || '',
      isActive: template.isActive,
    });
    setCurrentTemplateSteps(template.steps || []);
    setOpenDialog(true);
  };

  // Save template
  const handleSaveTemplate = () => {
    if (!templateForm.name.trim()) {
      showError('Please enter a template name');
      return;
    }

    if (currentTemplateSteps.length === 0) {
      showError('Please add at least one step to the routine');
      return;
    }

    const estimatedDuration = currentTemplateSteps.reduce(
      (sum, step) => sum + (step.duration || 0),
      0
    );

    const newTemplate: RoutineTemplate = {
      id: editingTemplate?.id || Date.now(),
      habitId,
      name: templateForm.name,
      description: templateForm.description,
      steps: currentTemplateSteps,
      estimatedDuration,
      isActive: templateForm.isActive,
    };

    let updatedTemplates: RoutineTemplate[];
    if (editingTemplate) {
      updatedTemplates = templates.map((t) =>
        t.id === editingTemplate.id ? newTemplate : t
      );
    } else {
      updatedTemplates = [...templates, newTemplate];
    }

    setTemplates(updatedTemplates);
    onSave(updatedTemplates);
    setOpenDialog(false);
  };

  // Delete template
  const handleDeleteTemplate = (templateId: number) => {
    const updatedTemplates = templates.filter((t) => t.id !== templateId);
    setTemplates(updatedTemplates);
    onSave(updatedTemplates);
    setConfirmDeleteId(null);
  };

  // Open add step dialog
  const handleAddStep = () => {
    setEditingStep(null);
    setStepForm({ name: '', description: '', duration: 0, isOptional: false });
    setOpenStepDialog(true);
  };

  // Open edit step dialog
  const handleEditStep = (step: RoutineStep) => {
    setEditingStep(step);
    setStepForm({
      name: step.name,
      description: step.description || '',
      duration: step.duration || 0,
      isOptional: step.isOptional || false,
    });
    setOpenStepDialog(true);
  };

  // Save step
  const handleSaveStep = () => {
    if (!stepForm.name.trim()) {
      showError('Please enter a step name');
      return;
    }

    const newStep: RoutineStep = {
      id: editingStep?.id || Date.now(),
      templateId: editingTemplate?.id || Date.now(),
      name: stepForm.name,
      description: stepForm.description,
      duration: stepForm.duration,
      order: editingStep?.order || currentTemplateSteps.length,
      isOptional: stepForm.isOptional,
      metrics: [],
    };

    let updatedSteps: RoutineStep[];
    if (editingStep) {
      updatedSteps = currentTemplateSteps.map((s) =>
        s.id === editingStep.id ? newStep : s
      );
    } else {
      updatedSteps = [...currentTemplateSteps, newStep];
    }

    setCurrentTemplateSteps(updatedSteps);
    setOpenStepDialog(false);
  };

  // Delete step
  const handleDeleteStep = (stepId: number) => {
    const updatedSteps = currentTemplateSteps.filter((s) => s.id !== stepId);
    setCurrentTemplateSteps(updatedSteps);
  };

  // Move step up
  const handleMoveStepUp = (index: number) => {
    if (index === 0) return;
    const updatedSteps = [...currentTemplateSteps];
    [updatedSteps[index - 1], updatedSteps[index]] = [
      updatedSteps[index],
      updatedSteps[index - 1],
    ];
    updatedSteps.forEach((step, i) => (step.order = i));
    setCurrentTemplateSteps(updatedSteps);
  };

  // Move step down
  const handleMoveStepDown = (index: number) => {
    if (index === currentTemplateSteps.length - 1) return;
    const updatedSteps = [...currentTemplateSteps];
    [updatedSteps[index], updatedSteps[index + 1]] = [
      updatedSteps[index + 1],
      updatedSteps[index],
    ];
    updatedSteps.forEach((step, i) => (step.order = i));
    setCurrentTemplateSteps(updatedSteps);
  };

  return (
    <Box>
      {/* Template List */}
      {templates.length === 0 ? (
        <Alert severity="info" sx={{ mb: 2 }}>
          No routine templates defined. Click "Add Routine Template" to create a multi-step workflow for this habit.
        </Alert>
      ) : (
        <List>
          {templates.map((template) => (
            <ListItem
              key={template.id}
              secondaryAction={
                <Box>
                  <IconButton
                    edge="end"
                    onClick={() => handleEditTemplate(template)}
                    size="small"
                    sx={{ mr: 1 }}
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    edge="end"
                    onClick={() => handleDeleteTemplate(template.id)}
                    size="small"
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              }
              sx={{
                border: 1,
                borderColor: 'divider',
                borderRadius: 1,
                mb: 1,
              }}
            >
              <ListItemText
                primary={
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Typography fontWeight={500}>{template.name}</Typography>
                    {!template.isActive && (
                      <Chip label="Inactive" size="small" color="default" />
                    )}
                  </Box>
                }
                secondary={
                  <Box>
                    {template.description && (
                      <Typography variant="body2" color="text.secondary">
                        {template.description}
                      </Typography>
                    )}
                    <Box sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
                      <Chip
                        icon={<CompleteIcon />}
                        label={`${template.steps?.length || 0} steps`}
                        size="small"
                        variant="outlined"
                      />
                      {template.estimatedDuration && (
                        <Chip
                          icon={<TimerIcon />}
                          label={`~${template.estimatedDuration} min`}
                          size="small"
                          variant="outlined"
                        />
                      )}
                    </Box>
                  </Box>
                }
              />
            </ListItem>
          ))}
        </List>
      )}

      <Button
        startIcon={<AddIcon />}
        onClick={handleAddTemplate}
        variant="outlined"
        fullWidth
        sx={{ mt: 1 }}
      >
        Add Routine Template
      </Button>

      {/* Template Edit Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingTemplate ? 'Edit Routine Template' : 'New Routine Template'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <TextField
              label="Template Name"
              fullWidth
              value={templateForm.name}
              onChange={(e) =>
                setTemplateForm({ ...templateForm, name: e.target.value })
              }
              sx={{ mb: 2 }}
              required
            />
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={templateForm.description}
              onChange={(e) =>
                setTemplateForm({ ...templateForm, description: e.target.value })
              }
              sx={{ mb: 2 }}
            />
            <FormControlLabel
              control={
                <Switch
                  checked={templateForm.isActive}
                  onChange={(e) =>
                    setTemplateForm({ ...templateForm, isActive: e.target.checked })
                  }
                />
              }
              label="Active"
              sx={{ mb: 2 }}
            />

            <Divider sx={{ my: 2 }} />

            <Typography variant="h6" gutterBottom>
              Routine Steps
            </Typography>

            {currentTemplateSteps.length === 0 ? (
              <Alert severity="warning" sx={{ mb: 2 }}>
                No steps added yet. Add steps to create your routine workflow.
              </Alert>
            ) : (
              <List>
                {currentTemplateSteps.map((step, index) => (
                  <ListItem
                    key={step.id}
                    secondaryAction={
                      <Box>
                        <IconButton
                          onClick={() => handleMoveStepUp(index)}
                          disabled={index === 0}
                          size="small"
                        >
                          ↑
                        </IconButton>
                        <IconButton
                          onClick={() => handleMoveStepDown(index)}
                          disabled={index === currentTemplateSteps.length - 1}
                          size="small"
                        >
                          ↓
                        </IconButton>
                        <IconButton
                          onClick={() => handleEditStep(step)}
                          size="small"
                          sx={{ ml: 1 }}
                        >
                          <EditIcon />
                        </IconButton>
                        <IconButton
                          onClick={() => handleDeleteStep(step.id)}
                          size="small"
                        >
                          <DeleteIcon />
                        </IconButton>
                      </Box>
                    }
                    sx={{
                      border: 1,
                      borderColor: 'divider',
                      borderRadius: 1,
                      mb: 1,
                    }}
                  >
                    <DragIcon sx={{ mr: 1, color: 'action.disabled' }} />
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography fontWeight={500}>
                            {index + 1}. {step.name}
                          </Typography>
                          {step.isOptional && (
                            <Chip label="Optional" size="small" />
                          )}
                        </Box>
                      }
                      secondary={
                        <Box>
                          {step.description && (
                            <Typography variant="body2">{step.description}</Typography>
                          )}
                          {step.duration && (
                            <Chip
                              icon={<TimerIcon />}
                              label={`${step.duration} min`}
                              size="small"
                              variant="outlined"
                              sx={{ mt: 0.5 }}
                            />
                          )}
                        </Box>
                      }
                    />
                  </ListItem>
                ))}
              </List>
            )}

            <Button
              startIcon={<AddIcon />}
              onClick={handleAddStep}
              variant="outlined"
              fullWidth
            >
              Add Step
            </Button>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cancel</Button>
          <Button onClick={handleSaveTemplate} variant="contained">
            Save Template
          </Button>
        </DialogActions>
      </Dialog>

      {/* Step Edit Dialog */}
      <Dialog open={openStepDialog} onClose={() => setOpenStepDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingStep ? 'Edit Step' : 'New Step'}</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <TextField
              label="Step Name"
              fullWidth
              value={stepForm.name}
              onChange={(e) => setStepForm({ ...stepForm, name: e.target.value })}
              sx={{ mb: 2 }}
              required
            />
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={stepForm.description}
              onChange={(e) =>
                setStepForm({ ...stepForm, description: e.target.value })
              }
              sx={{ mb: 2 }}
            />
            <TextField
              label="Duration (minutes)"
              type="number"
              fullWidth
              value={stepForm.duration}
              onChange={(e) =>
                setStepForm({ ...stepForm, duration: parseInt(e.target.value) || 0 })
              }
              sx={{ mb: 2 }}
            />
            <FormControlLabel
              control={
                <Switch
                  checked={stepForm.isOptional}
                  onChange={(e) =>
                    setStepForm({ ...stepForm, isOptional: e.target.checked })
                  }
                />
              }
              label="Optional Step"
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenStepDialog(false)}>Cancel</Button>
          <Button onClick={handleSaveStep} variant="contained">
            {editingStep ? 'Save Changes' : 'Add Step'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default RoutineTemplateManager;
