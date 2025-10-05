# Enhanced Routine Tracking - Integration Guide

**Status:** UI Components Complete ✅ | Integration In Progress 🔄

This guide shows how to integrate the new Enhanced Routine Tracking components into the existing React app.

## Components Available

### 1. ActivityLogger
**Purpose:** Rich activity logging with notes, rating, mood, energy  
**File:** `src/components/ActivityLogger.tsx` (180 lines)  
**Status:** ✅ Complete, ready for integration

### 2. HabitTimer
**Purpose:** Built-in timer/stopwatch for habit tracking  
**File:** `src/components/HabitTimer.tsx` (300+ lines)  
**Status:** ✅ Complete, ready for integration

### 3. CustomMetricsManager
**Purpose:** Define custom trackable fields per habit  
**File:** `src/components/CustomMetricsManager.tsx` (260 lines)  
**Status:** ✅ Complete, ready for integration

### 4. MetricInput
**Purpose:** Dynamic form inputs for entering metric values  
**File:** `src/components/MetricInput.tsx` (220 lines)  
**Status:** ✅ Complete, ready for integration

### 5. RoutineSessionView
**Purpose:** Multi-step routine workflow with progress tracking  
**File:** `src/components/RoutineSessionView.tsx` (320 lines)  
**Status:** ✅ Complete, ready for integration

### 6. MetricValuesDisplay
**Purpose:** Display historical metric values with statistics  
**File:** `src/components/MetricValuesDisplay.tsx` (240 lines)  
**Status:** ✅ Complete, ready for integration

---

## Integration Steps

### Step 1: Integrate ActivityLogger into TodayView

**Location:** `src/pages/TodayView.tsx`

**Add state and handlers:**
```typescript
const [activityLoggerOpen, setActivityLoggerOpen] = useState(false);
const [selectedHabitForLog, setSelectedHabitForLog] = useState<Habit | null>(null);
const [selectedEntry, setSelectedEntry] = useState<DailyHabitEntry | null>(null);

const handleOpenActivityLogger = (habit: Habit, entry: DailyHabitEntry) => {
  setSelectedHabitForLog(habit);
  setSelectedEntry(entry);
  setActivityLoggerOpen(true);
};

const handleSaveActivity = async (updates: Partial<DailyHabitEntry>) => {
  if (selectedEntry) {
    const updated = { ...selectedEntry, ...updates };
    await saveDailyEntry(updated);
    setActivityLoggerOpen(false);
  }
};
```

**Add button to habit card:**
```typescript
// In the habit card, add an IconButton
<IconButton 
  size="small" 
  onClick={() => handleOpenActivityLogger(habit, entry)}
  title="Log Activity Details"
>
  <NoteAdd />
</IconButton>
```

**Add dialog:**
```typescript
<Dialog 
  open={activityLoggerOpen} 
  onClose={() => setActivityLoggerOpen(false)}
  maxWidth="sm"
  fullWidth
>
  {selectedHabitForLog && selectedEntry && (
    <ActivityLogger
      habit={selectedHabitForLog}
      entry={selectedEntry}
      onSave={handleSaveActivity}
      onClose={() => setActivityLoggerOpen(false)}
    />
  )}
</Dialog>
```

---

### Step 2: Integrate HabitTimer into TodayView

**Add state and handlers:**
```typescript
const [timerOpen, setTimerOpen] = useState(false);
const [timerHabit, setTimerHabit] = useState<Habit | null>(null);

const handleOpenTimer = (habit: Habit) => {
  setTimerHabit(habit);
  setTimerOpen(true);
};

const handleTimerComplete = async (durationSeconds: number) => {
  if (timerHabit) {
    // Save timer session
    const session: TimerSession = {
      id: Date.now(),
      habitId: timerHabit.id,
      startTime: new Date().toISOString(),
      duration: durationSeconds,
      type: 'timer',
      isPaused: false,
      totalPausedTime: 0,
    };
    
    // TODO: Add to offlineDb when implemented
    // await saveTimerSession(session);
    
    setTimerOpen(false);
  }
};
```

**Add button to habit card:**
```typescript
<IconButton 
  size="small" 
  onClick={() => handleOpenTimer(habit)}
  title="Start Timer"
>
  <Timer />
</IconButton>
```

