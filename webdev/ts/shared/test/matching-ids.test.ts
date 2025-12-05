// test/id-matching.test.ts
import { describe, it, expect } from 'vitest';
import { extractIdsFromHtml, extractJQueryIdsFromTs } from './utils';
import * as path from 'path';
import { fileURLToPath } from 'url';

// Get the directory of the current test file
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Resolve paths relative to the test file
const htmlPath = path.resolve(__dirname, '../../../base/shared/index.html');
const tsPath = path.resolve(__dirname, '../str8ts.ts');

describe('ID matching between HTML and TypeScript', () => {
  it('should have all jQuery IDs present in the HTML', () => {
    const htmlIds = extractIdsFromHtml(htmlPath);
    const tsIds = extractJQueryIdsFromTs(tsPath);

    const missingIds = [...tsIds].filter((id) => !htmlIds.has(id));

    expect(
      missingIds,
      `Missing IDs in index.html: ${missingIds.join(', ')}`
    ).toEqual([]);
  });
});
