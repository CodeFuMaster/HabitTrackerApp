import { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline, Box, CircularProgress, Typography } from '@mui/material';
import { NotificationProvider } from './contexts/NotificationContext';
import Navigation from './components/Navigation';
import TodayView from './pages/TodayView';
import WeekView from './pages/WeekView';
import HabitsView from './pages/HabitsView';
import CategoriesView from './pages/CategoriesView';
import StatsView from './pages/StatsView';
import { ErrorBoundary } from './components/ErrorBoundary';
import { syncService } from './services/syncService';
import { seedSampleData } from './utils/seedData';

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: Infinity, // Never consider data stale - rely on manual invalidation only
      gcTime: Infinity, // Keep data in cache forever (previously cacheTime)
      refetchOnWindowFocus: false, // Don't refetch when window regains focus
      refetchOnMount: false, // Don't refetch when component mounts
      refetchOnReconnect: false, // Don't refetch when reconnecting
      retry: false, // Don't retry failed requests
    },
    mutations: {
      retry: false, // Don't retry failed mutations
    },
  },
});

// Create Material-UI theme
const theme = createTheme({
  palette: {
    primary: {
      main: '#6366F1', // Indigo
    },
    secondary: {
      main: '#10B981', // Green
    },
    background: {
      default: '#F9FAFB',
      paper: '#FFFFFF',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  },
  shape: {
    borderRadius: 12,
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
        },
      },
    },
  },
});

function App() {
  const [isInitializing, setIsInitializing] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Initialize sync service and load data
    const initialize = async () => {
      try {
        console.log('Initializing app...');
        await syncService.initialize();
        
        // Seed sample data if database is empty
        await seedSampleData();
        
        // Try to load initial data, but don't fail if server is offline
        try {
          await syncService.loadInitialData();
          console.log('Initial data loaded from server');
        } catch (dataError) {
          console.warn('Could not load initial data from server, working offline:', dataError);
          // This is OK - we'll work with local data only
        }
        
        console.log('App initialized successfully');
        setIsInitializing(false);
      } catch (err) {
        console.error('Failed to initialize app:', err);
        setError(err instanceof Error ? err.message : 'Failed to initialize');
        setIsInitializing(false);
      }
    };

    initialize();

    // Cleanup on unmount
    return () => {
      syncService.stopPeriodicSync();
    };
  }, []);

  if (isInitializing) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '100vh',
            gap: 2,
          }}
        >
          <CircularProgress size={60} />
          <Typography variant="h6" color="text.secondary">
            Loading Habit Tracker...
          </Typography>
        </Box>
      </ThemeProvider>
    );
  }

  if (error) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '100vh',
            gap: 2,
            p: 3,
          }}
        >
          <Typography variant="h5" color="warning.main" gutterBottom>
            Initialization Issue
          </Typography>
          <Typography variant="body1" color="text.secondary" align="center">
            {error}
          </Typography>
          <Typography variant="body2" color="text.secondary" align="center">
            The app will work in offline mode with local data only.
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ mt: 2 }}>
            Check that your MVC server is running on port 5178.
          </Typography>
        </Box>
      </ThemeProvider>
    );
  }

  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <NotificationProvider>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <BrowserRouter>
              <Navigation />
              <Routes>
                <Route path="/" element={<Navigate to="/today" replace />} />
                <Route path="/today" element={<TodayView />} />
                <Route path="/week" element={<WeekView />} />
                <Route path="/habits" element={<HabitsView />} />
                <Route path="/categories" element={<CategoriesView />} />
                <Route path="/stats" element={<StatsView />} />
              </Routes>
            </BrowserRouter>
          </ThemeProvider>
        </NotificationProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;
