# 🖥️ HabitTracker Desktop App (Electron)

## Overview

HabitTracker is now available as a native desktop application! Built with Electron, it provides a seamless desktop experience with system tray integration, native notifications, and auto-launch capabilities.

---

## 🚀 Quick Start

### Development Mode

Run the app in development mode with hot-reload:

```bash
npm run electron:dev
```

This will:
1. Start Vite dev server on port 5173
2. Wait for the server to be ready
3. Launch Electron with the app

### Production Build

Build the app for your operating system:

#### Windows
```bash
npm run electron:build:win
```
Creates: `dist-electron/HabitTracker Setup.exe` and portable version

#### macOS
```bash
npm run electron:build:mac
```
Creates: `dist-electron/HabitTracker.dmg`

#### Linux
```bash
npm run electron:build:linux
```
Creates: `dist-electron/HabitTracker.AppImage` and `.deb` package

#### All Platforms
```bash
npm run electron:build
```

---

## ✨ Desktop Features

### System Tray Integration
- **Always accessible** - App runs in system tray
- **Quick actions** - Right-click for menu
- **Show/Hide** - Double-click to toggle window
- **Minimize to tray** - Window hides instead of closing

### Native Notifications
- **System notifications** - Uses OS-native notification system
- **Better visibility** - Appears even when app is minimized
- **Click to focus** - Clicking notification brings app to front

### Auto-Launch
- **Starts with system** - Launches automatically on login
- **Background running** - Starts minimized to tray
- **Always ready** - Never miss a habit reminder

### Desktop Experience
- **Native window** - Full desktop window controls
- **Keyboard shortcuts** - Standard desktop shortcuts
- **Multi-window support** - Can open multiple instances
- **Offline-first** - Works without internet

---

## 📁 Project Structure

```
habit-tracker-react/
├── electron/
│   ├── main.js          ← Main process (Node.js)
│   └── preload.js       ← Preload script (bridge)
├── src/
│   ├── services/
│   │   ├── electronService.ts     ← Electron integration
│   │   └── notificationService.ts ← Updated for Electron
│   └── electron.d.ts    ← TypeScript declarations
├── public/
│   ├── icon.png         ← App icon (256x256)
│   └── tray-icon.png    ← Tray icon (16x16)
└── package.json         ← Electron config
```

---

## 🎯 System Tray Menu

When you right-click the tray icon, you get:

```
┌─────────────────────┐
│ Show HabitTracker   │  ← Focus main window
├─────────────────────┤
│ Today's Habits      │  ← Jump to Today page
├─────────────────────┤
│ Statistics          │  ← Jump to Stats page
├─────────────────────┤
│ Quit                │  ← Exit app
└─────────────────────┘
```

---

## 🔔 Notification Behavior

### Web vs Desktop

| Feature | Web Browser | Desktop App |
|---------|-------------|-------------|
| **Notifications** | Browser API | Native OS |
| **Visibility** | Tab-specific | System-wide |
| **Persistence** | Auto-dismiss | Stays visible |
| **Sound** | Limited | Full system sound |
| **Background** | Tab must be open | Always running |

### Electron Notifications
- **Always visible** - Even when app is minimized
- **Action center** - Appear in Windows Action Center / macOS Notification Center
- **No permission** - No need to request permission
- **Click to focus** - Automatically brings app to foreground

---

## ⚙️ Configuration

### Auto-Launch

Edit `electron/main.js`:
```javascript
app.setLoginItemSettings({
  openAtLogin: true,  // Set to false to disable
  path: process.execPath,
});
```

### Window Settings

Edit `electron/main.js`:
```javascript
mainWindow = new BrowserWindow({
  width: 1200,        // Default width
  height: 800,        // Default height
  minWidth: 800,      // Minimum width
  minHeight: 600,     // Minimum height
});
```

### Build Configuration

