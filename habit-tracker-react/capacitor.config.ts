import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.habittracker.app',
  appName: 'HabitTracker',
  webDir: 'dist',
  server: {
    androidScheme: 'https',
    // For development with live reload
    // url: 'http://YOUR_IP:5173',
    // cleartext: true
  },
  plugins: {
    SplashScreen: {
      launchShowDuration: 2000,
      launchAutoHide: true,
      backgroundColor: "#6366F1",
      androidSplashResourceName: "splash",
      androidScaleType: "CENTER_CROP",
      showSpinner: true,
      androidSpinnerStyle: "large",
      iosSpinnerStyle: "small",
      spinnerColor: "#ffffff",
      splashFullScreen: true,
      splashImmersive: true
    },
    StatusBar: {
      style: "LIGHT",
      backgroundColor: "#6366F1"
    },
    LocalNotifications: {
      smallIcon: "ic_stat_icon_config_sample",
      iconColor: "#6366F1",
      sound: "beep.wav"
    }
  }
};

export default config;
