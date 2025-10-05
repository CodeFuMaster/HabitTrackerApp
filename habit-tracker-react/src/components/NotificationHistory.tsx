import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Stack,
  IconButton,
  Divider,
} from '@mui/material';
import {
  History,
  Delete,
  FilterList,
  Refresh,
} from '@mui/icons-material';
import { notificationService, type NotificationHistoryItem } from '../services/notificationService';
import { format } from 'date-fns';
import { useNotification } from '../contexts/NotificationContext';

export default function NotificationHistory() {
  const { showSuccess } = useNotification();
  const [open, setOpen] = useState(false);
  const [history, setHistory] = useState<NotificationHistoryItem[]>([]);
  const [filter, setFilter] = useState<'all' | 'viewed' | 'dismissed' | 'snoozed' | 'completed'>('all');
  const [searchTerm, setSearchTerm] = useState('');

  const loadHistory = () => {
    const allHistory = notificationService.getHistory();
    setHistory(allHistory);
  };

  useEffect(() => {
    if (open) {
      loadHistory();
    }
  }, [open]);

  const handleClearHistory = () => {
    notificationService.clearHistory();
    setHistory([]);
    showSuccess('Notification history cleared');
  };

  const filteredHistory = history.filter(item => {
    const matchesFilter = filter === 'all' || item.action === filter;
    const matchesSearch = searchTerm === '' || 
      item.habitName.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesFilter && matchesSearch;
  });

  const getActionColor = (action: string) => {
    switch (action) {
      case 'completed': return 'success';
      case 'snoozed': return 'warning';
      case 'viewed': return 'info';
      case 'dismissed': return 'default';
      default: return 'default';
    }
  };

  const getActionIcon = (action: string) => {
    switch (action) {
      case 'completed': return '✓';
      case 'snoozed': return '💤';
      case 'viewed': return '👁️';
      case 'dismissed': return '✕';
      default: return '';
    }
  };

  return (
    <>
      <Button
        variant="outlined"
        startIcon={<History />}
        onClick={() => setOpen(true)}
      >
        Notification History
      </Button>

      <Dialog
        open={open}
        onClose={() => setOpen(false)}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6">Notification History</Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <IconButton size="small" onClick={loadHistory} title="Refresh">
                <Refresh />
              </IconButton>
              {history.length > 0 && (
                <IconButton size="small" onClick={handleClearHistory} title="Clear All" color="error">
                  <Delete />
                </IconButton>
              )}
            </Box>
          </Box>
        </DialogTitle>

        <DialogContent>
          {/* Filters */}
          <Box sx={{ mb: 3, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <TextField
              size="small"
              placeholder="Search habits..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              sx={{ flex: 1, minWidth: 200 }}
            />
            <TextField
              select
              size="small"
              label="Filter by action"
              value={filter}
              onChange={(e) => setFilter(e.target.value as any)}
              sx={{ minWidth: 150 }}
              InputProps={{
                startAdornment: <FilterList sx={{ mr: 1, fontSize: 18 }} />,
              }}
            >
              <MenuItem value="all">All Actions</MenuItem>
              <MenuItem value="viewed">Viewed</MenuItem>
              <MenuItem value="dismissed">Dismissed</MenuItem>
              <MenuItem value="snoozed">Snoozed</MenuItem>
              <MenuItem value="completed">Completed</MenuItem>
            </TextField>
          </Box>

          {/* History List */}
          {filteredHistory.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 6 }}>
              <Typography color="text.secondary">
                {history.length === 0
                  ? 'No notification history yet'
                  : 'No notifications match your filters'}
              </Typography>
            </Box>
          ) : (
            <Stack spacing={1} sx={{ maxHeight: '500px', overflow: 'auto' }}>
              {filteredHistory.map((item) => (
                <Card key={item.id} variant="outlined">
                  <CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Box sx={{ flex: 1 }}>
                        <Typography variant="body1" fontWeight="medium">
                          {item.habitName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {format(new Date(item.timestamp), 'MMM d, yyyy • h:mm a')}
                        </Typography>
                      </Box>
                      <Chip
                        label={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            <span>{getActionIcon(item.action)}</span>
                            <span>{item.action}</span>
                          </Box>
                        }
                        size="small"
                        color={getActionColor(item.action)}
                      />
                    </Box>
                  </CardContent>
                </Card>
              ))}
            </Stack>
          )}

          {/* Statistics */}
          {history.length > 0 && (
            <>
              <Divider sx={{ my: 3 }} />
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Box sx={{ flex: 1, minWidth: 120 }}>
                  <Typography variant="caption" color="text.secondary">
                    Total Notifications
                  </Typography>
                  <Typography variant="h6">{history.length}</Typography>
                </Box>
                <Box sx={{ flex: 1, minWidth: 120 }}>
                  <Typography variant="caption" color="text.secondary">
                    Completed
                  </Typography>
                  <Typography variant="h6" color="success.main">
                    {history.filter(h => h.action === 'completed').length}
                  </Typography>
                </Box>
                <Box sx={{ flex: 1, minWidth: 120 }}>
                  <Typography variant="caption" color="text.secondary">
                    Snoozed
                  </Typography>
                  <Typography variant="h6" color="warning.main">
                    {history.filter(h => h.action === 'snoozed').length}
                  </Typography>
                </Box>
                <Box sx={{ flex: 1, minWidth: 120 }}>
                  <Typography variant="caption" color="text.secondary">
                    Dismissed
                  </Typography>
                  <Typography variant="h6">
                    {history.filter(h => h.action === 'dismissed').length}
                  </Typography>
                </Box>
              </Box>
            </>
          )}
        </DialogContent>

        <DialogActions>
          <Button onClick={() => setOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