**Add dialog:**
```typescript
<Dialog 
  open={timerOpen} 
  onClose={() => setTimerOpen(false)}
  maxWidth="sm"
  fullWidth
>
  {timerHabit && (
    <HabitTimer
      habit={timerHabit}
      onComplete={handleTimerComplete}
    />
  )}
</Dialog>
```

---

### Step 3: Integrate CustomMetricsManager into HabitsView

**Location:** `src/pages/HabitsView.tsx`

**Add to edit dialog (after other form fields):**
```typescript
{/* Custom Metrics Section */}
<Box sx={{ mt: 3 }}>
  <Typography variant="subtitle2" gutterBottom>
    Custom Metrics
  </Typography>
  <Typography variant="body2" color="text.secondary" gutterBottom>
    Define custom trackable fields for this habit (weight, reps, mood, etc.)
  </Typography>
  <CustomMetricsManager
    habitId={formData.id || Date.now()}
    metrics={formData.metricDefinitions || []}
    onSave={(updatedMetrics) => {
      setFormData({
        ...formData,
        metricDefinitions: updatedMetrics
      });
    }}
  />
</Box>
```

**Update Habit type to include metricDefinitions:**
```typescript
// In habit.types.ts, add to Habit interface:
interface Habit {
  // ... existing fields
  metricDefinitions?: HabitMetricDefinition[];
}
```

---

### Step 4: Add Metric Input to Entry Logging

**Location:** `src/components/ActivityLogger.tsx` or wherever you log entries

**Add metric inputs:**
```typescript
{/* Custom Metrics */}
{habit.metricDefinitions && habit.metricDefinitions.length > 0 && (
  <Box sx={{ mt: 2 }}>
    <Typography variant="subtitle2" gutterBottom>
      Custom Metrics
    </Typography>
    {habit.metricDefinitions.map((metric) => (
      <MetricInput
        key={metric.id}
        metric={metric}
        value={customMetrics[metric.id]}
        onChange={(value) => {
          setCustomMetrics((prev) => ({
            ...prev,
            [metric.id]: value
          }));
        }}
      />
    ))}
  </Box>
)}
```

**Update save handler:**
```typescript
const handleSaveWithMetrics = () => {
  const updates = {
    notes,
    rating,
    mood,
    energyLevel,
    customMetrics: Object.values(customMetrics)
  };
  onSave(updates);
};
```

---

### Step 5: Display Metric Values

**Create a new view or add to habit details:**
```typescript
import MetricValuesDisplay from '../components/MetricValuesDisplay';

// In your component:
<MetricValuesDisplay
  metrics={habit.metricDefinitions || []}
  values={allMetricValuesForHabit}
  compact={false}  // Set to true for compact view
/>
```

**Compact view (for cards):**
```typescript
<MetricValuesDisplay
  metrics={habit.metricDefinitions || []}
  values={recentMetricValues}
  compact={true}
/>
```

---

### Step 6: Integrate RoutineSessionView

**Location:** Create new route or add to HabitsView

**Add route in App.tsx:**
```typescript
<Route path="/routine/:habitId/:templateId" element={<RoutineSessionPage />} />
```

**Create RoutineSessionPage.tsx:**
```typescript
import { useParams, useNavigate } from 'react-router-dom';
import RoutineSessionView from '../components/RoutineSessionView';

const RoutineSessionPage: React.FC = () => {
  const { habitId, templateId } = useParams();
  const navigate = useNavigate();
  
  const habit = useHabit(parseInt(habitId!));
  const template = habit?.routineTemplates?.find(t => t.id === parseInt(templateId!));
  
  const handleComplete = async (sessionData) => {
    // Save session data
    await saveRoutineSession({
      habitId: habit.id,
      templateId: template.id,
      completedSteps: sessionData.completedSteps,
      duration: sessionData.totalDuration,
      metrics: sessionData.metrics,
      timestamp: new Date().toISOString()
    });
    
    navigate('/today');
  };
  
  return template ? (
    <RoutineSessionView
      habit={habit}
      template={template}
      onComplete={handleComplete}
      onClose={() => navigate(-1)}
    />
  ) : (
    <div>Template not found</div>
  );
};
```

