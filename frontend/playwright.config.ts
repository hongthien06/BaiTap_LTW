import { defineConfig, devices } from '@playwright/test'

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:5173'

export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  expect: { timeout: 7_000 },
  // Chay tuan tu: cac test dong vao cung mot database va dinh rate limit neu chay song song.
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list']],
  use: {
    baseURL: BASE_URL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    // Desktop chay het moi luong.
    { name: 'desktop', use: { ...devices['Desktop Chrome'] } },
    // Mobile 360px chi chay test responsive (AC-6); khong lap lai luong dat hang.
    {
      name: 'mobile-360',
      testMatch: /responsive\.spec\.ts/,
      use: { ...devices['Desktop Chrome'], viewport: { width: 360, height: 740 } },
    },
  ],
})
