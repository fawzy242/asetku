import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "VITE_");
  const isDev = mode === "development";
  const isProd = mode === "production";

  // PERBAIKAN: Jangan pakai fallback ke localhost:5001
  // Biarkan kosong untuk production agar pakai relative path
  const backendUrl = env.VITE_API_BASE_URL;
  
  // Hapus /api dari URL untuk proxy (karena proxy sudah pakai /api path)
  const proxyTarget = backendUrl ? backendUrl.replace(/\/api$/, "") : "";

  return {
    plugins: [react()],
    root: process.cwd(),
    publicDir: "public",
    base: "/",

    server: {
      port: 3000,
      strictPort: true,
      open: isDev,
      proxy: backendUrl ? {
        "/api": {
          target: proxyTarget,
          changeOrigin: true,
          secure: false,
        },
        "/health": {
          target: proxyTarget,
          changeOrigin: true,
          secure: false,
        },
        "/swagger": {
          target: proxyTarget,
          changeOrigin: true,
          secure: false,
        },
      } : {}, // Proxy hanya aktif jika ada backendUrl (development)
    },

    build: {
      outDir: "../wwwroot",
      emptyOutDir: true,
      sourcemap: isDev,
      minify: isProd ? "esbuild" : false,
      chunkSizeWarningLimit: 1000,
      target: "es2020",
      rollupOptions: {
        input: {
          main: resolve(process.cwd(), "index.html"),
        },
        output: {
          manualChunks: {
            "react-vendor": ["react", "react-dom", "react-router-dom"],
            "mui-vendor": ["@mui/material", "@mui/x-data-grid", "@mui/x-date-pickers"],
            "chart-vendor": ["chart.js", "react-chartjs-2"],
            "utils-vendor": ["axios", "dayjs", "zustand", "@tanstack/react-query"],
          },
          chunkFileNames: isProd ? "assets/[name].[hash].js" : "assets/[name].js",
          entryFileNames: isProd ? "assets/[name].[hash].js" : "assets/[name].js",
          assetFileNames: isProd ? "assets/[name].[hash].[ext]" : "assets/[name].[ext]",
        },
      },
    },

    preview: {
      port: 4173,
      strictPort: true,
    },

    define: {
      __APP_VERSION__: JSON.stringify(env.VITE_APP_VERSION || "1.0.0"),
    },

    optimizeDeps: {
      include: ["react", "react-dom", "react-router-dom", "@mui/material", "axios", "dayjs"],
    },

    esbuild: {
      logOverride: { "this-is-undefined-in-esm": "silent" },
      drop: isProd ? ["console", "debugger"] : [],
    },
  };
});