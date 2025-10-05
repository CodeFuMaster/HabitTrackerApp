# 🚀 Quick Start Guide

## Running the App

### Start MVC Server (Terminal 1)
```powershell
cd C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp
dotnet run
```
**URL**: http://localhost:5178

### Start React App (Terminal 2)  
```powershell
cd C:\Repo\HabitTrackerApp\HabitTrackerApp\habit-tracker-react
npm run dev
```
**URL**: http://localhost:5174

---

## What Works Now

### ✅ Today View
- See all habits scheduled for today
- Mark habits as complete/incomplete
- View completion progress
- Click habit for details
- Auto-sync every 30 seconds

### ✅ Offline Mode
- Works without internet
- All data stored locally
- Syncs when server available

### ✅ Data Sync
- Automatic background sync
- Manual sync via refresh button
- Conflict resolution
- Real-time updates

---

## Testing Checklist

1. ✅ Open http://localhost:5174/
2. ✅ See "Loading Habit Tracker..."
3. ✅ See Today View with habits
4. ✅ Click checkbox to complete habit
5. ✅ Click habit card to see details
6. ✅ Click refresh to force sync
7. ✅ Check MVC server still running
8. ✅ Stop MVC server - app still works offline
9. ✅ Restart MVC server - auto-syncs

---

## File Structure

```
habit-tracker-react/
├── src/
│   ├── types/
│   │   └── habit.types.ts       # TypeScript types
│   ├── services/
│   │   ├── apiService.ts        # API calls
│   │   ├── offlineDb.ts         # SQLite database
│   │   └── syncService.ts       # Sync logic
│   ├── hooks/
│   │   └── useHabits.ts         # React hooks
│   ├── pages/
│   │   └── TodayView.tsx        # Main view
│   ├── App.tsx                  # Root component
│   ├── main.tsx                 # Entry point
│   ├── index.css                # Global styles
│   └── App.css                  # App styles
├── index.html                   # HTML template
├── package.json                 # Dependencies
├── tsconfig.json                # TypeScript config
└── vite.config.ts               # Vite config
```

---

## Troubleshooting

### Port Already in Use
If port 5174 is in use, Vite will automatically use 5175 or 5176.

### MVC Server Not Responding
Make sure it's running on port 5178:
```powershell
cd C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp
dotnet run
```

### Database Errors
Clear browser storage and reload:
1. Open DevTools (F12)
2. Application tab → Storage → Clear site data
3. Reload page

### Sync Not Working
1. Check MVC server is running
2. Check console for errors (F12)
3. Click refresh button manually

---

## Browser DevTools

### View Console Logs
1. Press `F12`
2. Go to Console tab
3. See sync status messages

### View Local Database
1. Press `F12`
2. Go to Application tab
3. IndexedDB → localforage → habit-tracker-db

### Network Requests
1. Press `F12`
2. Go to Network tab
3. See API calls to localhost:5178

---

## Next Steps

See **WEEK1_COMPLETE.md** for:
- ✅ What we've built
- 🎯 What's next
- 📊 Progress tracking
- 💡 Testing guide

See **REACT_IMPLEMENTATION_PLAN.md** for:
- 📋 Complete 4-week plan
- 🏗️ Architecture details
- 📱 Electron & Capacitor setup
- 🎨 UI component examples
