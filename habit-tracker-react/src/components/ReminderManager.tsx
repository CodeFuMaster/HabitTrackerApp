import { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Switch,
  FormControlLabel,
  TextField,
  Typography,
  Card,
  CardContent,
  Chip,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material';
import {
  Notifications,
  NotificationsOff,
  VolumeUp,
  VolumeOff,
  Schedule,
  Science,
  Delete,
  Add,
} from '@mui/icons-material';
import {
  notificationService,
  type ReminderSettings,
  formatTime,
  getDayAbbr,
} from '../services/notificationService';
import { useNotification } from '../contexts/NotificationContext';

interface ReminderManagerProps {
  habitId: number;
  habitName: string;
}

export default function ReminderManager({ habitId, habitName }: ReminderManagerProps) {
  const { showSuccess, showError, showInfo } = useNotification();
  const [permission, setPermission] = useState<NotificationPermission>('default');
  const [settings, setSettings] = useState<ReminderSettings>({
    habitId,
    habitName,
    enabled: false,
    time: '09:00',
    days: [1, 2, 3, 4, 5], // Weekdays by default
    sound: true,
    vibrate: true,
  });
  const [openHistory, setOpenHistory] = useState(false);

  useEffect(() => {
    // Load existing settings
    const existing = notificationService.getReminder(habitId);
    if (existing) {
      setSettings(existing);
    }
    
    // Check permission
    setPermission(notificationService.getPermission());
  }, [habitId]);

  const handleRequestPermission = async () => {
    const granted = await notificationService.requestPermission();
    if (granted) {
      setPermission('granted');
      showSuccess('Notification permission granted!');
    } else {
      showError('Notification permission denied');
    }
  };

  const handleSave = () => {
    const updatedSettings = {
      ...settings,
      habitId,
      habitName,
    };
    
    notificationService.saveReminder(updatedSettings);
    setSettings(updatedSettings);
    
    if (updatedSettings.enabled) {
      showSuccess('Reminder saved and activated!');
    } else {
      showInfo('Reminder settings saved (currently disabled)');
    }
  };

  const handleToggleEnabled = (checked: boolean) => {
    if (checked && permission !== 'granted') {
      handleRequestPermission();
      return;
    }
    
    setSettings({ ...settings, enabled: checked });
  };

  const handleToggleDay = (day: number) => {
    const newDays = settings.days.includes(day)
      ? settings.days.filter(d => d !== day)
      : [...settings.days, day].sort();
    
    setSettings({ ...settings, days: newDays });
  };

  const handleTestNotification = async () => {
    if (permission !== 'granted') {
      await handleRequestPermission();
      return;
    }
    
    await notificationService.testNotification();
    showSuccess('Test notification sent!');
  };

  const handleDelete = () => {
    notificationService.deleteReminder(habitId);
    setSettings({
      ...settings,
      enabled: false,
    });
    showSuccess('Reminder deleted');
  };

  const history = notificationService.getHistory().filter(h => h.habitId === habitId);

  return (
    <Box>
      {/* Permission Request */}
      {permission !== 'granted' && (
        <Alert 
          severity="warning" 
          sx={{ mb: 2 }}
          action={
            <Button color="inherit" size="small" onClick={handleRequestPermission}>
              Enable
            </Button>
          }
        >
          Notifications are disabled. Enable them to receive reminders.
        </Alert>
      )}

      {/* Main Settings Card */}
      <Card variant="outlined">
        <CardContent>
          {/* Enable/Disable Switch */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {settings.enabled ? <Notifications color="primary" /> : <NotificationsOff />}
              <Typography variant="h6">
                Reminder Settings
              </Typography>
            </Box>
            <FormControlLabel
              control={
                <Switch
                  checked={settings.enabled}
                  onChange={(e) => handleToggleEnabled(e.target.checked)}
                  disabled={permission !== 'granted'}
                />
              }
              label={settings.enabled ? 'Enabled' : 'Disabled'}
            />
          </Box>

          {/* Time Picker */}
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Schedule fontSize="small" />
              Reminder Time
            </Typography>
            <TextField
              type="time"
              value={settings.time}
              onChange={(e) => setSettings({ ...settings, time: e.target.value })}
              fullWidth
              disabled={!settings.enabled}
              helperText={`You'll be reminded at ${formatTime(settings.time)}`}
            />
          </Box>

          {/* Days of Week */}
          <Box sx={{ mb: 3 }}>
            <Typography variant="subtitle2" gutterBottom>
              Repeat On
            </Typography>
            <ToggleButtonGroup
              value={settings.days}
              onChange={(_, newDays) => setSettings({ ...settings, days: newDays })}
              aria-label="days of week"
              sx={{ flexWrap: 'wrap', gap: 0.5 }}
              disabled={!settings.enabled}
            >
              {[0, 1, 2, 3, 4, 5, 6].map((day) => (
                <ToggleButton
                  key={day}
                  value={day}
                  selected={settings.days.includes(day)}
                  onChange={() => handleToggleDay(day)}
                  sx={{ px: 1.5, minWidth: 45 }}
                >
                  {getDayAbbr(day)}
                </ToggleButton>
              ))}
            </ToggleButtonGroup>
            {settings.days.length === 0 && (
              <Typography variant="caption" color="error" sx={{ mt: 1, display: 'block' }}>
                Please select at least one day
              </Typography>
            )}
          </Box>

          {/* Sound Settings */}
          <Box sx={{ mb: 2 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={settings.sound}
                  onChange={(e) => setSettings({ ...settings, sound: e.target.checked })}
                  disabled={!settings.enabled}
                />
              }
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  {settings.sound ? <VolumeUp fontSize="small" /> : <VolumeOff fontSize="small" />}
                  <Typography variant="body2">Play Sound</Typography>
                </Box>
              }
            />
          </Box>

          {/* Action Buttons */}
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              onClick={handleSave}
              disabled={!settings.enabled || settings.days.length === 0}
              startIcon={settings.enabled ? <Notifications /> : <Add />}
            >
              {settings.enabled ? 'Save Reminder' : 'Enable Reminder'}
            </Button>
            
            <Button
              variant="outlined"
              onClick={handleTestNotification}
              startIcon={<Science />}
              disabled={permission !== 'granted'}
            >
              Test
            </Button>

            {settings.enabled && (
              <Button
                variant="outlined"
                color="error"
                onClick={handleDelete}
                startIcon={<Delete />}
              >
                Delete
              </Button>
            )}
            
            {history.length > 0 && (
              <Button
                variant="text"
                onClick={() => setOpenHistory(true)}
              >
                History ({history.length})
              </Button>
            )}
          </Box>

          {/* Current Schedule Summary */}
          {settings.enabled && (
            <Box sx={{ mt: 3, p: 2, bgcolor: 'primary.light', borderRadius: 1 }}>
              <Typography variant="body2" color="primary.dark" fontWeight={600}>
                📅 Active Reminder Schedule
              </Typography>
              <Typography variant="caption" color="primary.dark">
                {formatTime(settings.time)} on {settings.days.map(d => getDayAbbr(d)).join(', ')}
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* History Dialog */}
      <Dialog
        open={openHistory}
        onClose={() => setOpenHistory(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Notification History</DialogTitle>
        <DialogContent>
          {history.length === 0 ? (
            <Typography color="text.secondary" align="center" sx={{ py: 3 }}>
              No notification history yet
            </Typography>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              {history.slice(0, 20).map((item) => (
                <Card key={item.id} variant="outlined">
                  <CardContent sx={{ py: 1.5 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Box>
                        <Typography variant="body2">
                          {new Date(item.timestamp).toLocaleString()}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {item.habitName}
                        </Typography>
                      </Box>
                      <Chip
                        label={item.action}
                        size="small"
                        color={
                          item.action === 'completed' ? 'success' :
                          item.action === 'snoozed' ? 'warning' :
                          item.action === 'viewed' ? 'info' :
                          'default'
                        }
                      />
                    </Box>
                  </CardContent>
                </Card>
              ))}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          {history.length > 0 && (
            <Button
              onClick={() => {
                notificationService.clearHistory();
                setOpenHistory(false);
                showSuccess('History cleared');
              }}
              color="error"
            >
              Clear History
            </Button>
          )}
          <Button onClick={() => setOpenHistory(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
