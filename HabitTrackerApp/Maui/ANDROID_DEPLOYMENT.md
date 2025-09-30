# Android Deployment Guide

## 📱 **Deploying Habit Tracker to Android Devices**

### **Prerequisites**

1. **Android SDK** - Install via Visual Studio Installer
2. **Android Device** or **Android Emulator**
3. **Developer Options Enabled** on physical device
4. **USB Debugging Enabled** on physical device

---

## 🚀 **Deployment Methods**

### **Method 1: Direct Debug Deployment (Recommended for Testing)**

**Step 1: Enable Developer Options on Android Device**
```
Settings → About Phone → Tap "Build Number" 7 times
Settings → Developer Options → Enable "USB Debugging"
```

**Step 2: Connect Device and Deploy**
```bash
# Build for Android Debug
dotnet build -f net9.0-android -c Debug

# Deploy to connected device
dotnet build -f net9.0-android -c Debug -p:AndroidSdkDirectory="C:\Program Files (x86)\Android\android-sdk"
```

**Step 3: Install via Visual Studio**
- Open project in Visual Studio 2022
- Select Android target framework
- Choose your connected device or emulator
- Click "Debug" or press F5

---

### **Method 2: APK File Distribution**

**Step 1: Build Release APK**
```bash
# Build signed APK for distribution
dotnet publish -f net9.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=myapp.keystore -p:AndroidSigningKeyAlias=mykey -p:AndroidSigningKeyPass=mypassword -p:AndroidSigningStorePass=mystorepassword
```

**Step 2: Locate APK File**
```
bin\Release\net9.0-android\publish\com.habittracker.mobile-Signed.apk
```

**Step 3: Install APK on Device**
```bash
# Using ADB (Android Debug Bridge)
adb install com.habittracker.mobile-Signed.apk

# Or transfer APK to device and install manually
```

---

### **Method 3: Android App Bundle (AAB) for Google Play Store**

**Step 1: Create App Bundle**
```bash
# Build AAB for Play Store
dotnet publish -f net9.0-android -c Release -p:AndroidPackageFormat=aab
```

**Step 2: Upload to Play Console**
- Create developer account on Google Play Console
- Upload the .aab file from `bin\Release\net9.0-android\publish\`
- Complete Play Store listing and review process

---

## 🔧 **Android-Specific Configuration**

### **Update Android Manifest**
```xml
<!-- Platforms/Android/AndroidManifest.xml -->
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    
    <!-- Internet permission for API sync -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    
    <!-- Notification permissions for habit reminders -->
    <uses-permission android:name="android.permission.VIBRATE" />
    <uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />
    
    <!-- Background sync permissions -->
    <uses-permission android:name="android.permission.WAKE_LOCK" />
    
    <application android:allowBackup="true" android:theme="@style/Maui.SplashTheme" android:icon="@mipmap/appicon">
        <activity android:name="crc64e1fb321c08285b90.MainActivity" android:exported="true" android:launchMode="singleTop" android:theme="@style/Maui.SplashTheme">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
```

### **Configure Network Security (for local API)**
```xml
<!-- Platforms/Android/Resources/xml/network_security_config.xml -->
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <domain-config cleartextTrafficPermitted="true">
        <domain includeSubdomains="true">localhost</domain>
        <domain includeSubdomains="true">10.0.2.2</domain> <!-- Android emulator host -->
        <domain includeSubdomains="true">192.168.1.0/24</domain> <!-- Local network -->
    </domain-config>
</network-security-config>
```

---

## 🏗️ **Build Tasks (Already Created)**

### **Available Tasks:**
1. **"Build Android Debug"** - Quick debug build
2. **"Build Android Release APK"** - Production APK build
3. **"Build MAUI App"** - All platforms build

### **Run Tasks:**
```bash
# In VS Code Command Palette (Ctrl+Shift+P)
Tasks: Run Task → Select desired Android build task

# Or via Terminal
dotnet build -f net9.0-android -c Debug    # Debug
dotnet publish -f net9.0-android -c Release # Release APK
```

---

## 📲 **Testing Deployment**

### **1. Android Emulator (No Physical Device)**
```bash
# List available emulators
emulator -list-avds

# Start emulator
emulator -avd Pixel_7_API_34

# Deploy to emulator
dotnet build -f net9.0-android -c Debug
# App will auto-install on emulator
```

### **2. Physical Android Device**
```bash
# Check device connection
adb devices

# Should show:
# List of devices attached
# <device-id>    device

# Deploy directly
dotnet build -f net9.0-android -c Debug
# App will auto-install on connected device
```

---

## 🌐 **API Connection for Android**

### **Local Network API Access:**

**Option 1: Use Computer's IP Address**
```csharp
// In ApiService.cs, update base URL for Android
#if ANDROID
    client.BaseAddress = new Uri("http://192.168.1.100:5178/api/"); // Your computer's IP
#else
    client.BaseAddress = new Uri("http://localhost:5178/api/");
#endif
```

**Option 2: Android Emulator Special IP**
```csharp
// For Android Emulator specifically
#if ANDROID
    client.BaseAddress = new Uri("http://10.0.2.2:5178/api/"); // Emulator → Host mapping
#endif
```

**Option 3: Use ngrok for External Access**
```bash
# Install ngrok and expose local API
ngrok http 5178

# Use ngrok URL in app
# https://abc123.ngrok.io/api/
```

---

## 📦 **Distribution Options**

### **1. Direct APK Distribution**
- **Use Case**: Internal testing, enterprise distribution
- **Process**: Build APK → Transfer to device → Install
- **Pros**: No store approval needed
- **Cons**: Users must enable "Unknown Sources"

### **2. Google Play Store**
- **Use Case**: Public distribution
- **Process**: Build AAB → Upload to Play Console → Store review
- **Pros**: Wide distribution, automatic updates
- **Cons**: Store approval required, 30% fee on paid apps

### **3. Alternative App Stores**
- **Options**: Samsung Galaxy Store, Amazon Appstore, F-Droid
- **Process**: Similar to Google Play but different requirements

---

## 🔍 **Troubleshooting**

### **Common Issues:**

**1. "Unable to connect to API"**
```bash
# Check API is running
curl http://localhost:5178/api/health

# Update API URL for Android network access
# Use computer's IP instead of localhost
```

**2. "App not installing on device"**
```bash
# Enable Developer Options and USB Debugging
# Check ADB connection: adb devices
# Try: adb install -r app.apk
```

**3. "Build errors"**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build -f net9.0-android -c Debug
```

---

## 🎯 **Quick Start Commands**

```bash
# 1. Build Android Debug (fastest for testing)
dotnet build -f net9.0-android -c Debug

# 2. Build Release APK (for distribution)
dotnet publish -f net9.0-android -c Release

# 3. Check connected devices
adb devices

# 4. Install APK manually
adb install path/to/app.apk

# 5. View app logs
adb logcat | grep HabitTracker
```

Your Habit Tracker app is now ready for Android deployment! 🚀