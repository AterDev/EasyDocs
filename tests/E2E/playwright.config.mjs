import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const e2eRoot = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(e2eRoot, '..', '..');
const browserChannel = process.env.E2E_BROWSER_CHANNEL || 'msedge';

export default defineConfig({
  testDir: path.join(e2eRoot, 'tests'),
  outputDir: path.join(e2eRoot, 'test-results'),
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI ? [['line'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: 'http://127.0.0.1:4173/e2e/',
    ...devices['Desktop Chrome'],
    channel: browserChannel,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'off'
  },
  webServer: {
    command: `node "${path.join(e2eRoot, 'site-server.mjs')}"`,
    cwd: repoRoot,
    url: 'http://127.0.0.1:4173/e2e/',
    reuseExistingServer: false,
    timeout: 120_000,
    env: {
      E2E_BASE_HREF: '/e2e/',
      E2E_PORT: '4173',
      E2E_CLI_DLL: process.env.E2E_CLI_DLL ?? ''
    }
  }
});
