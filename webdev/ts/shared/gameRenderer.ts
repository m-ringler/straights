// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later';

import * as Str8ts from './game';

const chistmasEmojis = [
  '🔔',
  '🎁',
  '🕯️',
  '🎅',
  '👼',
  '🎶',
  '❄️',
  '☃️',
  '⛄',
  '🌟',
  '🎄',
  '🍷',
  '🦌',
  '🌨️',
  '🎆',
  '🎇',
  '🧦',
  '🎀',
  '🧸',
  '🍀',
];

export class JQueryFieldRenderer {
  constructor(
    private $: JQueryStatic,
    private darkMode: boolean
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

  renderField(field: Str8ts.Field) {
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
      fillBlackField(element);
    }
  }

  private getBackgroundColor(field: Str8ts.Field) {
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

  private getTextColor(field: Str8ts.Field) {
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
  getElement(field: Str8ts.Field): JQuery<HTMLElement> {
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

  private static getSelector(field: Str8ts.Field): string {
    return `#ce${field.row}_${field.col}`;
  }
}

function fillBlackField(element: JQuery<HTMLElement>) {
  const now = new Date();
  if (isChristmasTime(now)) {
    setEmoji(element, chistmasEmojis);
  }
}

function isChristmasTime(now: Date) {
  const month = now.getMonth(); // 0 = Jan, 11 = Dec
  const day = now.getDate();
  return (month === 11 && day >= 20) || (month === 0 && day <= 6);
}

function setEmoji(element: JQuery<HTMLElement>, emojis: string[]) {
  let emoji = element.data('festive-emoji') as string | undefined;
  if (!emoji) {
    emoji = randomItem(emojis);
    element.data('festive-emoji', emoji);
  }
  element.text(emoji);
}

function randomItem(emojis: string[]) {
  return emojis[Math.floor(Math.random() * emojis.length)];
}