Edit `package.json` → `build` section:
```json
{
  "build": {
    "appId": "com.habittracker.app",
    "productName": "HabitTracker",
    "win": {
      "target": ["nsis", "portable"]
    }
  }
}
```

---

## 📦 Installation

### For Users

#### Windows
1. Download `HabitTracker Setup.exe`
2. Run the installer
3. App launches and adds to Start Menu
4. Icon appears in system tray

#### macOS
1. Download `HabitTracker.dmg`
2. Open and drag to Applications
3. Launch from Applications folder
4. Grant permissions if prompted

#### Linux
```bash
# AppImage (no installation needed)
chmod +x HabitTracker.AppImage
./HabitTracker.AppImage

# Or install .deb package
sudo dpkg -i HabitTracker.deb
```

---

## 🐛 Troubleshooting

### App doesn't start
- Check if Node.js is installed: `node --version`
- Reinstall dependencies: `npm install`
- Clear cache: `npm run clean` (if script exists)

### Notifications not working
- On Windows: Check Windows Settings → Notifications
- On macOS: Check System Settings → Notifications
- On Linux: Ensure notification daemon is running

### Build fails
- Update electron-builder: `npm install -D electron-builder@latest`
- Check disk space (builds require ~500MB)
- Try building for specific platform only

### Tray icon not showing
- Icons must be in `public/` folder
- Windows: 16x16 PNG recommended
- macOS: Template image with transparency
- Linux: 22x22 PNG recommended

---

## 🔧 Development Tips

### Debug Main Process
```bash
# Run with dev tools
npm run electron:dev

# Main process logs appear in terminal
console.log('Main process:', data);
```

### Debug Renderer Process
- Open DevTools: `Ctrl+Shift+I` (Windows/Linux) or `Cmd+Option+I` (Mac)
- Console tab shows renderer logs
- Network tab for API calls

### Hot Reload
- Changes to React code → Auto-reload
- Changes to `electron/main.js` → Restart required
- Use `nodemon` for auto-restart (optional)

---

## 📊 Performance

### Bundle Size
- **Installer**: ~100-150 MB (includes Chromium + Node.js)
- **Installed**: ~200-250 MB
- **RAM Usage**: ~150-200 MB idle
- **Startup Time**: 2-3 seconds

### Optimization Tips
1. **Lazy load** - Split large components
2. **Minimize dependencies** - Remove unused packages
3. **Compress assets** - Optimize images
4. **Use production build** - `npm run electron:build`

---

## 🚀 Distribution

### Code Signing (Optional but Recommended)

#### Windows
1. Get code signing certificate
2. Add to `package.json`:
```json
{
  "build": {
    "win": {
      "certificateFile": "path/to/cert.pfx",
      "certificatePassword": "password"
    }
  }
}
```

#### macOS
1. Get Apple Developer certificate
2. Add to `package.json`:
```json
{
  "build": {
    "mac": {
      "identity": "Developer ID Application: Your Name"
    }
  }
}
```

### Auto-Updates (Future Enhancement)
```bash
npm install electron-updater
```

Then in `electron/main.js`:
```javascript
const { autoUpdater } = require('electron-updater');
autoUpdater.checkForUpdatesAndNotify();
```

---

## 📝 Scripts Reference

| Command | Description |
|---------|-------------|
| `npm run dev` | Web dev server only |
| `npm run electron` | Electron only (dev mode) |
| `npm run electron:dev` | Web + Electron together |
| `npm run electron:build` | Build for current OS |
| `npm run electron:build:win` | Build for Windows |
| `npm run electron:build:mac` | Build for macOS |
| `npm run electron:build:linux` | Build for Linux |

---

## 🎉 Success!

You now have a fully functional desktop app with:
- ✅ Native window
- ✅ System tray
- ✅ Native notifications
- ✅ Auto-launch
- ✅ Offline support
- ✅ Cross-platform

**Ready to distribute to users!** 🚀

---

*For more information, see: https://www.electronjs.org/docs/latest/*
