# 📱 **Android Deployment Checklist**

## ✅ **Ready to Deploy!** 

Your Habit Tracker MAUI app is configured and ready for Android deployment.

---

## 🚀 **Quick Start Deployment**

### **Option 1: Visual Studio (Recommended)**
1. Open `HabitTrackerMobile.sln` in Visual Studio 2022
2. Select **Android** from target dropdown
3. Choose your Android device or emulator
4. Click **Debug** button or press **F5**
5. App will build and deploy automatically

### **Option 2: VS Code Tasks (Current Setup)**
```bash
# Use Command Palette (Ctrl+Shift+P)
Tasks: Run Task → "Deploy to Android Device"

# Or use terminal
dotnet run -f net9.0-android
```

### **Option 3: Command Line**
```bash
# Build for Android
dotnet build -f net9.0-android -c Debug

# Build Release APK for distribution
dotnet publish -f net9.0-android -c Release
```

---

## 📋 **Pre-Deployment Checklist**

### ✅ **Environment Setup**
- [x] **Android SDK** - Installed via Visual Studio
- [x] **MAUI Workload** - Installed (.NET MAUI Extension)
- [x] **Project Configuration** - All packages restored

### ✅ **Device Preparation**

**For Physical Android Device:**
- [ ] Enable **Developer Options** (Settings → About → Tap Build Number 7x)
- [ ] Enable **USB Debugging** (Developer Options → USB Debugging)
- [ ] Connect device via USB
- [ ] Accept USB debugging prompt on device

**For Android Emulator:**
- [ ] Create Android Virtual Device (AVD) in Android Studio or Visual Studio
- [ ] Start emulator before deploying

### ✅ **Network Configuration**
- [x] **API URL Configured** - Using your computer's IP (192.168.1.103:5178)
- [ ] **ASP.NET Core API Running** - Start your backend API first
- [ ] **Firewall Rules** - Ensure Windows Firewall allows API access

---

## 🌐 **API Connection Setup**

### **1. Start Your Backend API**
```bash
# Navigate to your ASP.NET Core project directory
cd C:\Repo\HabitTrackerApp\HabitTrackerApp\HabitTrackerApp

# Run the API
dotnet run
```

### **2. Verify API is Accessible**
```bash
# Test from another computer/phone on same network
curl http://192.168.1.103:5178/api/enhanced/test-gym-session

# Or open in browser
http://192.168.1.103:5178
```

### **3. Configure Windows Firewall (if needed)**
```powershell
# Allow .NET Core through firewall
netsh advfirewall firewall add rule name="ASP.NET Core API" dir=in action=allow protocol=TCP localport=5178
```

---

## 🔧 **Available Build Tasks**

| Task Name | Purpose | Command |
|-----------|---------|---------|
| **"Deploy to Android Device"** | Build and deploy to connected device | `dotnet run -f net9.0-android` |
| **"Build Android Debug"** | Quick debug build | `dotnet build -f net9.0-android -c Debug` |
| **"Build Android Release APK"** | Production APK build | `dotnet publish -f net9.0-android -c Release` |
| **"Build MAUI App"** | All platforms | `dotnet build` |

---

## 📲 **Deployment Methods**

### **Method 1: Direct Deployment (Development)**
- **Best For**: Testing and development
- **Process**: Connect device → Run task → App installs automatically
- **Time**: ~30 seconds

### **Method 2: APK Distribution**
- **Best For**: Sharing with testers
- **Process**: Build Release APK → Transfer file → Install manually
- **Location**: `bin\Release\net9.0-android\publish\*.apk`

### **Method 3: Google Play Store**
- **Best For**: Public distribution
- **Process**: Build AAB → Upload to Play Console → Store approval
- **Requirements**: Developer account ($25 one-time fee)

---

## 🎯 **Test Your Deployment**

### **1. Basic Functionality Test**
- [ ] App launches successfully
- [ ] Main dashboard loads
- [ ] Can create a new habit
- [ ] Can mark habits as complete

### **2. API Integration Test**
- [ ] App connects to your API (online indicator shows green)
- [ ] Can sync data with backend
- [ ] Test endpoints work (`/api/enhanced/test-gym-session`)

### **3. Offline Functionality Test**
- [ ] Disconnect WiFi/mobile data
- [ ] App still functions (shows offline indicator)
- [ ] Can create/edit habits offline
- [ ] Data syncs when reconnected

---

## 📁 **File Locations**

```
📦 Build Outputs
├── 🗂️ bin/Debug/net9.0-android/          # Debug builds
│   └── HabitTrackerMobile.dll
├── 🗂️ bin/Release/net9.0-android/        # Release builds  
│   └── publish/
│       ├── com.habittracker.mobile.apk   # Installable APK
│       └── com.habittracker.mobile.aab   # Play Store bundle
└── 🗂️ obj/                               # Intermediate files
```

---

## 🆘 **Troubleshooting**

### **"Cannot connect to API"**
```bash
# 1. Check API is running
curl http://192.168.1.103:5178/api/health

# 2. Check device is on same network
ping 192.168.1.103

# 3. Update IP in MauiProgram.cs if changed
```

### **"Device not detected"**
```bash
# 1. Check ADB connection
adb devices

# 2. Restart ADB server
adb kill-server
adb start-server

# 3. Enable USB Debugging on device again
```

### **"Build fails"**
```bash
# 1. Clean and restore
dotnet clean
dotnet restore

# 2. Check Android SDK path
# 3. Restart Visual Studio
```

---

## 🎉 **Ready to Deploy!**

Your Habit Tracker mobile app is fully configured for Android deployment:

**✅ Cross-platform architecture ready**  
**✅ Offline-first database configured**  
**✅ API integration with proper network settings**  
**✅ Build tasks created for easy deployment**  
**✅ Comprehensive documentation provided**

**Next Step**: Choose your deployment method and start testing your habit tracker on Android! 🚀

---

**💡 Pro Tip**: Start with Visual Studio deployment for the easiest experience, then move to command-line builds for automation.