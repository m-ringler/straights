// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import * as Str8ts from './game.js';
import * as emojisModule from './seasonalEmojis.js';

export interface RenderableField {
  row: number;
  col: number;
  mode: number;
  value: number | undefined;
  user: number | undefined;
  notes: Set<number>;
  hint: number | undefined;
  wrong: boolean;
  isShowingSolution: boolean;
  game: {
    size: number;
  };
  isEditable(): boolean;
  isActive(): boolean;
}

export class JQueryFieldRenderer {
  private emojiSetId = 0;
  private emojiSet = '';

  constructor(
    private $: JQueryStatic,
    private darkMode: boolean,
    private maxGridSize: number
  ) {
    this.colors = this.darkMode
      ? JQueryFieldRenderer.gameColorsDark
      : JQueryFieldRenderer.gameColorsLight;
  }

  static gameColorsLight = {
    FG_BLACK: '#ffffff',
    FG_USER: '#003378',
    FG_USER_WRONG: '#5f0052ff',
    FG_SOLUTION: '#5f0052ff',
    FG_WHITEKNOWN: '#000000',
    BG_BLACK: '#000000',
    BG_USER: '#ffffff',
    BG_USER_ACTIVE: '#c7ddff',
    BG_USER_WRONG: '#ffc7c7',
    BG_USER_WRONG_ACTIVE: '#eeaaff',
    BG_WHITEKNOWN: '#ffffff',
    BG_HINT: '#ffff99',
  };

  static gameColorsDark = {
    FG_BLACK: '#aaaaaa',
    FG_USER: '#003378',
    FG_USER_WRONG: '#5f0052ff',
    FG_SOLUTION: '#5f0052ff',
    FG_WHITEKNOWN: '#000000',
    BG_BLACK: '#000000',
    BG_USER: '#aaaaaa',
    BG_USER_ACTIVE: '#7379bf',
    BG_USER_WRONG: '#ffc7c7',
    BG_USER_WRONG_ACTIVE: '#eeaaff',
    BG_WHITEKNOWN: '#aaaaaa',
    BG_HINT: '#ffff99',
  };

  colors: {
    FG_BLACK: string;
    FG_USER: string;
    FG_USER_WRONG: string;
    FG_SOLUTION: string;
    FG_WHITEKNOWN: string;
    BG_BLACK: string;
    BG_USER: string;
    BG_USER_ACTIVE: string;
    BG_USER_WRONG: string;
    BG_USER_WRONG_ACTIVE: string;
    BG_WHITEKNOWN: string;
    BG_HINT: string;
  };

  emojis: string[] = [];

  setEmojis(emojis: string | string[] | null) {
    if (emojis === null) {
      this.emojis = [];
    } else if (typeof emojis === 'string') {
      const Seg = Intl.Segmenter;
      if (typeof Seg === 'function') {
        const seg = new Seg(undefined, { granularity: 'grapheme' });
        this.emojis = Array.from(seg.segment(emojis), (s) => s.segment);
      } else {
        this.emojis = Array.from(emojis);
      }
    } else {
      this.emojis = emojis;
    }

    this.emojiSetId = (this.emojiSetId + 1) % 100;
    this.emojiSet = this.emojiSetId.toString();
  }

  renderField(field: RenderableField) {
    const element = this.getElement(field);
    element.empty();
    element.css('background-color', this.getBackgroundColor(field));
    element.css('color', this.getTextColor(field));
    if (field.isEditable()) {
      if (field.isShowingSolution) {
        element.text(field.value!);
      } else {
        if (field.user) {
          element.text(field.user);
        } else if (field.notes.size > 0) {
          let notes = '<table class="mini" cellspacing="0">';
          for (let i = 1; i <= field.game.size; i++) {
            if ((i - 1) % 3 === 0) notes += '<tr>';
            if (field.notes.has(i)) {
              const class_attribute = field.hint === i ? ' class="hint"' : '';
              notes += `<td${class_attribute}>${i}</td>`;
            } else {
              notes += `<td class="transparent">${i}</td>`;
            }
            if (i % 3 === 0) notes += '</tr>';
          }
          notes += '</table>';
          element.append(notes);
        }
      }
    } else if (field.mode === Str8ts.FieldModes.BLACKKNOWN) {
      element.text(field.value!);
    } else if (field.mode === Str8ts.FieldModes.WHITEKNOWN) {
      element.text(field.value!);
    } else if (field.mode === Str8ts.FieldModes.BLACK) {
      this.fillBlackField(element);
    }
  }

