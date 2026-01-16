// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import * as vt from 'vitest';
import * as Str8ts from '../game';
import { FieldModes } from '../gameReader';

const dummyRenderer = {
  renderField: (f: Str8ts.Field) => {},
};

const oldStyleData = {
  check_count: 2,
  hint_count: 15,
  data: [
    [
      { notes: [] },
      { notes: [3, 5, 6, 8, 9] },
      { notes: [] },
      { notes: [] },
      { notes: [2, 3, 5, 6, 8] },
      { notes: [2, 3, 6, 9] },
      { user: 5, notes: [2, 5] },
      { user: 2, notes: [2, 3, 5] },
      { notes: [8, 9] },
    ],
    [
      { notes: [4, 6] },
      { notes: [4, 6, 7] },
      { user: 5, notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { user: 4, notes: [1, 4] },
      { user: 3, notes: [2, 3] },
      { user: 2, notes: [2, 3] },
    ],
    [
      { notes: [4, 5, 6, 8, 9] },
      { notes: [3, 4, 5, 6, 8, 9] },
      { notes: [] },
      { notes: [] },
      { notes: [5, 8] },
      { notes: [6, 8, 9] },
      { notes: [] },
      { user: 4, notes: [2, 3, 4] },
      { user: 3, notes: [1, 2, 3, 4] },
    ],
    [
      { user: 1, notes: [1, 9] },
      { notes: [] },
      { notes: [8, 9] },
      { notes: [3, 8] },
      { notes: [4, 7] },
      { notes: [7, 8, 9] },
      { notes: [4, 7, 8, 9] },
      { notes: [] },
      { notes: [] },
    ],
    [
      { notes: [] },
      { notes: [] },
      { notes: [8, 9] },
      { notes: [] },
      { notes: [3, 6] },
      { notes: [7, 8, 9, 6] },
      { notes: [6, 7, 8, 9] },
      { user: 5, notes: [3, 5] },
      { notes: [8, 9] },
    ],
    [
      { notes: [4, 5, 8, 9] },
      { notes: [] },
      { user: 7, notes: [7, 9] },
      { user: 6, notes: [8, 6] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [1, 4, 8, 9] },
    ],
    [
      { notes: [8, 9] },
      { notes: [] },
      { user: 6, notes: [6] },
      { user: 5, notes: [3, 5, 6] },
      { notes: [2, 4] },
      { notes: [2, 4] },
      { user: 3, notes: [2, 3, 4] },
      { notes: [] },
      { notes: [] },
    ],
    [
      { user: 3, notes: [1, 3] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [] },
      { notes: [1, 4, 5] },
      { notes: [] },
      { notes: [1, 4, 8, 9] },
    ],
    [
      { notes: [4, 8] },
      { user: 1, notes: [] },
      { notes: [] },
      { notes: [2, 8] },
      { notes: [] },
      { notes: [] },
      { notes: [2, 4] },
      { user: 7, notes: [2, 4, 7, 8] },
      { notes: [] },
    ],
  ],
};

function gameToText(game: Str8ts.Game) {
  const result: string[] = [];
  result.push(`${game.size}\n`);
  for (let row = 0; row < game.size; row++) {
    const r = game.data[row];
    for (let col = 0; col < game.size; col++) {
      const f = r[col];
      let v = '';
      switch (f.mode) {
        case FieldModes.BLACK:
          v = 'b,';
          break;
        case FieldModes.BLACKKNOWN:
          v = `b${f.value},`;
          break;
        case FieldModes.USER:
          v = `${f.user ?? ''}(${f.value}),`;
          break;
        case FieldModes.WHITEKNOWN:
          v = `w${f.value},`;
          break;
        default:
          v = `error unknown mode: ${f.mode},`;
          break;
      }

      result.push(v);
    }

    result.push('\n');
  }

  return result.join('');
}

const v128gameCodes: [size: number, gameCode: string][] = [
  [4, 'gCCBsC2blYEaC'],
  [5, 'gCkWIIEXIvIwoFdVwguEA2C'],
  [6, 'gDELdYRLIK4MhwQQ2tuMIiAEG5LiDdhRg'],
  [7, 'gDtI4eAiGYoiXCYtwXEsNxlUS4g4NcZECCQfTMGINzF'],
  [8, 'gEDSAhRGuC4xMvcpTHWIlwiGG5zFY8prhGQSSCMMtwEwU5r6G4RCoCP4'],
  [
    9,
    'gE3gBqAjBKjhA3hicD37D3qACpDAhh3rijXggJDCCjh3sXgCZkDD6gBAg3jJEDihiCrXggZiX33jgsJKBjCg',
  ],
  [
    10,
    'gFBoApXkCiDL4i4EkXhB3jErjEXhghCCjrCBi3oX3kKBKgjjB3gXjBsiiDsYA3iiEDrXk45hEErjXgiChB3kXiB3jjXohAhgcirDkCA',
  ],
  [
    11,
    'gFkDkjKlCXhpAjhkCjEpACA9EjFLkXgA6hhXtXkDqjaI3hogBX3kEj3qChqidEjkX33jBAh39DLkYCiXk3hBiA3sXjjBCpiXlE3jkCEDghBilMjYCiLAIpB3lEEg',
  ],
  [
    12,
    'gGIdhXjrXg33kM3qhjCBXgYlklCkCDkgjBB3lFjD3tkMlXhCB33slELDil3hI6MJMlNqpjjAiX3gkElBhCDqoLXlF4iqLskBjYXjDi3lFsE3hJogXiBkEjNDihBiC3hYMFFkj3hB7Xg3iK4EXjg',
  ],
  [
    13,
    'gGlAFmECAhhDjKkmbFN3ggJBiiXsFlE3hoBA3jCkD3l4AiBB63sDklDEDhChiYFE3lmXk3gA3ijDiXlF83pdDC-DkXp33ghXijjEXlmXh3gA-BhXjkDFEiChBiiX3lOEl3g3iiBjXkllGXgBAiXjDlEEt4AheBhmCEFtDEqgAjhA',
  ],
];

vt.describe('Game', () => {
  vt.describe('restoreStateAsync', () => {
    vt.it('should roundtrip with dumpState', async () => {
      const dumped = {
        check_count: 2,
        hint_count: 1,
        data: {
          gameState:
            'AAIAAAAAAABYsBMAAIAHAACAAoAAAJgAAAAAMADgoQUAAD4AAADgAQAAAD4AAAAAAIAPAEAAAADgAQAA',
          checkerboard: 'QEUChANRACkUQAA',
          size: 9,
          created: new Date('2025-12-27T20:45:25.053Z'),
          percentSolved: 23,
        },
      };

      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      await game.restoreStateAsync(dumped);
      var result = game.dumpState();
      vt.expect(result).toEqual(dumped);
    });
  });

  vt.it('should read old state format (F2.1)', async () => {
    const code =
      'gErEJoChCAjijCX33hhAkD3rCC3hhAXkBBjjKqI3jphDCiECXjC3333gD3iiAhhXrBI3333gXhhgJDsKAjKg';
    const game = new Str8ts.Game(dummyRenderer).parseGameCode(code)!;
    await game.restoreStateAsync(oldStyleData);
    const result = game.dumpState();
    const expected = {
      check_count: 2,
      hint_count: 15,
      data: {
        gameState:
          'ALRtmYQgADAKNBAQEBCAmzck0AgIBABMCAlw5IBJgAcPATBmICASAwYBQYECAgQyJEYUQJACIA',
        checkerboard: 'AA4IiAQCeQR6AAA',
        size: 9,
        percentSolved: 84,
      },
    };

    vt.expect(result).toMatchExpected(expected);
  });

  vt.it('should read old state format (F1)', async () => {
    const code =
      'gErEJoChCAjijCX33hhAkD3rCC3hhAXkBBjjKqI3jphDCiECXjC3333gD3iiAhhXrBI3333gXhhgJDsKAjKg';
    const game = new Str8ts.Game(dummyRenderer).parseGameCode(code)!;
    await game.restoreStateAsync(oldStyleData.data);
    const result = game.dumpState();
    const expected = {
      check_count: 0,
      hint_count: 0,
      data: {
        gameState:
          'ALRtmYQgADAKNBAQEBCAmzck0AgIBABMCAlw5IBJgAcPATBmICASAwYBQYECAgQyJEYUQJACIA',
        checkerboard: 'AA4IiAQCeQR6AAA',
        size: 9,
        percentSolved: 84,
      },
    };

    vt.expect(result).toMatchExpected(expected);
  });

  vt.describe('dumpState', () => {
    vt.it('should produce the expected output', async () => {
      const expected = {
        check_count: 0,
        hint_count: 0,
        data: {
          gameState:
            'AAIAAAAAAABYsBMAAIAHAACAAoAAAJgAAAAAMADgoQUAAD4AAADgAQAAAD4AAAAAAIAPAEAAAADgAQAA',
          checkerboard: 'QEUChANRACkUQAA',
          size: 9,
          percentSolved: 23,
        },
      };

      const game = new Str8ts.Game(dummyRenderer).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      await game.restoreStateBase64Async(expected.data.gameState);
      var result = game.dumpState();
      vt.expect(result).toMatchExpected(expected);
    });
  });

  vt.describe('parseGameCode', () => {
    vt.it('should parse V2 game code correctly', () => {
      const game = new Str8ts.Game(dummyRenderer).parseGameCode(
        'AkgAb-BxBhbwwsBBbyMQAb0gxFhRwQlAxRrxyL7xRAkwcLxh7xUUkBAxVE11mLwUrxuAQLwg7yBRxr7w0hA'
      )!;

      var result = gameToText(game);
      vt.expect(result).toMatchSnapshot();
    });

    vt.it.each(v128gameCodes)(
      'should parse V128 game code for size %i correctly',
      (size: number, code: string) => {
        const game = new Str8ts.Game(dummyRenderer).parseGameCode(code);
        vt.expect(game).toBeTruthy();
        vt.expect(game?.renderer).toBe(dummyRenderer);

        var result = gameToText(game);
        vt.expect(result).toMatchSnapshot();
      }
    );

    vt.it.each(v128gameCodes)(
      'should return null for incomplete V128 game code of size %i',
      (size: number, code: string) => {
        const game = new Str8ts.Game(dummyRenderer).parseGameCode(
          code.substring(0, code.length / 2)
        );
        vt.expect(game).toBe(null);
      }
    );
  });
});
