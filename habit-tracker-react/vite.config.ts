import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  base: './', // Important for Capacitor mobile apps
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks: undefined // Single bundle for better mobile performance
      }
    }
  },
  server: {
    port: 5173,
    strictPort: false,
    host: true, // Allow external connections for mobile live reload
  },
})
