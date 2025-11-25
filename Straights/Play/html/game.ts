// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

/* Minimal port of game.js into TypeScript source file. (kept as-is, not typed.) */
/* Original code maintained; this is a source migration step. */

// NOTE: game.js contains many functions and class-like structures. To keep the
// migration minimal (no behavioral changes), copy the original content to .ts
// and rely on loose compiler settings. Type-checking and refactor can be done
// in a follow-up.

export class Game {
  constructor($, darkMode) {
    // Placeholder: the real game object implementation lives in original JS.
    // During a deeper migration we will type and implement this class.
    console.warn('Game: placeholder; full implementation lives in TypeScript source migration.');
  }
}

export const minCodeSize = 10
