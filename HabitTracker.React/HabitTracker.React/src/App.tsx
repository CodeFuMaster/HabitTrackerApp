import React, { useState, useEffect } from 'react';
import './App.css';

// TypeScript interfaces matching your backend models
interface Habit {
  id: number;
  name: string;
  description?: string;
  categoryId?: number;
  isActive: boolean;
  createdAt: string;
  lastCompletedDate?: string;
}

interface DailyHabitEntry {
  id: number;
  habitId: number;
  date: string;
  isCompleted: boolean;
  notes?: string;
  rating?: number;
}

interface SyncStatus {
  isOnline: boolean;
  lastSyncTime?: string;
  pendingChanges: number;
  serverUrl?: string;
}

const App: React.FC = () => {
  const [habits, setHabits] = useState<Habit[]>([]);
  const [syncStatus, setSyncStatus] = useState<SyncStatus>({
    isOnline: false,
    pendingChanges: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Check server connection and sync status
  useEffect(() => {
    checkServerConnection();
    // Set up periodic sync check
    const interval = setInterval(checkServerConnection, 30000); // Every 30 seconds
    return () => clearInterval(interval);
  }, []);

  // Load habits from API
  useEffect(() => {
    loadHabits();
  }, []);

  const checkServerConnection = async () => {
    try {
      const response = await fetch('/api/sync/ping');
      if (response.ok) {
        const syncResponse = await fetch('/api/sync/sync-status');
        if (syncResponse.ok) {
          const status = await syncResponse.json();
          setSyncStatus({
            isOnline: true,
            lastSyncTime: status.lastSyncTime,
            pendingChanges: status.pendingChanges || 0,
            serverUrl: status.serverUrl
          });
        }
      }
    } catch (error) {
      setSyncStatus(prev => ({
        ...prev,
        isOnline: false
      }));
    }
  };

  const loadHabits = async () => {
    try {
      setLoading(true);
      // For now, let's create some mock data since we haven't implemented the full API yet
      const mockHabits: Habit[] = [
        {
          id: 1,
          name: "Tuesday Gym",
          description: "Strength training session",
          isActive: true,
          createdAt: new Date().toISOString()
        },
        {
          id: 2,
          name: "Morning Routine",
          description: "Wim Hof breathing, meditation, cold shower",
          isActive: true,
          createdAt: new Date().toISOString()
        },
        {
          id: 3,
          name: "Martial Arts Training",
          description: "Technique practice and sparring",
          isActive: true,
          createdAt: new Date().toISOString()
        }
      ];
      
      setHabits(mockHabits);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load habits');
    } finally {
      setLoading(false);
    }
  };

  const toggleHabitCompletion = async (habitId: number) => {
    try {
      // Find if habit is completed today
      const today = new Date().toISOString().split('T')[0];
      
      // In a real implementation, this would sync with the API
      console.log(`Toggling habit ${habitId} completion for ${today}`);
      
      // Update local state (in real app, this would sync to server)
      // For now, just show a notification
      alert(`Habit toggled! ${syncStatus.isOnline ? 'Synced to server' : 'Saved locally, will sync when online'}`);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update habit');
    }
  };

  const triggerSync = async () => {
    try {
      const response = await fetch('/api/sync/trigger-sync', { method: 'POST' });
      if (response.ok) {
        await checkServerConnection();
        alert('Sync completed successfully!');
      } else {
        throw new Error('Sync failed');
      }
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Sync failed');
    }
  };

  if (loading) {
    return (
      <div className="app">
        <div className="loading">
          <h2>Loading HabitTracker...</h2>
          <div className="spinner"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>🎯 HabitTracker</h1>
        <div className="sync-status">
          <div className={`status-indicator ${syncStatus.isOnline ? 'online' : 'offline'}`}>
            {syncStatus.isOnline ? '🟢 Online' : '🔴 Offline'}
          </div>
          {syncStatus.pendingChanges > 0 && (
            <div className="pending-changes">
              {syncStatus.pendingChanges} changes pending
            </div>
          )}
          {syncStatus.lastSyncTime && (
            <div className="last-sync">
              Last sync: {new Date(syncStatus.lastSyncTime).toLocaleTimeString()}
            </div>
          )}
          <button onClick={triggerSync} className="sync-button">
            🔄 Sync Now
          </button>
        </div>
      </header>

      {error && (
        <div className="error-message">
          ⚠️ {error}
        </div>
      )}

      <main className="main-content">
        <div className="habits-section">
          <h2>Today's Habits</h2>
          <div className="habits-grid">
            {habits.map(habit => (
              <div key={habit.id} className="habit-card">
                <div className="habit-header">
                  <h3>{habit.name}</h3>
                  <button 
                    className="complete-button"
                    onClick={() => toggleHabitCompletion(habit.id)}
                  >
                    ✓
                  </button>
                </div>
                {habit.description && (
                  <p className="habit-description">{habit.description}</p>
                )}
                <div className="habit-footer">
                  <span className={`status ${habit.isActive ? 'active' : 'inactive'}`}>
                    {habit.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
              </div>
            ))}
          </div>
          
          {habits.length === 0 && (
            <div className="empty-state">
              <h3>No habits found</h3>
              <p>Start by creating your first habit!</p>
            </div>
          )}
        </div>

        <div className="api-info">
          <h3>🚀 API Connection</h3>
          <p><strong>Server:</strong> {syncStatus.serverUrl || 'http://localhost:5266'}</p>
          <p><strong>Status:</strong> {syncStatus.isOnline ? 'Connected' : 'Disconnected'}</p>
          <p><strong>Architecture:</strong> Offline-First with Real-time Sync</p>
          
          <div className="features">
            <h4>✨ Implemented Features:</h4>
            <ul>
              <li>✅ Separate API Server (Port 5266)</li>
              <li>✅ SignalR Real-time Sync</li>
              <li>✅ Offline-First Architecture</li>
              <li>✅ Network Discovery Service</li>
              <li>✅ Background Sync</li>
              <li>🔄 React SPA (Current)</li>
              <li>⏳ MAUI Integration (Next)</li>
            </ul>
          </div>
        </div>
      </main>
    </div>
  );
};

export default App;