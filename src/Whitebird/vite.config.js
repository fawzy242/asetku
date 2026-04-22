import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const isDev = mode === 'development';
  
  return {
    plugins: [react()],
    root: process.cwd(),
    publicDir: 'public',
    server: {
      port: 3000,
      strictPort: true,
      open: isDev,
      proxy: {
        '/api': {
          target: env.VITE_BACKEND_URL || 'https://localhost:5001',
          changeOrigin: true,
          secure: false,
        },
        '/health': {
          target: env.VITE_BACKEND_URL || 'https://localhost:5001',
          changeOrigin: true,
          secure: false,
        },
        '/swagger': {
          target: env.VITE_BACKEND_URL || 'https://localhost:5001',
          changeOrigin: true,
          secure: false,
        }
      }
    },
    build: {
      outDir: 'wwwroot',
      emptyOutDir: true,
      sourcemap: isDev,
      minify: !isDev,
      rollupOptions: {
        input: {
          main: resolve(__dirname, 'index.html'),
        },
        output: {
          manualChunks: {
            vendor: ['react', 'react-dom', 'react-router-dom'],
            mui: ['@mui/material', '@mui/icons-material', '@mui/x-data-grid'],
            charts: ['chart.js', 'react-chartjs-2']
          }
        }
      }
    },
    preview: {
      port: 4173,
      strictPort: true
    },
    define: {
      __APP_VERSION__: JSON.stringify(env.VITE_APP_VERSION || '1.0.0'),
      __BUILD_TIME__: JSON.stringify(new Date().toISOString()),
    }
  };
});