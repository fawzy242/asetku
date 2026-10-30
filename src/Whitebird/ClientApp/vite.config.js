import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "VITE_");
  const isDev = mode === "development";
  const isProd = mode === "production";

  // Default backend URL for the dev proxy. Can be overridden in .env via
  // VITE_BACKEND_URL if the backend runs elsewhere.
  const backendUrl = env.VITE_BACKEND_URL || "https://localhost:5001";

  // Version: base value from .env, "-dev" suffix only in development.
  const baseVersion = env.VITE_APP_VERSION || "1.0.0";
  const appVersion = isDev ? `${baseVersion}-dev` : baseVersion;

  // Debug logs: always ON in development, always OFF in production.
  // Overriding import.meta.env.VITE_ENABLE_DEBUG_LOGS via define means the
  // frontend can keep reading it from import.meta.env without any code
  // change — the value is injected at build time.
  const enableDebugLogs = isDev;

  return {
    plugins: [react()],
    root: process.cwd(),
    publicDir: "public",
    base: "/",

    server: {
      port: 3000,
      strictPort: true,
      open: isDev,
      proxy: isDev
        ? {
            "/api": { target: backendUrl, changeOrigin: true, secure: false },
            "/health": { target: backendUrl, changeOrigin: true, secure: false },
            "/swagger": { target: backendUrl, changeOrigin: true, secure: false },
          }
        : undefined,
    },

    build: {
      outDir: "../wwwroot",
      emptyOutDir: true,
      sourcemap: false,
      minify: isProd ? "esbuild" : false,
      chunkSizeWarningLimit: 1000,
      target: "es2020",
      rollupOptions: {
        input: { main: resolve(process.cwd(), "index.html") },
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

    preview: { port: 4173, strictPort: true },

    define: {
      // Version injected as a compile-time global
      __APP_VERSION__: JSON.stringify(appVersion),

      // Debug flag: injected via define so frontend code reading
      // `import.meta.env.VITE_ENABLE_DEBUG_LOGS` gets the mode-derived
      // value without any source changes.
      "import.meta.env.VITE_ENABLE_DEBUG_LOGS": JSON.stringify(enableDebugLogs ? "true" : "false"),

      // Consistent mirror as a global for any new code
      __DEBUG_LOGS__: JSON.stringify(enableDebugLogs),
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