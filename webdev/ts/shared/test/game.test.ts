// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import * as vt from 'vitest';
import * as Str8ts from '../game';

vt.describe('Game', () => {
  vt.describe('restoreStateAsync', () => {
    vt.it('should roundtrip with dumpState', async () => {
      const dumped = {
          check_count: 2,
          hint_count: 1,
          data: {
            gameState:
              'AAIAAAAAAABYsBMAAIAHAACAAoAAAJgAAAAAMADgoQUAAD4AAADgAQAAAD4AAAAAAIAPAEAAAADgAQAA',
            checkerBoard: 'QEUChANRACkUQAA=',
            size: 9,
            created: '2025-12-27T20:45:25.053Z',
            percentSolved: 23,
          },
        };
        
       const game = new Str8ts.Game({}, false);
      const result = await encoder.encodeAsync(1, []);
      vt.expect(result.base64Data).toBe('AA'); // Single flag byte (0 = not gzipped)
      vt.expect(result.count).toBe(0);
    });
  });
});
