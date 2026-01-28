import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'edge-runtime',
    setupFiles: ['./webdev/ts/test/vitest.setup.ts'],
  },
});
