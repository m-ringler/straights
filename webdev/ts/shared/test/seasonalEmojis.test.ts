// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect } from 'vitest';
import {
  getEasterDate,
  getEmojis,
  christmasEmojis,
  valentineEmojis,
  easterEmojis,
} from '../seasonalEmojis';

describe('getEasterDate', () => {
  it.each([
    [2026, 3, 5], // April 5
    [2027, 2, 28], // March 28
    [2028, 3, 16], // April 16
    [2029, 3, 1], // April 1
    [2030, 3, 21], // April 21
    [2031, 3, 13], // April 13
  ])(
    'should calculate Easter date for year %i as month %i, day %i',
    (year, month, day) => {
      const result = getEasterDate(year);
      expect(result.getFullYear()).toBe(year);
      expect(result.getMonth()).toBe(month);
      expect(result.getDate()).toBe(day);
    }
  );
});

describe('getEmojis', () => {
  it('should return Christmas emojis during Christmas time', () => {
    const date = new Date(2026, 11, 25); // December 25
    expect(getEmojis(date)).toEqual({
      emojis: christmasEmojis,
      key: 'xmas',
    });
  });

  it("should return Valentine emojis on Valentine's Day", () => {
    const date = new Date(2026, 1, 14); // February 14
    expect(getEmojis(date)).toEqual({
      emojis: valentineEmojis,
      key: 'valentine',
    });
  });

  it('should return Easter emojis during Easter time', () => {
    const easterDate = getEasterDate(2026); // April 5
    const date = new Date(
      easterDate.getFullYear(),
      easterDate.getMonth(),
      easterDate.getDate()
    );
    expect(getEmojis(date)).toEqual({
      emojis: easterEmojis,
      key: 'easter',
    });
  });

  it('should return null when not during any seasonal period', () => {
    const date = new Date(2026, 5, 15); // June 15
    expect(getEmojis(date)).toBe(null);
  });

  it('should return Easter emojis within 7 days before Easter', () => {
    const easterDate = getEasterDate(2026); // April 5
    const date = new Date(
      easterDate.getFullYear(),
      easterDate.getMonth(),
      easterDate.getDate() - 5
    );
    expect(getEmojis(date)).toEqual({
      emojis: easterEmojis,
      key: 'easter',
    });
  });

  it('should return Easter emojis within 7 days after Easter', () => {
    const easterDate = getEasterDate(2026); // April 5
    const date = new Date(
      easterDate.getFullYear(),
      easterDate.getMonth(),
      easterDate.getDate() + 3
    );
    expect(getEmojis(date)).toEqual({
      emojis: easterEmojis,
      key: 'easter',
    });
  });

  it('should not return Easter emojis more than 7 days before Easter', () => {
    const easterDate = getEasterDate(2026); // April 5
    const date = new Date(
      easterDate.getFullYear(),
      easterDate.getMonth(),
      easterDate.getDate() - 8
    );
    expect(getEmojis(date)).toBe(null);
  });

  it('should not return Easter emojis more than 7 days after Easter', () => {
    const easterDate = getEasterDate(2026); // April 5
    const date = new Date(
      easterDate.getFullYear(),
      easterDate.getMonth(),
      easterDate.getDate() + 8
    );
    expect(getEmojis(date)).toBe(null);
  });
});

describe('emoji sets', () => {
  it('should have correct Christmas emojis', () => {
    expect(christmasEmojis).toMatchInlineSnapshot(`
      [
        "🔔",
        "🎁",
        "🕯️",
        "🎅",
        "👼",
        "🎶",
        "❄️",
        "☃️",
        "⛄",
        "🌟",
        "🎄",
        "🦌",
        "🌨️",
        "🎆",
        "🎇",
        "🧦",
        "🎀",
        "🧸",
        "🍀",
        "🛷",
      ]
    `);
  });

  it('should have correct Valentine emojis', () => {
    expect(valentineEmojis).toMatchInlineSnapshot(`
      [
        "🧡",
        "💛",
        "💚",
        "💙",
        "💜",
        "💖",
        "💘",
        "💕",
      ]
    `);
  });

  it('should have correct Easter emojis', () => {
    expect(easterEmojis).toMatchInlineSnapshot(`
      [
        "🐣",
        "🐰",
        "🌷",
        "🥚",
        "🥚",
        "🐤",
        "🐇",
        "🐑",
        "🧺",
      ]
    `);
  });
});
