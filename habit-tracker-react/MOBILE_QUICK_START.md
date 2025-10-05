# 🚀 Quick Start - Mobile Development

## Prerequisites

### Required for Android:
- ✅ **Android Studio** (latest version)
- ✅ **JDK 17** or higher
- ✅ **Android SDK** (API 33+)
- ✅ **Android Emulator** or physical device

### Required for iOS (macOS only):
- ✅ **Xcode** (latest version)
- ✅ **CocoaPods** (`sudo gem install cocoapods`)
- ✅ **iOS Simulator** or physical device
- ✅ **Apple Developer Account** (for device testing)

---

## 🏃 Run Your First Mobile Build

### Option 1: Android Emulator (Fastest)

1. **Start Vite dev server** (in one terminal):
   ```bash
   cd habit-tracker-react
   npm run dev
   ```

2. **Run on Android** (in another terminal):
   ```bash
   npm run android:dev
   ```

3. **What happens**:
   - Android Studio opens automatically
   - Select an emulator (or create one if needed)
   - Click "Run" button (green triangle)
   - App launches with live reload!

4. **Make changes**:
   - Edit any `.tsx` file in `src/`
   - Save
   - See changes instantly on emulator 🔥

### Option 2: Android Physical Device

1. **Enable Developer Mode** on your phone:
   - Settings → About Phone
   - Tap "Build Number" 7 times
   - Go back → Developer Options
   - Enable "USB Debugging"

2. **Connect via USB** and run:
   ```bash
   npm run android:dev
   ```

3. **Authorize your computer** on the phone prompt

4. **App installs and launches** with live reload!

### Option 3: iOS Simulator (macOS only)

1. **Start Vite dev server**:
   ```bash
   npm run dev
   ```

2. **Run on iOS**:
   ```bash
   npm run ios:dev
   ```

3. **Xcode opens**:
   - Select simulator (iPhone 15, iPad Pro, etc.)
   - Click "Run" button (▶️)
   - App launches with live reload!

---

## 🎯 Common Commands

```bash
# Development (with live reload)
npm run android:dev          # Android with hot reload
npm run ios:dev              # iOS with hot reload (macOS)

# Production build & open IDE
npm run mobile:android       # Build + open Android Studio
npm run mobile:ios           # Build + open Xcode

# Manual sync (after changes)
npm run mobile:build         # Build React + sync to platforms
npx cap sync                 # Sync web assets only
npx cap sync android         # Sync Android only
npx cap sync ios             # Sync iOS only
```

---

## 📱 Test Native Features

Once your app is running on a device, test these mobile-specific features:

### 1. **Haptic Feedback**
- Mark a habit as complete
- Feel the vibration!

### 2. **Native Notifications**
- Create a habit
- Set a reminder
- Wait for notification (or change system time)
- Native notification appears!

### 3. **Status Bar**
- Notice the blue status bar
- Matches app theme

### 4. **Back Button** (Android)
- Press hardware back button
- App navigates correctly

### 5. **Offline Mode**
- Enable airplane mode
- App still works!
- Data syncs when online

---

## 🐛 Troubleshooting

### Android Studio won't install?
**Download**: https://developer.android.com/studio
- Install with default settings
- Let it download SDK on first launch

### Can't see device in Android Studio?
```bash
# Check device is connected
adb devices

# If empty, check USB debugging is enabled
# Or restart adb
adb kill-server
adb start-server
```

### Emulator is slow?
- Enable hardware acceleration in BIOS (VT-x/AMD-V)
- Or use a physical device (much faster!)

### "Command 'cap' not found"?
```bash
# Make sure you're in the right directory
cd habit-tracker-react

# Run with npx
npx cap --version
```

### iOS: "No provisioning profile"?
- You need an Apple Developer account ($99/year)
- Or use simulator instead (free)

### Changes not appearing?
```bash
# Rebuild and sync
npm run mobile:build

# Or hard refresh in Android Studio / Xcode
```

---

## 📚 Next Steps

1. ✅ **Run on emulator** - See your app on mobile!
2. ✅ **Test on physical device** - Real mobile experience
3. ✅ **Try live reload** - Edit code, see instant changes
4. ✅ **Test notifications** - Set reminders, test alerts
5. ✅ **Test offline mode** - Airplane mode test
6. ✅ **Build release APK** - Share with friends!

---

## 🎉 That's It!

You now have:
- ✅ Native Android app
- ✅ Live reload development
- ✅ All features working
- ✅ Ready to test on device

**Enjoy building! 🚀**

For detailed instructions, see: `MOBILE_IMPLEMENTATION_COMPLETE.md`
