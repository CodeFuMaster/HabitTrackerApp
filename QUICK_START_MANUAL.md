# 🚀 **Habit Tracker App - Quick Start Manual**

## **Prerequisites**
- .NET 9.0 SDK installed
- Android Studio (for Android emulator)
- Visual Studio Code or Visual Studio

---

## **🖥️ Step 1: Start Backend API**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp.API"
dotnet run
```
✅ **Result**: API runs on `http://localhost:5188`

---

## **🌐 Step 2: Start Web Application**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp"
dotnet run
```
✅ **Result**: Web app runs on `http://localhost:5178`
📱 **Access**: Open browser → `http://localhost:5178`

---

## **💻 Step 3: Start Desktop Application**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp.MAUI"
dotnet run -f net9.0-windows10.0.19041.0
```
✅ **Result**: Windows desktop app launches automatically

---

## **📱 Step 4: Start Android Application**

### **Option A: Build APK and Install**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp.MAUI"
dotnet publish -f net9.0-android
```
✅ **Result**: APK created at `bin\Release\net9.0-android\com.companyname.habittrackerapp.maui-Signed.apk`

**Install APK:**
1. Start Android emulator in Android Studio
2. Drag APK file to emulator screen
3. App installs and runs automatically

### **Option B: Use Android Studio**
1. Open Android Studio
2. Open the MAUI project folder
3. Click green **Run** button ▶️
4. Select Android emulator
5. App builds and installs automatically

---

## **⚡ Quick All-Platforms Start**

**Terminal 1 - API:**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp.API" && dotnet run
```

**Terminal 2 - Web:**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp" && dotnet run
```

**Terminal 3 - Desktop:**
```powershell
cd "C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp.MAUI" && dotnet run -f net9.0-windows10.0.19041.0
```

**Android:**
- Use Android Studio or drag APK to emulator

---

## **📋 Access Points**

| Platform | Access Method |
|----------|---------------|
| **Backend API** | `http://localhost:5188` |
| **Web App** | `http://localhost:5178` |
| **Desktop** | Launches automatically |
| **Android** | Install APK on emulator/device |

---

## **🔧 Troubleshooting**

**Database Error "no such table":**
- ✅ Already fixed! Database auto-creates with sample data

**Build Errors:**
```powershell
dotnet clean
dotnet build
```

**Android APK Not Found:**
```powershell
dotnet publish -f net9.0-android -c Release
```

**Port Already in Use:**
- Check running processes: `netstat -ano | Select-String ":5178|:5188"`
- Kill process or restart terminal

---

## **🎯 That's It!**
Your Habit Tracker is now running on all platforms! 🎉

**Sample Data Included:**
- Health: Drink Water, Exercise
- Learning: Read Books
- Productivity: Plan Daily Tasks

---

## **📁 Project Structure**
```
HabitTrackerApp/
├── HabitTrackerApp.API/          # Backend REST API
├── HabitTrackerApp/              # Web Application (MVC)
├── HabitTrackerApp.MAUI/         # Cross-platform Desktop & Mobile
├── HabitTrackerApp.Core/         # Shared Business Logic
└── QUICK_START_MANUAL.md         # This file
```

## **🛠️ Development Tips**

**Hot Reload:**
- Web & API: Automatic hot reload on file changes
- Desktop: Restart app to see changes
- Android: Rebuild and reinstall APK

**Debugging:**
- Use Visual Studio or VS Code debugger
- Check browser console for web app issues
- Use Android Studio logcat for Android debugging

**Database Location:**
- Web/API: `habittracker.db` in project directory
- Desktop/Mobile: App data directory (auto-created)

---

*Happy habit tracking! 📈*