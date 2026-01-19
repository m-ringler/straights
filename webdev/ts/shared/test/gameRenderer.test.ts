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
  let container: HTMLDivElement;
  let renderer: JQueryFieldRenderer;

  beforeEach(() => {
    // Set up DOM container
    container = document.createElement('div');
    document.body.appendChild(container);

    // Create renderer with light mode
    renderer = new JQueryFieldRenderer($, false);
  });

  afterEach(() => {
    if (container && container.parentNode) {
      container.parentNode.removeChild(container);
    }
  });

  /**
   * Helper to extract rendered field properties for assertions
   */
  function extractFieldProperties(element: HTMLElement): {
    backgroundColor: string;
    textColor: string;
    textContent: string;
    html: string;
  } {
    return {
      backgroundColor: element.style.backgroundColor,
      textColor: element.style.color,
      textContent: element.textContent || '',
      html: element.innerHTML,
    };
  }

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
  }): {
    field: RenderableField;
    element: HTMLElement;
    props: ReturnType<typeof extractFieldProperties>;
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
    const fieldElement = document.createElement('div');
    fieldElement.id = `ce${field.row}_${field.col}`;
    container.appendChild(fieldElement);

    // Render the field
    renderer.renderField(field);

    const props = extractFieldProperties(fieldElement);

    return { field, element: fieldElement, props };
  }
  describe('BLACK field mode', () => {
    it('should render BLACK field as empty with black background', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(props.textContent).toBe('');
      expect(props.backgroundColor).toBe('rgb(0, 0, 0)');
      expect(props.textColor).toBe('rgb(170, 170, 170)');
    });
  });

  describe('BLACKKNOWN field mode', () => {
    it('should render BLACKKNOWN field with value and black colors', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.BLACKKNOWN,
        value: 5,
      });

      expect(props.textContent).toBe('5');
      expect(props.backgroundColor).toBe('rgb(0, 0, 0)');
      expect(props.textColor).toBe('rgb(170, 170, 170)');
    });
  });

  describe('WHITEKNOWN field mode', () => {
    it('should render WHITEKNOWN field with value', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 7,
      });

      expect(props.textContent).toBe('7');
      expect(props.backgroundColor).toBe('rgb(255, 255, 255)');
      expect(props.textColor).toBe('rgb(0, 0, 0)');
    });
  });

  describe('USER (editable) field mode', () => {
    it('should render USER field with no content (empty)', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
      });

      expect(props.textContent).toBe('');
      expect(props.backgroundColor).toBe('rgb(255, 255, 255)');
      expect(props.textColor).toBe('rgb(0, 51, 120)');
    });

    it('should render USER field showing solution when isShowingSolution is true', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        value: 4,
        isShowingSolution: true,
      });

      expect(props.textContent).toBe('4');
      expect(props.textColor).toBe('rgb(0, 51, 120)');
    });

    it('should render USER field with user-entered number', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        user: 3,
      });

      expect(props.textContent).toBe('3');
      expect(props.backgroundColor).toBe('rgb(255, 255, 255)');
    });

    it('should render USER field with multiple notes', () => {
      const { props, element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 2, 5, 7, 8, 9],
      });

      expect(props.textContent).toContain('1');
      expect(props.textContent).toContain('2');
      expect(props.textContent).toContain('5');
      expect(element.querySelector('table')).toBeTruthy();
    });

    it('should render USER field with notes table including hint highlight', () => {
      const { element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [2, 3, 5, 8],
        hint: 5,
      });

      const hintCell = element.querySelector('td.hint');
      expect(hintCell).toBeTruthy();
      expect(hintCell?.textContent).toBe('5');
    });

    it('should render USER field with single note', () => {
      const { props, element } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [4],
      });

      expect(props.textContent).toContain('4');
      expect(element.querySelector('table')).toBeTruthy();
    });
  });

  describe('Background colors', () => {
    it('should use BG_USER color for editable field with no special state', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
      });

      expect(props.backgroundColor).toBe('rgb(255, 255, 255)');
    });

    it('should use BG_USER_ACTIVE color when field is active', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        isActive: true,
      });

      expect(props.backgroundColor).toBe('rgb(199, 221, 255)');
    });

    it('should use BG_USER_WRONG color when field has wrong entry', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        user: 5,
        wrong: true,
      });

      expect(props.backgroundColor).toBe('rgb(255, 199, 199)');
    });

    it('should use BG_USER_WRONG_ACTIVE when field is active and wrong', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        user: 6,
        wrong: true,
        isActive: true,
      });

      expect(props.backgroundColor).toBe('rgb(238, 170, 255)');
    });

    it('should use BG_HINT color when hint is set', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        hint: 3,
      });

      expect(props.backgroundColor).toBe('rgb(255, 255, 153)');
    });

    it('should use BG_BLACK color for BLACK fields', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(props.backgroundColor).toBe('rgb(0, 0, 0)');
    });

    it('should use BG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 2,
      });

      expect(props.backgroundColor).toBe('rgb(255, 255, 255)');
    });
  });

  describe('Text colors', () => {
    it('should use FG_USER color for normal user field', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        user: 1,
      });

      expect(props.textColor).toBe('rgb(0, 51, 120)');
    });

    it('should use FG_USER_WRONG color for wrong entry', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        user: 9,
        wrong: true,
      });

      expect(props.textColor).toBe('rgb(95, 0, 82)');
    });

    it('should use FG_SOLUTION color when showing solution that differs from user entry', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        value: 5,
        user: 3,
        isShowingSolution: true,
      });

      expect(props.textColor).toBe('rgb(95, 0, 82)');
    });

    it('should use FG_USER color when showing correct solution', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        value: 4,
        user: 4,
        isShowingSolution: true,
      });

      expect(props.textColor).toBe('rgb(0, 51, 120)');
    });

    it('should use FG_BLACK color for BLACK fields', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.BLACK,
      });

      expect(props.textColor).toBe('rgb(170, 170, 170)');
    });

    it('should use FG_WHITEKNOWN color for WHITEKNOWN fields', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.WHITEKNOWN,
        value: 8,
      });

      expect(props.textColor).toBe('rgb(0, 0, 0)');
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
      container.appendChild(fieldElement);

      darkRenderer.renderField(field);
      const props = extractFieldProperties(fieldElement);

      expect(props.backgroundColor).toBe('rgb(170, 170, 170)');
      expect(props.textContent).toBe('2');
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
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        notes: [1, 3, 5, 7, 9],
      });

      expect(props.html).toMatchSnapshot();
    });
  });

  describe('Color priority (hint > active > wrong)', () => {
    it('hint color takes precedence over active', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        hint: 5,
        isActive: true,
      });

      expect(props.backgroundColor).toBe('rgb(255, 255, 153)'); // BG_HINT
    });

    it('active color takes precedence over wrong when no hint', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        wrong: true,
        isActive: true,
      });

      expect(props.backgroundColor).toBe('rgb(238, 170, 255)'); // BG_USER_WRONG_ACTIVE
    });

    it('wrong color without active state', () => {
      const { props } = createAndRenderField({
        mode: FieldModes.USER,
        wrong: true,
      });

      expect(props.backgroundColor).toBe('rgb(255, 199, 199)'); // BG_USER_WRONG
    });
  });
});
