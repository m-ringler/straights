// @vitest-environment jsdom
// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { JQueryFieldRenderer, RenderableField } from '../gameRenderer';
import { FieldModes } from '../gameReader';
import $ from 'jquery';
import * as seasonalEmojisModule from '../seasonalEmojis.js';

// Mock the seasonalEmojis module to always return null
vi.mock('../seasonalEmojis.js', () => ({
  getEmojis: () => null,
}));

describe('JQueryFieldRenderer', () => {
  let row: HTMLTableRowElement;
  let table: HTMLTableElement;
  let renderer: JQueryFieldRenderer;

  beforeEach(() => {
    // Set up DOM container
    table = document.createElement('table');
    row = document.createElement('tr');
    table.appendChild(row);
    document.body.appendChild(table);

    // Create renderer with light mode
    renderer = new JQueryFieldRenderer($, false);
  });

  afterEach(() => {
    if (table && table.parentNode) {
      table.parentNode.removeChild(table);
    }
  });

  type renderData = {
    mode: number;
    value?: number;
    user?: number;
    notes?: number[];
    hint?: number;
    wrong?: boolean;
    isShowingSolution?: boolean;
    isActive?: boolean;
    gameSize?: number;
    darkMode?: boolean;
  };

  /**
   * Helper to create a mock field and render it
   */

  function createAndRenderFieldToHtml(config: renderData): string {
    const { element } = createAndRenderField(config);
    return element.outerHTML.replaceAll('<t', '\n<t');
  }

  function createAndRenderField(config: renderData): {
    field: RenderableField;
    element: HTMLElement;
  } {
    const gameSize = config.gameSize ?? 9;
    const isActive = config.isActive ?? false;

    const field: RenderableField = {
      row: 0,
      col: 0,
      mode: config.mode,
      value: config.value,
      user: config.user,
      notes: new Set(config.notes ?? []),
      hint: config.hint,
      wrong: config.wrong ?? false,
      isShowingSolution: config.isShowingSolution ?? false,
      game: { size: gameSize },
      isEditable: () => config.mode === FieldModes.USER,
      isActive: () => isActive,
    };

    // Create DOM element for the field
    const fieldElement = document.createElement('td');
    fieldElement.id = `ce${field.row}_${field.col}`;
    row.appendChild(fieldElement);

    // Render the field
    renderer.renderField(field);

    return { field, element: fieldElement };
  }
  describe('BLACK field mode', () => {
    it('should render BLACK field as empty with black background', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"
      `
      );
    });
  });

  describe('BLACKKNOWN field mode', () => {
    it('should render BLACKKNOWN field with value and black colors', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACKKNOWN,
        value: 5,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);">5</td>"
      `
      );
    });
  });

  describe('WHITEKNOWN field mode', () => {
    it('should render WHITEKNOWN field with value', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.WHITEKNOWN,
        value: 7,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">7</td>"
      `
      );
    });
  });

  describe('USER (editable) field mode', () => {
    it('should render USER field with no content (empty)', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        value: 7,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"></td>"
      `
      );
    });

    it.each<{
      name: string;
      config: any;
    }>([
      { name: 'empty', config: {} },
      { name: 'notes only', config: { notes: [2, 3, 4] } },
      { name: 'wrong', config: { user: 2, notes: [2, 3, 4] } },
    ])(
      'should render USER field showing solution when isShowingSolution is true ($name)',
      ({ config }) => {
        const html = createAndRenderFieldToHtml({
          mode: FieldModes.USER,
          value: 4,
          isShowingSolution: true,
          ...config,
        });

        expect(html).toBe(
          `
<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(95, 0, 82);">4</td>`
        );
      }
    );

    it('should render USER field showing solution when isShowingSolution is true (correct)', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        value: 4,
        user: 4,
        notes: [2, 3, 4],
        isShowingSolution: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">4</td>"
      `
      );
    });

    it('should render USER field with user-entered number (unchecked)', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 3,
        value: 5,
        notes: [2, 3, 4],
        isShowingSolution: false,
        wrong: false,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">3</td>"
      `
      );
    });

    it('should render USER field with user-entered number (wrong)', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 3,
        notes: [2, 3, 4],
        value: 5,
        isShowingSolution: false,
        wrong: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">3</td>"
      `
      );
    });

    it('should render USER field with multiple notes', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        notes: [1, 2, 5, 7, 8, 9],
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">
        <table class="mini" cellspacing="0">
        <tbody>
        <tr>
        <td>1</td>
        <td>2</td>
        <td class="transparent">3</td></tr>
        <tr>
        <td class="transparent">4</td>
        <td>5</td>
        <td class="transparent">6</td></tr>
        <tr>
        <td>7</td>
        <td>8</td>
        <td>9</td></tr></tbody></table></td>"
      `
      );
    });

    it('should render USER field with notes table including hint highlight', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        notes: [2, 3, 5, 8],
        hint: 5,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);">
        <table class="mini" cellspacing="0">
        <tbody>
        <tr>
        <td class="transparent">1</td>
        <td>2</td>
        <td>3</td></tr>
        <tr>
        <td class="transparent">4</td>
        <td class="hint">5</td>
        <td class="transparent">6</td></tr>
        <tr>
        <td class="transparent">7</td>
        <td>8</td>
        <td class="transparent">9</td></tr></tbody></table></td>"
      `
      );
    });

    it('should render USER field with single note', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        notes: [4],
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">
        <table class="mini" cellspacing="0">
        <tbody>
        <tr>
        <td class="transparent">1</td>
        <td class="transparent">2</td>
        <td class="transparent">3</td></tr>
        <tr>
        <td>4</td>
        <td class="transparent">5</td>
        <td class="transparent">6</td></tr>
        <tr>
        <td class="transparent">7</td>
        <td class="transparent">8</td>
        <td class="transparent">9</td></tr></tbody></table></td>"
      `
      );
    });
  });

  describe('Background colors', () => {
    it('should use BG_USER color for editable field with no special state', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"></td>"
      `
      );
    });

    it('should use BG_USER_ACTIVE color when field is active', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        isActive: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(199, 221, 255); color: rgb(0, 51, 120);"></td>"
      `
      );
    });

    it('should use BG_USER_WRONG color when field has wrong entry', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 5,
        wrong: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">5</td>"
      `
      );
    });

    it('should use BG_USER_WRONG_ACTIVE when field is active and wrong', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 6,
        wrong: true,
        isActive: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(238, 170, 255); color: rgb(95, 0, 82);">6</td>"
      `
      );
    });

    it('should use BG_HINT color when hint is set', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        notes: [1, 2, 3],
        hint: 3,
        value: 2,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);">
        <table class="mini" cellspacing="0">
        <tbody>
        <tr>
        <td>1</td>
        <td>2</td>
        <td class="hint">3</td></tr>
        <tr>
        <td class="transparent">4</td>
        <td class="transparent">5</td>
        <td class="transparent">6</td></tr>
        <tr>
        <td class="transparent">7</td>
        <td class="transparent">8</td>
        <td class="transparent">9</td></tr></tbody></table></td>"
      `
      );
    });

    it('should use BG_BLACK color for BLACK fields', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"
      `
      );
    });

    it('should use BG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.WHITEKNOWN,
        value: 2,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">2</td>"
      `
      );
    });
  });

  describe('Text colors', () => {
    it('should use FG_USER color for normal user field', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 1,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">1</td>"
      `
      );
    });

    it('should use FG_USER_WRONG color for wrong entry', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 9,
        wrong: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">9</td>"
      `
      );
    });

    it('should use FG_SOLUTION color when showing solution that differs from user entry', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        value: 5,
        user: 3,
        isShowingSolution: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(95, 0, 82);">5</td>"
      `
      );
    });

    it('should use FG_USER color when showing correct solution', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        value: 4,
        user: 4,
        isShowingSolution: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">4</td>"
      `
      );
    });

    it('should use FG_BLACK color for BLACK fields', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"
      `
      );
    });

    it('should use FG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.WHITEKNOWN,
        value: 8,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">8</td>"
      `
      );
    });
  });

  describe('Dark mode', () => {
    it('should use dark mode colors when darkMode is true', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        user: 2,
        darkMode: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">2</td>"
      `
      );
    });
  });

  describe('Color priority (hint > active > wrong)', () => {
    it('hint color takes precedence over active', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        hint: 5,
        isActive: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);"></td>"
      `
      );
    });

    it('active color takes precedence over wrong when no hint', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        wrong: true,
        isActive: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(238, 170, 255); color: rgb(95, 0, 82);"></td>"
      `
      );
    });

    it('wrong color without active state', () => {
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.USER,
        wrong: true,
      });

      expect(html).toMatchInlineSnapshot(
        `
        "
        <td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);"></td>"
      `
      );
    });
  });

  describe('setEmojis', () => {
    it('should set emojis to empty array when passed null', () => {
      renderer.setEmojis(['😀', '😃', '😄']);
      renderer.setEmojis(null);

      expect(renderer.emojis).toEqual([]);
    });

    it('should set emojis from string array', () => {
      const emojis = ['😀', '😃', '😄'];
      renderer.setEmojis(emojis);

      expect(renderer.emojis).toEqual(emojis);
    });

    it('should segment string emojis using Intl.Segmenter when available', () => {
      // This test assumes Intl.Segmenter is available (it is in modern browsers/Node)
      const emojiString = '😀😃😄';
      renderer.setEmojis(emojiString);

      // Should have segmented the string into individual emojis
      expect(renderer.emojis.length).toBeGreaterThan(0);
      expect(renderer.emojis[0]).toBe('😀');
    });

    it('should update emojiSetId using modulo 100', () => {
      renderer.setEmojis(['😀']);
      const initialSetId = renderer['emojiSetId'];

      renderer.setEmojis(['😃']);
      expect(renderer['emojiSetId']).toBe((initialSetId + 1) % 100);
    });

    it('should reset emojiSetId to 0 after reaching 99', () => {
      // Set emojiSetId to 99
      renderer['emojiSetId'] = 99;
      renderer.setEmojis(['😀']);

      expect(renderer['emojiSetId']).toBe(0);
    });

    it('should set emojiSet to string representation of emojiSetId after increment', () => {
      renderer['emojiSetId'] = 42;
      renderer.setEmojis(['😀']);

      expect(renderer['emojiSet']).toBe('43'); // (42 + 1) % 100 = 43
    });

    it('should set emojiSet to string representation of incremented emojiSetId', () => {
      // Start fresh with emojiSetId at 0
      renderer['emojiSetId'] = 0;
      renderer.setEmojis(['😀']);

      expect(renderer['emojiSet']).toBe('1'); // (0 + 1) % 100 = 1

      renderer.setEmojis(['😃']);
      expect(renderer['emojiSet']).toBe('2'); // (1 + 1) % 100 = 2
    });

    it('should wrap emojiSetId correctly at boundary (99 -> 0)', () => {
      renderer['emojiSetId'] = 99;
      renderer.setEmojis(['😀']);

      expect(renderer['emojiSet']).toBe('0'); // (99 + 1) % 100 = 0
    });

    it('should handle empty array', () => {
      renderer.setEmojis([]);

      expect(renderer.emojis).toEqual([]);
    });

    it('should handle string with special emoji sequences', () => {
      // Test with a complex emoji that might have multiple code points
      const complexEmoji = '👨‍👩‍👧‍👦'; // Family emoji with ZWJ sequences
      renderer.setEmojis(complexEmoji);

      // Intl.Segmenter should handle this correctly as a single grapheme
      expect(renderer.emojis.length).toBe(1);
    });

    it('should update emojis multiple times independently', () => {
      const emojis1 = ['😀', '😃'];
      renderer.setEmojis(emojis1);
      expect(renderer.emojis).toEqual(emojis1);

      const emojis2 = ['😄', '😁', '😆'];
      renderer.setEmojis(emojis2);
      expect(renderer.emojis).toEqual(emojis2);

      renderer.setEmojis(null);
      expect(renderer.emojis).toEqual([]);
    });

    it('should render BLACK field with set emoji', () => {
      renderer.setEmojis(['😀']);
      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      expect(html).toMatchInlineSnapshot(`
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);">😀</td>"
      `);
    });

    it('should render BLACK field with seasonal emoji', () => {
      // Spy on and mock getEmojis to return a seasonal emoji
      const spy = vi.spyOn(seasonalEmojisModule, 'getEmojis');
      spy.mockReturnValue({
        emojis: ['🎄'],
        key: 'seasonal-test',
      });

      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      expect(html).toMatchInlineSnapshot(`
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);">🎄</td>"
      `);

      // Clean up
      spy.mockRestore();
    });

    it('should prioritize setEmojis over seasonal emoji', () => {
      // Set up both emoji sources
      const spy = vi.spyOn(seasonalEmojisModule, 'getEmojis');
      spy.mockReturnValue({
        emojis: ['🎄'],
        key: 'seasonal-test',
      });

      // setEmojis should take precedence
      renderer.setEmojis(['😀']);

      const html = createAndRenderFieldToHtml({
        mode: FieldModes.BLACK,
      });

      // Should render the setEmojis emoji, not the seasonal one
      expect(html).toMatchInlineSnapshot(`
        "
        <td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);">😀</td>"
      `);

      // Clean up
      spy.mockRestore();
    });
  });
});