**Add "Start Routine" button:**
```typescript
{habit.routineTemplates && habit.routineTemplates.length > 0 && (
  <Button
    variant="contained"
    startIcon={<PlayArrow />}
    onClick={() => navigate(`/routine/${habit.id}/${habit.routineTemplates[0].id}`)}
  >
    Start Routine
  </Button>
)}
```

---

## Database Updates Required

### Update offlineDb.ts

**Add tables:**
```typescript
CREATE TABLE IF NOT EXISTS timer_sessions (
  id INTEGER PRIMARY KEY,
  habitId INTEGER NOT NULL,
  entryId INTEGER,
  startTime TEXT NOT NULL,
  endTime TEXT,
  duration INTEGER,
  isPaused INTEGER DEFAULT 0,
  pausedAt TEXT,
  totalPausedTime INTEGER DEFAULT 0,
  type TEXT DEFAULT 'timer',
  FOREIGN KEY (habitId) REFERENCES habits(id),
  FOREIGN KEY (entryId) REFERENCES dailyHabitEntries(id)
);

CREATE TABLE IF NOT EXISTS custom_metric_definitions (
  id INTEGER PRIMARY KEY,
  habitId INTEGER NOT NULL,
  name TEXT NOT NULL,
  type TEXT NOT NULL,
  unit TEXT,
  defaultValue TEXT,
  isRequired INTEGER DEFAULT 0,
  order INTEGER DEFAULT 0,
  options TEXT,
  FOREIGN KEY (habitId) REFERENCES habits(id)
);

CREATE TABLE IF NOT EXISTS custom_metric_values (
  id INTEGER PRIMARY KEY,
  entryId INTEGER NOT NULL,
  metricDefinitionId INTEGER NOT NULL,
  numericValue REAL,
  textValue TEXT,
  booleanValue INTEGER,
  timestamp TEXT NOT NULL,
  FOREIGN KEY (entryId) REFERENCES dailyHabitEntries(id),
  FOREIGN KEY (metricDefinitionId) REFERENCES custom_metric_definitions(id)
);

CREATE TABLE IF NOT EXISTS activity_logs (
  id INTEGER PRIMARY KEY,
  entryId INTEGER NOT NULL,
  timestamp TEXT NOT NULL,
  type TEXT NOT NULL,
  description TEXT,
  metadata TEXT,
  FOREIGN KEY (entryId) REFERENCES dailyHabitEntries(id)
);

CREATE TABLE IF NOT EXISTS routine_templates (
  id INTEGER PRIMARY KEY,
  habitId INTEGER NOT NULL,
  name TEXT NOT NULL,
  description TEXT,
  estimatedDuration INTEGER,
  isActive INTEGER DEFAULT 1,
  FOREIGN KEY (habitId) REFERENCES habits(id)
);

CREATE TABLE IF NOT EXISTS routine_steps (
  id INTEGER PRIMARY KEY,
  templateId INTEGER NOT NULL,
  name TEXT NOT NULL,
  description TEXT,
  duration INTEGER,
  order INTEGER NOT NULL,
  isOptional INTEGER DEFAULT 0,
  FOREIGN KEY (templateId) REFERENCES routine_templates(id)
);
```

**Add CRUD methods:**
```typescript
// Timer Sessions
async saveTimerSession(session: TimerSession): Promise<void> { /* ... */ }
async getTimerSessions(habitId: number): Promise<TimerSession[]> { /* ... */ }

// Custom Metrics
async saveMetricDefinition(metric: HabitMetricDefinition): Promise<void> { /* ... */ }
async getMetricDefinitions(habitId: number): Promise<HabitMetricDefinition[]> { /* ... */ }
async saveMetricValue(value: CustomMetricValue): Promise<void> { /* ... */ }
async getMetricValues(entryId: number): Promise<CustomMetricValue[]> { /* ... */ }

// Activity Logs
async saveActivityLog(log: ActivityLog): Promise<void> { /* ... */ }
async getActivityLogs(entryId: number): Promise<ActivityLog[]> { /* ... */ }

// Routine Templates
async saveRoutineTemplate(template: RoutineTemplate): Promise<void> { /* ... */ }
async getRoutineTemplates(habitId: number): Promise<RoutineTemplate[]> { /* ... */ }
```

---

## Testing Checklist