  private getBackgroundColor(field: RenderableField) {
    const colors = this.colors;
    if (
      field.mode === Str8ts.FieldModes.BLACK ||
      field.mode === Str8ts.FieldModes.BLACKKNOWN
    ) {
      return colors.BG_BLACK;
    }

    if (field.mode === Str8ts.FieldModes.WHITEKNOWN) {
      return colors.BG_WHITEKNOWN;
    }

    if (field.hint) {
      return colors.BG_HINT;
    }

    if (field.isActive()) {
      return field.wrong ? colors.BG_USER_WRONG_ACTIVE : colors.BG_USER_ACTIVE;
    }

    return field.wrong ? colors.BG_USER_WRONG : colors.BG_USER;
  }

  private getTextColor(field: RenderableField) {
    const colors = this.colors;
    if (
      field.mode === Str8ts.FieldModes.BLACKKNOWN ||
      field.mode === Str8ts.FieldModes.BLACK
    ) {
      return colors.FG_BLACK;
    }

    if (!field.isEditable()) {
      return colors.FG_WHITEKNOWN;
    }

    if (field.wrong) {
      return colors.FG_USER_WRONG;
    }

    if (field.isShowingSolution) {
      if (field.user !== field.value) {
        return colors.FG_SOLUTION;
      }
    }

    return colors.FG_USER;
  }

  /**
   * Retrieves the single DOM element associated with the field.
   * @returns A jQuery object containing exactly one HTMLElement.
   * @throws {Error} If no element is found or if multiple elements are found.
   */
  getElement(field: RenderableField): JQuery<HTMLElement> {
    const result = this.$(JQueryFieldRenderer.getSelector(field));
    if (result.length == 0) {
      throw new Error(
        `Element not found: this.${JQueryFieldRenderer.getSelector(field)}`
      );
    }

    if (result.length > 1) {
      throw new Error(
        `Multiple elements found: this.${JQueryFieldRenderer.getSelector(field)}`
      );
    }

    return result;
  }

  private static getSelector(field: RenderableField): string {
    return `#ce${field.row}_${field.col}`;
  }

  createGridInContainer(container: JQuery<HTMLElement> | any): void {
    for (let r = 0; r < this.maxGridSize; r++) {
      let row = `<tr class="row" id="r${r}" data-row="${r}">`;
      for (let c = 0; c < this.maxGridSize; c++) {
        row += `<td class="cell" id="ce${r}_${c}" data-row="${r}" data-col="${c}"></td>`;
      }
      row += '</tr>';
      container.append(row);
    }
  }

  setGridSize(size: number): void {
    if (size < 1 || size > this.maxGridSize) {
      throw new Error(
        `Invalid grid size ${size}, must be between 1 and ${this.maxGridSize}`
      );
    }

    for (let r = 0; r < size; r++) {
      this.$('#r' + r).show();
      for (let c = 0; c < size; c++) {
        this.$(`#ce${r}_${c}`).show();
      }
      for (let c = size; c < this.maxGridSize; c++) {
        this.$(`#ce${r}_${c}`).hide();
      }
    }
    for (let r = size; r < this.maxGridSize; r++) {
      this.$('#r' + r).hide();
    }
  }

  private fillBlackField(element: JQuery<HTMLElement>) {
    if (this.emojis?.length > 0) {
      setEmoji(element, this.emojis, this.emojiSet);
      return;
    }

    const emojisForDate = emojisModule.getEmojis(new Date());
    if (emojisForDate) {
      setEmoji(element, emojisForDate.emojis, emojisForDate.key);
    }
  }
}

function setEmoji(
  element: JQuery<HTMLElement>,
  emojis: string[],
  emojiKey: string
) {
  let currentEmojiKey = element.data('emojis') as string | undefined;
  let currentEmoji = element.data('emoji') as string | undefined;
  if (currentEmojiKey != emojiKey || !currentEmoji) {
    currentEmoji = randomItem(emojis);
    element.data('emojis', emojiKey);
    element.data('emoji', currentEmoji);
  }

  element.text(currentEmoji);
}

function randomItem(emojis: string[] | string) {
  return emojis[Math.floor(Math.random() * emojis.length)];
}
