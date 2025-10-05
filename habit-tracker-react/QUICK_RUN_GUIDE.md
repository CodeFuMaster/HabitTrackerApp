# 🚀 HabitTracker - Quick Run Guide

## 📍 Current Directory
Make sure you're in the right folder:
```bash
cd C:\Repo\HabitTrackerApp\HabitTrackerApp\habit-tracker-react
```

---

## 🌐 Web (Browser)

### Development Mode
```bash
npm run dev
```
Open: http://localhost:5173

### Production Build
```bash
npm run build
npm run preview
```

---

## 💻 Desktop (Electron)

### Development Mode
```bash
npm run electron:dev
```
- Starts web server
- Opens Electron window
- Hot reload enabled

### Production Build

#### Windows:
```bash
npm run electron:build:win
```
Output: `dist-electron/win/HabitTracker Setup.exe`

#### macOS:
```bash
npm run electron:build:mac
```
Output: `dist-electron/mac/HabitTracker.dmg`

#### Linux:
```bash
npm run electron:build:linux
```
Output: `dist-electron/HabitTracker.AppImage`

---

## 📱 Mobile

### Android

#### Development (Live Reload):
```bash
npm run android:dev
```
- Opens Android Studio
- Select device/emulator
- Click "Run" (green triangle)
- Edit code → see changes instantly

#### Open Android Studio:
```bash
npm run mobile:android
```

#### Manual Sync:
```bash
npm run mobile:build
```

### iOS (macOS only)

#### Development (Live Reload):
```bash
npm run ios:dev
```
- Opens Xcode
- Select device/simulator
- Click "Run" (▶️)
- Edit code → see changes instantly

#### Open Xcode:
```bash
npm run mobile:ios
```

---

## 🔧 Useful Commands

```bash
# Install dependencies
npm install

# Clean rebuild
rm -rf node_modules dist dist-electron android/build ios/build
npm install
npm run build

# Sync mobile platforms
npx cap sync

# Check Capacitor config
npx cap doctor

# View Android devices
adb devices

# View logs (Android)
adb logcat *:E

# Run TypeScript check
npx tsc --noEmit

# View all scripts
npm run
```

---

## 🐛 Quick Troubleshooting

### App won't start?
```bash
npm install
npm run build
```

### Changes not showing? (Mobile)
```bash
npm run mobile:build
```

### Android Studio issues?
```bash
cd android
./gradlew clean
cd ..
npm run mobile:build
```

### TypeScript errors?
```bash
npx tsc --noEmit
# Fix errors shown, then:
npm run build
```

---

## 📁 Key Files

- `package.json` - All npm scripts
- `vite.config.ts` - Vite configuration
- `capacitor.config.ts` - Capacitor configuration
- `electron/main.js` - Electron main process
- `src/main.tsx` - React entry point

---

## 🎯 Common Tasks

### Add a new npm package:
```bash
npm install package-name
```

### Update all packages:
```bash
npm update
```

### Build for all platforms:
```bash
npm run build                    # Web
npm run electron:build:win       # Windows
npm run electron:build:mac       # macOS
npm run electron:build:linux     # Linux
npm run mobile:build             # Android + iOS
```

### Test notifications:
1. Run app: `npm run dev`
2. Go to Today view
3. Click notification bell
4. Set a reminder for 1 minute from now
5. Wait for notification!

---

## 📚 Documentation

- `README.md` - Project overview
- `MOBILE_QUICK_START.md` - Mobile setup guide
- `MOBILE_IMPLEMENTATION_COMPLETE.md` - Complete mobile docs
- `SESSION_COMPLETE_SUMMARY.md` - Everything we built

---

## 🚀 Ready to Ship?

### Web:
1. `npm run build`
2. Upload `dist/` folder to:
   - Vercel
   - Netlify
   - GitHub Pages
   - Any static host

### Desktop:
1. Build for your platform
2. Distribute installer
3. Or publish to app stores

### Mobile:
1. `npm run mobile:build`
2. `npm run mobile:android` → Build signed APK
3. Upload to Google Play Store
4. (iOS) `npm run mobile:ios` → Archive → Upload to App Store

---

## 💡 Tips

- **Web Dev**: Use `npm run dev` for fast hot reload
- **Desktop Dev**: Use `npm run electron:dev` for full features
- **Mobile Dev**: Use `android:dev` or `ios:dev` for live reload
- **Production**: Always `npm run build` first!
- **Testing**: Test on real devices, not just emulators
- **Debugging**: Use browser DevTools (F12) for web/desktop

---

## 🎉 That's It!

You're ready to run your HabitTracker on any platform! 🚀

**Quick Start**: `npm run dev` → Open http://localhost:5173

**Need Help?** Check the documentation files or the code comments.

---

*HabitTracker - Track habits everywhere! 📱💻🌐*