### ActivityLogger
- [ ] Opens in dialog when clicking "Log Activity"
- [ ] Can enter notes (multiline)
- [ ] Can select rating (1-5 stars)
- [ ] Can select mood (5 levels with emojis)
- [ ] Can select energy level (5 levels)
- [ ] Summary shows all selected values
- [ ] Save updates entry in database
- [ ] Cancel closes without saving

### HabitTimer
- [ ] Opens in dialog when clicking "Start Timer"
- [ ] Can toggle between Timer and Stopwatch modes
- [ ] Timer: Can select preset durations (5-60 min)
- [ ] Timer: Can enter custom duration
- [ ] Timer: Counts down correctly
- [ ] Timer: Shows progress bar
- [ ] Stopwatch: Counts up correctly
- [ ] Can pause and resume
- [ ] Can stop and reset
- [ ] onComplete fires with duration
- [ ] Duration saved to database

### CustomMetricsManager
- [ ] Shows empty state when no metrics
- [ ] Can add new metric
- [ ] Can select metric type (9 types)
- [ ] Can enter unit for numeric metrics
- [ ] Can toggle required field
- [ ] Can edit existing metrics
- [ ] Can delete metrics (with confirmation)
- [ ] Drag handles shown (ready for reordering)
- [ ] Saves to habit.metricDefinitions

### MetricInput
- [ ] Renders correct input for each type
- [ ] Number: Shows unit in adornment
- [ ] Text: Multiline input
- [ ] Boolean: Switch with Yes/No label
- [ ] Rating: Star rating component
- [ ] Time: Time picker input
- [ ] Distance/Weight/Reps/Sets: Number with unit
- [ ] Select: Dropdown with options
- [ ] Required fields marked with *
- [ ] onChange fires with correct value

### MetricValuesDisplay
- [ ] Shows empty state when no values
- [ ] Compact mode: Shows latest values only
- [ ] Compact mode: Shows trend indicators
- [ ] Full mode: Groups values by date
- [ ] Full mode: Shows all metrics per day
- [ ] Summary section: Shows avg/min/max
- [ ] Boolean values show Yes/No chips
- [ ] Rating values show stars + number

### RoutineSessionView
- [ ] Shows routine name and description
- [ ] Shows progress bar
- [ ] Shows all steps in stepper
- [ ] Can start timer for timed steps
- [ ] Can enter metrics for each step
- [ ] Can complete step
- [ ] Can skip optional steps
- [ ] Progress updates correctly
- [ ] Shows completion alert when done
- [ ] Can restart routine
- [ ] onComplete fires with session data

---

## Next Steps

1. **Implement Database Tables** (1-2 hours)
   - Add tables to offlineDb.ts
   - Implement CRUD methods
   - Test data persistence

2. **Integrate Components** (2-3 hours)
   - Add to TodayView (ActivityLogger, HabitTimer)
   - Add to HabitsView (CustomMetricsManager)
   - Add to WeekView (display indicators)
   - Create RoutineSessionPage

3. **Update Sync Service** (1-2 hours)
   - Add sync methods for new data types
   - Update push/pull logic
   - Handle conflicts

4. **Add Backend API** (optional, 2-3 hours)
   - Create API endpoints for new features
   - Update controllers
   - Add database migrations

5. **Testing & Polish** (1-2 hours)
   - Test all workflows
   - Fix bugs
   - Add loading states
   - Improve error handling

---

## Estimated Time to Complete

- **Database Implementation:** 1-2 hours
- **Component Integration:** 2-3 hours
- **Sync Service Updates:** 1-2 hours
- **Backend API (optional):** 2-3 hours
- **Testing & Polish:** 1-2 hours

**Total:** 7-12 hours (depending on backend implementation)

---

## Current Progress

- ✅ Type definitions (8 new interfaces/enums)
- ✅ ActivityLogger component (180 lines)
- ✅ HabitTimer component (300+ lines)
- ✅ CustomMetricsManager component (260 lines)
- ✅ MetricInput component (220 lines)
- ✅ RoutineSessionView component (320 lines)
- ✅ MetricValuesDisplay component (240 lines)
- 🔲 Database tables and methods
- 🔲 Component integration into views
- 🔲 Sync service updates
- 🔲 Backend API endpoints (optional)

**Overall:** ~50% complete (UI done, integration/backend pending)
