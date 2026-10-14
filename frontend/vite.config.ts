import path from 'node:path'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// API address: under Aspire it comes via service discovery, without Aspire it is the port from launchSettings.json.
const apiTarget =
  process.env.services__api__https__0 ??
  process.env.services__api__http__0 ??
  'http://localhost:5258'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    proxy: {
      '/api': { target: apiTarget, changeOrigin: true, secure: false },
    },
  },
})
