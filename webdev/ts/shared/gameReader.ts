// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export interface GameBuilder<T> {
  setField(row: number, col: number, mode: number, value: number): void;

  getGame(): T;
}

export type GameBuilderFactory<T> = (size: number) => GameBuilder<T>;

export const FieldModes = {
  USER: 0,
  WHITEKNOWN: 1,
  BLACK: 2,
  BLACKKNOWN: 3,
} as const;

const base64urlCharacters =
  'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_';

function getMode(isKnown: boolean, isBlack: boolean) {
  const mode = isBlack
    ? isKnown
      ? FieldModes.BLACKKNOWN
      : FieldModes.BLACK
    : isKnown
      ? FieldModes.WHITEKNOWN
      : FieldModes.USER;
  return mode;
}

// Parse game
function parseGameV128<T>(
  binary: string,
  createBuilder: GameBuilderFactory<T>
): T | null {
  const size = parseInt(binary.substring(0, 5), 2);
  const pos = 5;

  const bitsPerNumber = Math.floor(Math.log2(size - 1)) + 1;
  const bitsPerField = 2 + bitsPerNumber; // black + known + number
  const builder = createBuilder(size);

  for (let row = 0; row < size; row++) {
    for (let col = 0; col < size; col++) {
      const fieldStart = pos + (row * size + col) * bitsPerField;
      const isBlack = binary[fieldStart] === '1';
      const isKnown = binary[fieldStart + 1] === '1';

      const numberBits = binary.substring(
        fieldStart + 2,
        fieldStart + 2 + bitsPerNumber
      );
      const value = parseInt(numberBits, 2) + 1;
      if (isNaN(value)) {
        console.warn(
          `Cannot parse game: invalid value ${value} at (${row}, ${col}).`
        );
        return null; // Invalid data
      }

      builder.setField(row, col, getMode(isKnown, isBlack), value);
    }
  }

  return builder?.getGame();
}

function parseGameV002<T>(
  binary: string,
  createBuilder: GameBuilderFactory<T>
): T | null {
  const builder = createBuilder(9);
  if (binary.length < 6 * 81 || binary.length > 6 * 81 + 8) {
    return null; // Invalid data
  }

  for (let i = 0; i < 81; i++) {
    const subBinary = binary.substring(i * 6, (i + 1) * 6);
    const mode = parseInt(subBinary.substring(0, 2), 2);
    const value = parseInt(subBinary.substring(2, 6), 2) + 1;
    builder.setField(Math.floor(i / 9), i % 9, mode, value);
  }

  return builder.getGame();
}

export function createGame<T>(
  base64urlEncodedGameCode: string,
  createBuilder: GameBuilderFactory<T>
): T | null {
  const decoded = base64GameCodeToBinary(base64urlEncodedGameCode);

  let result: T | null = null;
  switch (decoded.encodingVersion) {
    case 1:
      // not supported any more
      result = null;
      break;
    case 128:
      // 0b10000000: arbitrary size game encoding
      result = parseGameV128(decoded.binary, createBuilder);
      break;
    case 2:
      result = parseGameV002(decoded.binary, createBuilder);
      break;
    default:
      // Unknown encoding version
      result = null;
      break;
  }

  if (result === null) {
    console.warn('Failed to parse game from code: ', base64urlEncodedGameCode);
  }

  return result;
}

function base64GameCodeToBinary(gameCode: string) {
  let binary = '';
  for (let i = 0; i < gameCode.length; i++) {
    let b = base64urlCharacters.indexOf(gameCode.charAt(i)).toString(2);
    while (b.length < 6) b = '0' + b;
    binary += b;
  }
  const encodingVersion = parseInt(binary.substring(0, 8), 2);
  binary = binary.substring(8);

  return { encodingVersion, binary };
}
