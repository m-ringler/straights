// @vitest-environment jsdom
// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { JQueryFieldRenderer, RenderableField } from '../gameRenderer';
import { FieldModes } from '../gameReader';
import $ from 'jquery';

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

  /**
   * Helper to create a mock field and render it
   */
  function createAndRenderField(config: {
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
  }): {
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
      const { element } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"`
      );
    });
  });

  describe('BLACKKNOWN field mode', () => {
    it('should render BLACKKNOWN field with value and black colors', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.BLACKKNOWN,
        value: 5,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);">5</td>"`
      );
    });
  });

  describe('WHITEKNOWN field mode', () => {
    it('should render WHITEKNOWN field with value', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 7,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">7</td>"`
      );
    });
  });

  describe('USER (editable) field mode', () => {
    it('should render USER field with no content (empty)', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        value: 7,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"></td>"`
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
        const { element } = createAndRenderField({
          mode: FieldModes.USER,
          value: 4,
          isShowingSolution: true,
          ...config,
        });

        expect(element.outerHTML).toBe(
          '<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(95, 0, 82);">4</td>'
        );
      }
    );

    it('should render USER field showing solution when isShowingSolution is true (correct)', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        value: 4,
        user: 4,
        notes: [2, 3, 4],
        isShowingSolution: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">4</td>"`
      );
    });

    it('should render USER field with user-entered number (unchecked)', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 3,
        value: 5,
        notes: [2, 3, 4],
        isShowingSolution: false,
        wrong: false,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">3</td>"`
      );
    });

    it('should render USER field with user-entered number (wrong)', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 3,
        notes: [2, 3, 4],
        value: 5,
        isShowingSolution: false,
        wrong: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">3</td>"`
      );
    });

    it('should render USER field with multiple notes', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 2, 5, 7, 8, 9],
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"><table class="mini" cellspacing="0"><tbody><tr><td>1</td><td>2</td><td class="transparent">3</td></tr><tr><td class="transparent">4</td><td>5</td><td class="transparent">6</td></tr><tr><td>7</td><td>8</td><td>9</td></tr></tbody></table></td>"`
      );
    });

    it('should render USER field with notes table including hint highlight', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [2, 3, 5, 8],
        hint: 5,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);"><table class="mini" cellspacing="0"><tbody><tr><td class="transparent">1</td><td>2</td><td>3</td></tr><tr><td class="transparent">4</td><td class="hint">5</td><td class="transparent">6</td></tr><tr><td class="transparent">7</td><td>8</td><td class="transparent">9</td></tr></tbody></table></td>"`
      );
    });

    it('should render USER field with single note', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [4],
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"><table class="mini" cellspacing="0"><tbody><tr><td class="transparent">1</td><td class="transparent">2</td><td class="transparent">3</td></tr><tr><td>4</td><td class="transparent">5</td><td class="transparent">6</td></tr><tr><td class="transparent">7</td><td class="transparent">8</td><td class="transparent">9</td></tr></tbody></table></td>"`
      );
    });
  });

  describe('Background colors', () => {
    it('should use BG_USER color for editable field with no special state', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"></td>"`
      );
    });

    it('should use BG_USER_ACTIVE color when field is active', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        isActive: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(199, 221, 255); color: rgb(0, 51, 120);"></td>"`
      );
    });

    it('should use BG_USER_WRONG color when field has wrong entry', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 5,
        wrong: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">5</td>"`
      );
    });

    it('should use BG_USER_WRONG_ACTIVE when field is active and wrong', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 6,
        wrong: true,
        isActive: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(238, 170, 255); color: rgb(95, 0, 82);">6</td>"`
      );
    });

    it('should use BG_HINT color when hint is set', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        hint: 3,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);"></td>"`
      );
    });

    it('should use BG_BLACK color for BLACK fields', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"`
      );
    });

    it('should use BG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 2,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">2</td>"`
      );
    });
  });

  describe('Text colors', () => {
    it('should use FG_USER color for normal user field', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 1,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">1</td>"`
      );
    });

    it('should use FG_USER_WRONG color for wrong entry', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        user: 9,
        wrong: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);">9</td>"`
      );
    });

    it('should use FG_SOLUTION color when showing solution that differs from user entry', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        value: 5,
        user: 3,
        isShowingSolution: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(95, 0, 82);">5</td>"`
      );
    });

    it('should use FG_USER color when showing correct solution', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        value: 4,
        user: 4,
        isShowingSolution: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);">4</td>"`
      );
    });

    it('should use FG_BLACK color for BLACK fields', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(0, 0, 0); color: rgb(255, 255, 255);"></td>"`
      );
    });

    it('should use FG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 8,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 0, 0);">8</td>"`
      );
    });
  });

  describe('Dark mode', () => {
    it('should use dark mode colors when darkMode is true', () => {
      const darkRenderer = new JQueryFieldRenderer($, true);

      const field: RenderableField = {
        row: 0,
        col: 0,
        mode: FieldModes.USER,
        value: undefined,
        user: 2,
        notes: new Set(),
        hint: undefined,
        wrong: false,
        isShowingSolution: false,
        game: { size: 9 },
        isEditable: () => true,
        isActive: () => false,
      };

      const fieldElement = document.createElement('div');
      fieldElement.id = `ce${field.row}_${field.col}`;
      row.appendChild(fieldElement);

      darkRenderer.renderField(field);
      expect(fieldElement.outerHTML).toMatchInlineSnapshot(
        `"<div id="ce0_0" style="background-color: rgb(170, 170, 170); color: rgb(0, 51, 120);">2</div>"`
      );
    });
  });

  describe('Notes table rendering', () => {
    it('should render notes table with correct structure', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 3, 5, 7, 9],
      });

      const table = element.querySelector('table');
      expect(table).toBeTruthy();
      expect(table?.className).toBe('mini');

      const cells = element.querySelectorAll('td');
      expect(cells.length).toBe(9);

      const noteCells = Array.from(cells).filter(
        (cell) => !cell.classList.contains('transparent')
      );
      expect(noteCells.length).toBe(5);
    });

    it('should mark transparent cells for missing notes', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 5, 9],
      });

      const transparentCells = element.querySelectorAll('td.transparent');
      expect(transparentCells.length).toBe(6);

      const noteCells = element.querySelectorAll('td:not(.transparent)');
      expect(noteCells.length).toBe(3);
    });

    it('should apply hint class to highlighted note', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 2, 3, 4, 5, 6, 7, 8, 9],
        hint: 7,
      });

      const hintCell = element.querySelector('td.hint');
      expect(hintCell).toBeTruthy();
      expect(hintCell?.textContent).toBe('7');

      const otherCells = element.querySelectorAll('td:not(.hint)');
      expect(otherCells.length).toBe(8);
    });

    it('should render correct table structure with snapshot', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 3, 5, 7, 9],
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 255); color: rgb(0, 51, 120);"><table class="mini" cellspacing="0"><tbody><tr><td>1</td><td class="transparent">2</td><td>3</td></tr><tr><td class="transparent">4</td><td>5</td><td class="transparent">6</td></tr><tr><td>7</td><td class="transparent">8</td><td>9</td></tr></tbody></table></td>"`
      );
    });
  });

  describe('Color priority (hint > active > wrong)', () => {
    it('hint color takes precedence over active', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        hint: 5,
        isActive: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 255, 153); color: rgb(0, 51, 120);"></td>"`
      );
    });

    it('active color takes precedence over wrong when no hint', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        wrong: true,
        isActive: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(238, 170, 255); color: rgb(95, 0, 82);"></td>"`
      );
    });

    it('wrong color without active state', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        wrong: true,
      });

      expect(element.outerHTML).toMatchInlineSnapshot(
        `"<td id="ce0_0" style="background-color: rgb(255, 199, 199); color: rgb(95, 0, 82);"></td>"`
      );
    });
  });
});
