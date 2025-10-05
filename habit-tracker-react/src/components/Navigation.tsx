import { AppBar, Toolbar, Typography, IconButton, Tabs, Tab } from '@mui/material';
import { Sync as SyncIcon } from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useSync } from '../hooks/useHabits';

export default function Navigation() {
  const navigate = useNavigate();
  const location = useLocation();
  const { syncNow, isSyncing } = useSync();

  const currentTab = location.pathname === '/' ? '/today' : location.pathname;

  const handleTabChange = (_event: React.SyntheticEvent, newValue: string) => {
    navigate(newValue);
  };

  return (
    <AppBar position="static" elevation={1}>
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 0, mr: 4, fontWeight: 'bold' }}>
          🎯 Habit Tracker
        </Typography>

        <Tabs
          value={currentTab}
          onChange={handleTabChange}
          textColor="inherit"
          indicatorColor="secondary"
          variant="scrollable"
          scrollButtons="auto"
          allowScrollButtonsMobile
          sx={{ 
            flexGrow: 1,
            '& .MuiTab-root': {
              minWidth: { xs: 80, sm: 120 },
              fontSize: { xs: '0.875rem', sm: '1rem' },
            }
          }}
        >
          <Tab label="Today" value="/today" />
          <Tab label="Week" value="/week" />
          <Tab label="Habits" value="/habits" />
          <Tab label="Categories" value="/categories" />
          <Tab label="Stats" value="/stats" />
        </Tabs>

        <IconButton
          color="inherit"
          onClick={() => syncNow()}
          disabled={isSyncing}
          sx={{ ml: 2 }}
        >
          <SyncIcon sx={{ animation: isSyncing ? 'spin 1s linear infinite' : 'none' }} />
        </IconButton>
      </Toolbar>
    </AppBar>
  );
}
