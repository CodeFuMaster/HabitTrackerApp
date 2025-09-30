# 🚀 HabitTrackerApp Cross-Platform Implementation Plan

## 📋 **Phase 1: Foundation Setup (Weeks 1-2)**

### **Step 1.1: Project Structure Setup**

```
HabitTrackerApp/
├── HabitTrackerApp.Core/              # Shared business logic & models
│   ├── Models/
│   │   ├── Enhanced/                  # Enhanced models for complex routines
│   │   │   ├── RoutineSession.cs
│   │   │   ├── SessionActivity.cs
│   │   │   └── ActivityTemplate.cs
│   │   └── [Existing models]
│   ├── Services/
│   │   ├── Sync/
│   │   │   ├── ISyncService.cs
│   │   │   └── OfflineSyncService.cs
│   │   └── [Other services]
│   └── Interfaces/
├── HabitTrackerApp.API/               # Local network server
│   ├── Controllers/
│   ├── Hubs/                         # SignalR hubs
│   └── Data/
├── HabitTrackerApp.MAUI/             # Cross-platform app
│   ├── Platforms/
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── MacCatalyst/
│   │   └── Windows/
│   ├── Views/
│   ├── ViewModels/
│   └── Services/
└── HabitTrackerApp.Web/              # Blazor web interface
    ├── Components/
    └── Services/
```

### **Step 1.2: Technology Setup Commands**

```bash
# Create solution
dotnet new sln -n HabitTrackerApp

# Create projects
dotnet new classlib -n HabitTrackerApp.Core
dotnet new webapi -n HabitTrackerApp.API
dotnet new maui -n HabitTrackerApp.MAUI
dotnet new blazorserver -n HabitTrackerApp.Web

# Add projects to solution
dotnet sln add HabitTrackerApp.Core
dotnet sln add HabitTrackerApp.API
dotnet sln add HabitTrackerApp.MAUI
dotnet sln add HabitTrackerApp.Web

# Add package references
dotnet add HabitTrackerApp.Core package Microsoft.EntityFrameworkCore
dotnet add HabitTrackerApp.Core package Microsoft.Data.Sqlite
dotnet add HabitTrackerApp.API package Microsoft.AspNetCore.SignalR
dotnet add HabitTrackerApp.API package Microsoft.EntityFrameworkCore.Sqlite
dotnet add HabitTrackerApp.MAUI package CommunityToolkit.Mvvm
dotnet add HabitTrackerApp.MAUI package Microsoft.Data.Sqlite
```

## 📊 **Phase 2: Enhanced Data Model Implementation (Week 3)**

### **Step 2.1: Create Enhanced Models**

Based on your routine document, we need to support:

1. **Complex Routine Sessions** (e.g., "Tuesday Gym", "Morning Routine")
2. **Free-form Activities** within sessions (exercises, meditation, breathing)
3. **Flexible Metrics** (reps, weight, duration, notes)
4. **Timer Integration** for timed activities
5. **Progress Tracking** per activity and overall session

### **Step 2.2: Example Usage for Your Routines**

```csharp
// Tuesday Gym Session
var gymSession = new RoutineSession
{
    HabitId = tuesdayGymHabit.Id,
    Date = DateTime.Today,
    Activities = new List<SessionActivity>
    {
        new SessionActivity 
        { 
            Name = "Trap-bar Deadlift", 
            Type = ActivityType.Strength,
            Metrics = new List<ActivityMetric>
            {
                new ActivityMetric { Name = "Sets", NumericValue = 5 },
                new ActivityMetric { Name = "Reps", NumericValue = 3 },
                new ActivityMetric { Name = "Weight", NumericValue = 100, Unit = "kg" }
            }
        },
        new SessionActivity 
        { 
            Name = "Pull-ups", 
            Type = ActivityType.Strength,
            Metrics = new List<ActivityMetric>
            {
                new ActivityMetric { Name = "Sets", NumericValue = 4 },
                new ActivityMetric { Name = "Reps", NumericValue = 8 }
            }
        }
    }
};

// Morning Breathing Session with Timer
var breathingSession = new RoutineSession
{
    HabitId = morningRoutineHabit.Id,
    Date = DateTime.Today,
    Activities = new List<SessionActivity>
    {
        new SessionActivity 
        { 
            Name = "Wim Hof Breathing", 
            Type = ActivityType.Breathing,
            Duration = TimeSpan.FromMinutes(15),
            Metrics = new List<ActivityMetric>
            {
                new ActivityMetric { Name = "Rounds", NumericValue = 4 },
                new ActivityMetric { Name = "Hold Time", TimeValue = TimeSpan.FromSeconds(90) }
            }
        }
    }
};
```

## 🔄 **Phase 3: Offline-First Sync Implementation (Week 4)**

### **Step 3.1: Local SQLite Database Structure**

