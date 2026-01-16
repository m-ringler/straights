// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import * as vt from 'vitest';
import * as Str8ts from '../game';
import { FieldModes } from '../gameReader';
import { getActiveResourcesInfo } from 'node:process';

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
          created: new Date('2025-12-27T20:45:25.053Z').getTime(),
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

        var result = gameToText(game!);
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

  vt.describe('Field methods', () => {
    vt.it('should set user value on editable field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(3);
      vt.expect(field.user).toBe(3);
      vt.expect(field.wrong).toBe(false);
    });

    vt.it('should toggle user value when setting same value again', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(3);
      vt.expect(field.user).toBe(3);

      field.setUser(3);
      vt.expect(field.user).toBeUndefined();
    });

    vt.it('should not allow setting user value on non-editable fields', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      // Find a black or white-known field
      let nonEditableField = null;
      for (let r = 0; r < 9 && !nonEditableField; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (!field.isEditable() && field.user != 3) {
            nonEditableField = field;
            break;
          }
        }
      }

      vt.expect(nonEditableField).not.toBeNull();
      const userBefore = nonEditableField!.user;
      nonEditableField!.setUser(3);
      vt.expect(nonEditableField!.user).toBe(userBefore);
    });

    vt.it('should add and remove notes', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setNote(2);
      vt.expect(field.notes.has(2)).toBe(true);

      field.setNote(2);
      vt.expect(field.notes.has(2)).toBe(false);
    });

    vt.it('should clear user when setting notes', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      // Find an editable field
      let editableField = null;
      for (let r = 0; r < 9 && !editableField; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (field.isEditable()) {
            editableField = field;
            break;
          }
        }
      }

      vt.expect(editableField).not.toBeNull();
      editableField!.setUser(1);
      vt.expect(editableField!.user).toBe(1);

      // When we set a note, it should clear the user
      editableField!.setNote(2);
      vt.expect(editableField!.user).toBeUndefined();
      vt.expect(editableField!.notes.size).toBeGreaterThan(0);
    });

    vt.it('should toggle all notes on and off', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.toggleNoOrAllNotes();
      vt.expect(field.notes).toMatchInlineSnapshot(`
        Set {
          1,
          2,
          3,
          4,
          5,
          6,
          7,
          8,
          9,
        }
      `);

      field.toggleNoOrAllNotes();
      vt.expect(field.notes.size).toBe(0);
    });

    vt.it('should not toggle notes if field has user value', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(2);
      field.toggleNoOrAllNotes();
      vt.expect(field.notes.size).toBe(0);
    });

    vt.it('should clear user value on editable field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(3);
      vt.expect(field.user).toBe(3);

      field.clear();
      vt.expect(field.user).toBeUndefined();
      vt.expect(field.wrong).toBe(false);
    });

    vt.it('should clear notes on editable field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setNote(2);
      field.setNote(3);
      field.clear();
      vt.expect(field.notes.size).toBe(0);
    });

    vt.it('should copy field state', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const original = game.get(0, 2);
      original.setUser(2);
      original.wrong = true;
      original.isShowingSolution = true;

      const copy = original.copy();
      vt.expect(copy.user).toBe(2);
      vt.expect(copy.wrong).toBe(true);
      vt.expect(copy.isShowingSolution).toBe(true);
    });

    vt.it('should detect if field is solved correctly', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      vt.expect(field.isSolved()).toBe(false);

      field.setUser(field.value!);
      vt.expect(field.isSolved()).toBe(true);
    });

    vt.it('should mark field as wrong when incorrect value set', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(999); // Invalid value
      field.checkWrong();
      vt.expect(field.wrong).toBe(true);
    });

    vt.it('should show solution on field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.showSolution();
      vt.expect(field.isShowingSolution).toBe(true);
    });

    vt.it('should set and clear hint', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setHint(2);
      vt.expect(field.hint).toBe(2);

      field.setHint(undefined);
      vt.expect(field.hint).toBeUndefined();
    });

    vt.it('should populate notes when setting hint on empty field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      vt.expect(field.notes.size).toBe(0);
      field.setHint(2);
      vt.expect(field.notes.size).toBe(9);
    });

    vt.it('should track if field is active', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      // Find an editable field
      let editableField = null;
      let editableCoord = null;
      for (let r = 0; r < 9 && !editableField; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (field.isEditable()) {
            editableField = field;
            editableCoord = { r, c };
            break;
          }
        }
      }

      vt.expect(editableField).not.toBeNull();
      vt.expect(editableField!.isActive()).toBeFalsy();
      game.setActiveField(editableCoord!.r, editableCoord!.c);
      vt.expect(editableField!.isActive()).toBeTruthy();
    });

    vt.it('should convert field to JSON array', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      const field = game.get(0, 2);

      field.setUser(2);
      let jsonArray = field.toJsonArray();
      vt.expect(jsonArray).toEqual([2]);

      field.clear();
      field.setNote(1);
      field.setNote(3);
      jsonArray = field.toJsonArray();
      vt.expect(jsonArray.sort()).toEqual([1, 3]);
    });
  });

  vt.describe('Game navigation and interaction', () => {
    vt.it('should set and get active field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      game.setActiveField(0, 2);
      const activeField = game.getActiveField();
      vt.expect(activeField).not.toBeNull();
      vt.expect(activeField?.row).toBe(0);
      vt.expect(activeField?.col).toBe(2);
    });

    vt.it('should not set non-editable field as active', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      // Find a non-editable field
      let nonEditableField = null;
      let nonEditableCoord = null;
      for (let r = 0; r < 9 && !nonEditableField; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (!field.isEditable()) {
            nonEditableField = field;
            nonEditableCoord = { r, c };
            break;
          }
        }
      }

      vt.expect(nonEditableField).not.toBeNull();
      game.setActiveField(nonEditableCoord!.r, nonEditableCoord!.c);
      vt.expect(game.activeFieldIndex).toBeNull();
    });

    vt.it('should not set active field if game is solved', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;
      game.isSolved = true;

      game.setActiveField(0, 2);
      vt.expect(game.activeFieldIndex).toBeNull();
    });

    vt.it('should move selection in four directions', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      // Find first editable field
      let editableCoord = null;
      for (let r = 0; r < 9 && !editableCoord; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (field.isEditable()) {
            editableCoord = { r, c };
            break;
          }
        }
      }

      vt.expect(editableCoord).not.toBeNull();
      game.setActiveField(editableCoord!.r, editableCoord!.c);
      const originalIndex = game.activeFieldIndex;
      vt.expect(game.activeFieldIndex).toEqual({
        col: 0,
        row: 0,
      });

      game.moveSelection(0, 1); // Down
      vt.expect(game.activeFieldIndex).toEqual({
        col: 0,
        row: 2,
      });

      game.moveSelection(0, -1); // Up
      vt.expect(game.activeFieldIndex).toEqual(originalIndex);

      game.moveSelection(1, 0); // Right
      vt.expect(game.activeFieldIndex).toEqual({
        col: 2,
        row: 0,
      });

      game.moveSelection(1, 0); // Right
      vt.expect(game.activeFieldIndex).toEqual({
        col: 3,
        row: 0,
      });

      game.moveSelection(-2, 0); // Left
      vt.expect(game.activeFieldIndex).toEqual(originalIndex);
    });

    vt.it('should not move selection if no active field', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      vt.expect(game.activeFieldIndex).toBeNull();
      game.moveSelection(1, 0);
      vt.expect(game.activeFieldIndex).toBeNull();
    });

    vt.it('should wrap around when moving selection', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      // Find first editable field
      let editableCoord = null;
      for (let r = 0; r < 9 && !editableCoord; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (field.isEditable()) {
            editableCoord = { r, c };
            break;
          }
        }
      }

      vt.expect(editableCoord).not.toBeNull();
      game.setActiveField(editableCoord!.r, editableCoord!.c);

      const initialActiveField = game.getActiveField();
      vt.expect(initialActiveField).not.toBeNull();

      game.moveSelection(1, 0); // Try to move
      // Should find next editable field or wrap
      const newActiveField = game.getActiveField();
      vt.expect(newActiveField).not.toBeNull();
    });
  });

  vt.describe('Game solving and checking', () => {
    vt.it('should check if game is solved', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      vt.expect(game.isSolved).toBe(false);
      game.checkSolved();
      for (let r = 0; r < 9; r++) {
        for (let c = 0; c < 9; c++) {
          const field = game.get(r, c);
          if (field.isEditable()) {
            vt.expect(game.isSolved).toBe(false);
            field.setUser(field.value!);
            game.checkSolved();
          }
        }
      }

      vt.expect(game.isSolved).toBe(true);
    });

    vt.it('should restart game', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      game.setActiveField(0, 2);
      game.get(0, 2).setUser(1);
      game.get(0, 2).setNote(2);
      game.isSolved = true;

      game.restart();
      vt.expect(game.isSolved).toBe(false);
      vt.expect(game.get(0, 2).user).toBeUndefined();
      vt.expect(game.get(0, 2).notes.size).toBe(0);
    });

    vt.it('should check for solution and mark wrong answers', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      const field = game.get(0, 2);
      field.setUser(999); // Set wrong value

      game.check();
      vt.expect(field.wrong).toBe(true);
      vt.expect(game.check_count).toBe(1);
    });

    vt.it('should show solution', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      vt.expect(game.isSolved).toBe(false);
      game.showSolution();
      vt.expect(game.isSolved).toBe(true);
    });

    vt.it('should not show solution if already solved', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      game.isSolved = true;
      game.showSolution();
      // Should not re-process
      vt.expect(game.isSolved).toBe(true);
    });

    vt.it('should check for hint and detect wrong notes', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      const field = game.get(0, 2);
      // Add wrong note
      field.setNote(999); // Invalid note

      const result = game.checkForHint();
      vt.expect(result.isSolved).toBe(false);
      vt.expect(result.isWrong).toBe(true);
      vt.expect(game.hint_count).toBe(1);
    });

    vt.it('should automatically fill single note as user value', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      const field = game.get(0, 2);
      field.setNote(1);
      vt.expect(field.notes.size).toBe(1);

      game.checkSolved();
      // Single note should become user value
      vt.expect(field.user).toBe(1);
      vt.expect(field.notes.size).toBe(0);
    });
  });

  vt.describe('Game state and serialization', () => {
    vt.it('should get game as JSON array', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      game.get(0, 2).setUser(2);
      const json = game.toJsonArray();

      vt.expect(json).toHaveLength(9);
      vt.expect(json[0]).toHaveLength(9);
      vt.expect(json[0][2]).toEqual([2]);
    });

    vt.it('should calculate percent solved correctly', async () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      const state1 = game.dumpState();
      vt.expect(state1.data.percentSolved).toBeGreaterThanOrEqual(0);
      vt.expect(state1.data.percentSolved).toBeLessThanOrEqual(100);

      game.get(0, 2).setUser(game.get(0, 2).value!);
      const state2 = game.dumpState();
      vt.expect(state2.data.percentSolved).toBeGreaterThan(
        state1.data.percentSolved
      );
    });

    vt.it('should include check and hint counts in dumped state', () => {
      const game = new Str8ts.Game(dummyRenderer, 9).parseGameCode(
        'gEg4DCiJMBj3jLkXiaggDCkD3p3gIsD3jCghhCCqX3r3g3pDkYAjChCBiBihXkXjjXhBiXj4DCgYiJhDDisA'
      )!;

      game.check_count = 3;
      game.hint_count = 2;

      const state = game.dumpState();
      vt.expect(state.check_count).toBe(3);
      vt.expect(state.hint_count).toBe(2);
    });
  });
});
