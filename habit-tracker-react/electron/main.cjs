const { app, BrowserWindow, Tray, Menu, nativeImage, ipcMain, Notification } = require('electron');
const path = require('path');
const isDev = process.env.NODE_ENV === 'development';

let mainWindow = null;
let tray = null;
let isQuitting = false;

// Enable auto-launch (Windows)
if (process.platform === 'win32') {
  app.setLoginItemSettings({
    openAtLogin: true,
    path: process.execPath,
  });
}

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    icon: path.join(__dirname, '../public/icon.png'),
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.cjs'),
    },
    show: false, // Don't show until ready
  });

  // Load the app
  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    mainWindow.webContents.openDevTools();
  } else {
    mainWindow.loadFile(path.join(__dirname, '../dist/index.html'));
  }

  // Show window when ready
  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
  });

  // Handle window close - minimize to tray instead
  mainWindow.on('close', (event) => {
    if (!isQuitting) {
      event.preventDefault();
      mainWindow.hide();
      
      // Show notification on first minimize
      if (process.platform === 'win32' || process.platform === 'darwin') {
        new Notification({
          title: 'HabitTracker',
          body: 'App is still running in the system tray',
          icon: path.join(__dirname, '../public/icon.png'),
        }).show();
      }
    }
    return false;
  });

  // Clean up on closed
  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

function createTray() {
  // Create tray icon
  const iconPath = path.join(__dirname, '../public/tray-icon.png');
  let trayIcon;
  
  try {
    trayIcon = nativeImage.createFromPath(iconPath);
    // Resize for better display
    if (process.platform === 'win32') {
      trayIcon = trayIcon.resize({ width: 16, height: 16 });
    }
  } catch (error) {
    console.error('Failed to load tray icon:', error);
    trayIcon = nativeImage.createEmpty();
  }

  tray = new Tray(trayIcon);
  tray.setToolTip('HabitTracker - Your Daily Habits');

  // Create context menu
  const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Show HabitTracker',
      click: () => {
        if (mainWindow) {
          mainWindow.show();
          mainWindow.focus();
        }
      },
    },
    {
      label: 'Today\'s Habits',
      click: () => {
        if (mainWindow) {
          mainWindow.show();
          mainWindow.focus();
          mainWindow.webContents.send('navigate', '/today');
        }
      },
    },
    {
      type: 'separator',
    },
    {
      label: 'Statistics',
      click: () => {
        if (mainWindow) {
          mainWindow.show();
          mainWindow.focus();
          mainWindow.webContents.send('navigate', '/stats');
        }
      },
    },
    {
      type: 'separator',
    },
    {
      label: 'Quit',
      click: () => {
        isQuitting = true;
        app.quit();
      },
    },
  ]);

  tray.setContextMenu(contextMenu);

  // Double-click to show window
  tray.on('double-click', () => {
    if (mainWindow) {
      mainWindow.show();
      mainWindow.focus();
    }
  });
}

// Handle IPC messages from renderer
ipcMain.on('show-notification', (event, { title, body, icon }) => {
  if (Notification.isSupported()) {
    const notification = new Notification({
      title,
      body,
      icon: icon || path.join(__dirname, '../public/icon.png'),
    });
    
    notification.on('click', () => {
      if (mainWindow) {
        mainWindow.show();
        mainWindow.focus();
      }
    });
    
    notification.show();
  }
});

ipcMain.on('app-quit', () => {
  isQuitting = true;
  app.quit();
});

// App lifecycle
app.whenReady().then(() => {
  createWindow();
  createTray();
});

app.on('window-all-closed', () => {
  // On macOS, keep app running even if all windows are closed
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On macOS, re-create window when dock icon is clicked
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  } else if (mainWindow) {
    mainWindow.show();
  }
});

app.on('before-quit', () => {
  isQuitting = true;
});

// Handle app updates (optional)
if (!isDev) {
  // Auto-updater can be added here
  // require('electron-updater').autoUpdater.checkForUpdatesAndNotify();
}