```sql
-- Enhanced tables for offline sync
CREATE TABLE SyncLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TableName TEXT NOT NULL,
    RecordId INTEGER NOT NULL,
    Operation TEXT NOT NULL, -- INSERT, UPDATE, DELETE
    Timestamp DATETIME NOT NULL,
    Data TEXT, -- JSON serialized record
    DeviceId TEXT NOT NULL,
    Synced BOOLEAN DEFAULT FALSE
);

CREATE TABLE Settings (
    Key TEXT PRIMARY KEY,
    Value TEXT
);

-- Add sync fields to existing tables
ALTER TABLE Habits ADD COLUMN LastModifiedDate DATETIME;
ALTER TABLE Habits ADD COLUMN DeviceId TEXT;
ALTER TABLE DailyHabitEntries ADD COLUMN DeviceId TEXT;

-- New tables for enhanced functionality
CREATE TABLE RoutineSessions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    HabitId INTEGER NOT NULL,
    Date DATE NOT NULL,
    IsCompleted BOOLEAN DEFAULT FALSE,
    StartedAt DATETIME,
    CompletedAt DATETIME,
    Notes TEXT,
    Rating INTEGER,
    DeviceId TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (HabitId) REFERENCES Habits(Id)
);

CREATE TABLE SessionActivities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RoutineSessionId INTEGER NOT NULL,
    ActivityTemplateId INTEGER,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,
    IsCompleted BOOLEAN DEFAULT FALSE,
    Duration TEXT, -- TimeSpan as string
    StartedAt DATETIME,
    CompletedAt DATETIME,
    [Order] INTEGER NOT NULL,
    Notes TEXT,
    DeviceId TEXT,
    FOREIGN KEY (RoutineSessionId) REFERENCES RoutineSessions(Id)
);

CREATE TABLE ActivityMetrics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionActivityId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    NumericValue REAL,
    TextValue TEXT,
    TimeValue TEXT, -- TimeSpan as string
    Unit TEXT,
    SetNumber INTEGER,
    DeviceId TEXT,
    FOREIGN KEY (SessionActivityId) REFERENCES SessionActivities(Id)
);
```

### **Step 3.2: Sync Service Implementation**

The sync service will:
1. **Track all changes** locally in SyncLog table
2. **Detect server availability** on local network
3. **Send local changes** to server when available
4. **Receive server changes** and apply with conflict resolution
5. **Support real-time sync** via SignalR when online

## 🎯 **Phase 4: MAUI App UI Implementation (Weeks 5-6)**

### **Step 4.1: Main Navigation Structure**

```csharp
// MainPage.xaml - Tab-based navigation
<TabView>
    <TabViewItem Header="Today" IconImageSource="today.png">
        <views:TodayView />
    </TabViewItem>
    <TabViewItem Header="Week" IconImageSource="week.png">
        <views:WeekView />
    </TabViewItem>
    <TabViewItem Header="Habits" IconImageSource="habits.png">
        <views:HabitsView />
    </TabViewItem>
    <TabViewItem Header="Stats" IconImageSource="stats.png">
        <views:StatsView />
    </TabViewItem>
</TabView>
```

### **Step 4.2: Enhanced Habit Detail View with Sidebar**

```csharp
// HabitDetailView.xaml - Sidebar implementation
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    
    <!-- Main content area -->
    <ScrollView Grid.Column="0">
        <StackLayout>
            <!-- Habit overview, completion toggle, etc. -->
        </StackLayout>
    </ScrollView>
    
    <!-- Sidebar panel (slides in when habit clicked) -->
    <Frame Grid.Column="1" 
           IsVisible="{Binding ShowActivityPanel}"
           WidthRequest="350"
           BackgroundColor="LightGray">
        <CollectionView ItemsSource="{Binding CurrentSessionActivities}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <views:ActivityItemView />
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Frame>
</Grid>
```

### **Step 4.3: Activity Tracking with Timer**

```csharp
// ActivityItemView.xaml - Individual activity tracking
<Frame Padding="10" HasShadow="True">
    <StackLayout>
        <Label Text="{Binding Name}" FontSize="18" FontAttributes="Bold" />
        
        <!-- Timer for timed activities -->
        <StackLayout IsVisible="{Binding HasTimer}">
            <Label Text="{Binding TimerDisplay}" FontSize="36" HorizontalOptions="Center" />
            <StackLayout Orientation="Horizontal" HorizontalOptions="Center">
                <Button Text="Start" Command="{Binding StartTimerCommand}" />
                <Button Text="Pause" Command="{Binding PauseTimerCommand}" />
                <Button Text="Stop" Command="{Binding StopTimerCommand}" />
            </StackLayout>
        </StackLayout>
        
        <!-- Metrics input -->
        <CollectionView ItemsSource="{Binding Metrics}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <StackLayout Orientation="Horizontal">
                        <Label Text="{Binding Name}" WidthRequest="80" />
                        <Entry Text="{Binding Value}" WidthRequest="100" />
                        <Label Text="{Binding Unit}" />
                    </StackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
        <!-- Completion toggle -->
        <CheckBox IsChecked="{Binding IsCompleted}" />
        
        <!-- Notes -->
        <Editor Text="{Binding Notes}" Placeholder="Notes..." HeightRequest="60" />
    </StackLayout>
</Frame>
```

## ⚡ **Phase 5: Real-time Sync & API Implementation (Week 7)**

