# Habit Tracker Mobile App

A cross-platform mobile application built with .NET MAUI for tracking habits and routines. Features offline-first functionality with synchronization to a local ASP.NET Core API.

## 🚀 Features

### ✅ **Cross-Platform Support**
- **iOS** - Native iOS app
- **Android** - Native Android app  
- **Windows** - Native Windows app
- **macOS** - Native macOS app via MacCatalyst

### ✅ **Offline-First Architecture**
- **SQLite Local Database** - All data stored locally for offline access
- **Background Sync** - Automatic synchronization when connected to WiFi
- **Conflict Resolution** - Smart handling of data conflicts during sync

### ✅ **Advanced Habit Tracking**
- **Simple Habits** - Daily, weekly, monthly habit completion tracking
- **Complex Routines** - Multi-activity sessions with timers (gym workouts, morning routines)
- **Activity Templates** - Reusable templates for common activities
- **Metrics Tracking** - Custom metrics for each activity (reps, sets, duration, etc.)

### ✅ **Smart UI**
- **MVVM Pattern** - Clean architecture with CommunityToolkit.Mvvm
- **Real-time Updates** - Live timer updates during routine sessions
- **Intuitive Navigation** - Easy-to-use interface for habit management
- **Responsive Design** - Adaptive UI for all screen sizes

## 🏗️ Architecture

### **Core Components**

1. **Models** (`/Models/`)
   - `Habit.cs` - Core habit data model
   - `RoutineSession.cs` - Complex routine tracking
   - `DailyHabitEntry.cs` - Daily completion records
   - `Category.cs` - Habit categorization

2. **Services** (`/Services/`)
   - `ApiService.cs` - Communication with ASP.NET Core API
   - `LocalDatabaseService.cs` - SQLite operations
   - `SyncService.cs` - Offline-first synchronization

3. **ViewModels** (`/ViewModels/`)
   - `MainViewModel.cs` - Dashboard with today's habits
   - `HabitListViewModel.cs` - Habit management
   - `HabitDetailViewModel.cs` - Detailed habit editing
   - `RoutineSessionViewModel.cs` - Complex routine tracking with timers

## 🛠️ Technical Stack

- **.NET 9.0** - Latest .NET framework
- **.NET MAUI** - Cross-platform UI framework
- **CommunityToolkit.Mvvm** - MVVM pattern implementation
- **SQLite** - Local database for offline storage
- **System.Text.Json** - JSON serialization for API communication
- **CommunityToolkit.Maui** - Additional UI components

## 🔧 Prerequisites

- **Visual Studio 2022** (17.8 or later) with MAUI workload
- **.NET 9.0 SDK**
- **Android SDK** (for Android development)
- **Xcode** (for iOS development on macOS)

## 🚀 Getting Started

### 1. **Clone and Setup**
```bash
git clone <repository-url>
cd HabitTrackerMobile
```

### 2. **Restore Dependencies**
```bash
dotnet restore
```

### 3. **Build Project**
```bash
dotnet build
```

### 4. **Run on Specific Platform**
```bash
# Windows
dotnet build -f net9.0-windows10.0.19041.0

# Android
dotnet build -f net9.0-android

# iOS (macOS only)
dotnet build -f net9.0-ios

# macOS (macOS only)
dotnet build -f net9.0-maccatalyst
```

## 📡 API Connection

The mobile app connects to the ASP.NET Core API running on:
```
http://localhost:5178/api/
```

### **API Endpoints Used:**
- `GET /api/habit` - Fetch all habits
- `POST /api/habit` - Create new habit
- `GET /api/enhanced/routine-sessions/{habitId}` - Get routine sessions
- `POST /api/enhanced/routine-session` - Create routine session
- `GET /api/enhanced/test-gym-session` - Test gym routine
- `GET /api/enhanced/test-morning-routine` - Test morning routine

## 💾 Data Models

### **Habit Tracking Hierarchy:**
```
Habit
├── DailyHabitEntry (simple completion tracking)
└── RoutineSession (complex routine tracking)
    └── SessionActivity (individual activities within session)
        └── ActivityMetric (performance metrics)
```

### **Example Use Cases:**

**Simple Habit:**
- "Drink Water" - Daily completion checkbox

**Complex Routine:**
- "Tuesday Gym Session"
  - Activity: "Squats" (3 sets, 10 reps, 135lbs)
  - Activity: "Bench Press" (3 sets, 8 reps, 185lbs)
  - Activity: "Deadlifts" (1 set, 5 reps, 225lbs)

## 🔄 Offline Sync

### **Sync Strategy:**
1. **Local First** - All operations work offline
2. **Background Sync** - Automatic sync when online
3. **Conflict Resolution** - Last-write-wins with timestamps
4. **Queue Management** - Pending changes tracked for sync

### **Sync Process:**
1. Changes made locally are queued
2. When online, queued changes are pushed to API
3. Latest data is pulled from API
4. Local database is updated with remote changes

## 🧪 Testing

### **Test the API Connection:**
The app includes test endpoints to verify connectivity:
- Gym session simulation
- Morning routine simulation
- Database integration tests

### **Offline Testing:**
1. Disconnect from internet
2. Create/modify habits
3. Reconnect to internet
4. Verify automatic synchronization

## 📱 Platform-Specific Features

### **Android:**
- Material Design components
- Android notifications for habit reminders
- Background sync service

### **iOS:**
- Native iOS UI components
- iOS notifications
- Background app refresh

### **Windows:**
- Windows UI 3 components
- Native Windows notifications
- System tray integration

### **macOS:**
- Mac-style UI components
- macOS notifications
- Menu bar integration

## 🎯 Usage Examples

### **Daily Habit Tracking:**
1. Open app to see today's habits
2. Tap checkboxes to mark habits complete
3. View streak counters and progress

### **Complex Routine Session:**
1. Select a habit with routine support
2. Start a new routine session
3. Add activities (exercises, meditation, etc.)
4. Use built-in timers for each activity
5. Track metrics (reps, sets, duration)
6. Complete session with notes

### **Offline Usage:**
1. All features work without internet
2. Data syncs automatically when online
3. Conflict resolution handles simultaneous edits

## 🔮 Future Enhancements

- **Push Notifications** - Habit reminders
- **Analytics Dashboard** - Progress visualization
- **Social Features** - Share progress with friends
- **Habit Templates** - Pre-built habit configurations
- **Integration APIs** - Connect with fitness trackers

## 📄 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

**Built with ❤️ using .NET MAUI**