### **Step 5.1: SignalR Hub for Real-time Updates**

```csharp
// SyncHub.cs
public class SyncHub : Hub
{
    public async Task JoinGroup(string deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "SyncGroup");
    }
    
    public async Task NotifyDataChanged(string tableName, int recordId, string operation)
    {
        await Clients.Others.SendAsync("DataChanged", tableName, recordId, operation);
    }
}
```

### **Step 5.2: API Controllers for Sync**

```csharp
// SyncController.cs
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    [HttpPost("receive-changes")]
    public async Task<IActionResult> ReceiveChanges([FromBody] List<SyncRecord> changes)
    {
        // Apply changes to central database
        // Broadcast to other connected devices
        return Ok();
    }
    
    [HttpGet("changes-since/{timestamp}")]
    public async Task<ActionResult<List<SyncRecord>>> GetChangesSince(DateTime timestamp)
    {
        // Return all changes since timestamp
        return Ok(changes);
    }
    
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("Server available");
}
```

## 📱 **Phase 6: Platform-Specific Features (Week 8)**

### **Step 6.1: Local Network Discovery**

```csharp
// NetworkDiscoveryService.cs
public class NetworkDiscoveryService
{
    public async Task<string> DiscoverServerAsync()
    {
        // Scan local network for HabitTracker server
        // Return server IP:Port when found
        var ipAddresses = new[] { "192.168.1.", "192.168.0.", "10.0.0." };
        
        foreach (var baseIp in ipAddresses)
        {
            for (int i = 1; i < 255; i++)
            {
                var testIp = $"{baseIp}{i}:5000";
                if (await TestServerConnection(testIp))
                    return testIp;
            }
        }
        
        return null;
    }
}
```

### **Step 6.2: Background Sync Service**

```csharp
// BackgroundSyncService.cs - Platform-specific implementations
#if ANDROID
[Service]
public class BackgroundSyncService : Service
{
    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        // Start background sync
        Task.Run(async () => await PerformSync());
        return StartCommandResult.Sticky;
    }
}
#endif

#if WINDOWS
// Windows background task implementation
#endif
```

## 🎨 **Phase 7: UI Polish & User Experience (Week 9)**

### **Step 7.1: Responsive Design**

- **Mobile First**: Optimized for phone screens
- **Tablet Layout**: Sidebar always visible, more content
- **Desktop Layout**: Multi-pane interface

### **Step 7.2: Dark Mode Support**

```xml
<!-- App.xaml - Theme definitions -->
<Application.Resources>
    <ResourceDictionary>
        <Color x:Key="PrimaryColor">#6366F1</Color>
        <Color x:Key="SecondaryColor">#10B981</Color>
        
        <!-- Light theme -->
        <Color x:Key="BackgroundColorLight">#FFFFFF</Color>
        <Color x:Key="TextColorLight">#1F2937</Color>
        
        <!-- Dark theme -->
        <Color x:Key="BackgroundColorDark">#111827</Color>
        <Color x:Key="TextColorDark">#E5E7EB</Color>
    </ResourceDictionary>
</Application.Resources>
```

## 🔒 **Security & Performance Considerations**

### **Data Encryption**
- SQLite database encryption using SQLCipher
- Network communication over HTTPS/WSS
- Local storage encryption for sensitive data

### **Performance Optimization**
- Lazy loading for large datasets
- Image caching and compression
- Background sync with exponential backoff
- Local database indexing for fast queries

### **Error Handling**
- Comprehensive logging system
- Graceful offline mode handling
- User-friendly error messages
- Automatic retry mechanisms

## 🚀 **Deployment Strategy**

### **Local Network Server Setup**
1. **Raspberry Pi Deployment**: Run API server on local network
2. **Router Configuration**: Port forwarding for WiFi access
3. **Service Discovery**: Automatic server detection
4. **Backup Strategy**: Regular SQLite database backups

### **App Distribution**
1. **Android**: APK sideloading initially, Play Store later
2. **Windows**: MSIX package for easy installation
3. **iOS**: TestFlight for beta testing
4. **Web**: Local network access via browser

## 🎯 **Your Specific Use Cases Solved**

### **1. Gym Session Tracking**
- Click "Tuesday Gym" → Sidebar opens with exercise list
- Track each exercise independently (reps, weight, sets)
- Complete individual exercises or whole session
- Timer support for rest periods between sets

### **2. Morning Routine with Timers**
- Wim Hof breathing with built-in timer and round counter
- Meditation timer with progress tracking
- Cold shower timer with temperature notes
- Automatic progression to next activity

### **3. Martial Arts Training**
- Free-form activity logging
- Technique practice tracking
- Sparring session notes
- Progress photos and videos (optional)

### **4. Offline Gym Usage**
- Complete all tracking offline
- Automatic sync when WiFi available
- Conflict resolution for multiple device usage
- Data integrity guaranteed

This implementation plan provides a robust, scalable, and user-friendly solution that meets all your requirements while maintaining simplicity and zero cost. The offline-first approach ensures reliability, and the .NET MAUI cross-platform strategy minimizes development complexity.

Would you like me to start implementing any specific part of this plan?