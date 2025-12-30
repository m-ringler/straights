// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { BitmaskEncoder } from './encoder.js';
import * as EncoderModule from './encoder.js';

export const FieldModes = {
  USER: 0,
  WHITEKNOWN: 1,
  BLACK: 2,
  BLACKKNOWN: 3,
} as const;

const MIN_GRID_SIZE_V128 = 4;
const minCodeSizeV2 = 82;
const minCodeSizeV128 =
  (8 /* ENCODINGVERSION */ +
    5 /* size */ +
    2 * MIN_GRID_SIZE_V128 * MIN_GRID_SIZE_V128) /* black, known */ /
  6;

export const minCodeSize = Math.min(minCodeSizeV2, minCodeSizeV128);

export class Field {
  row: number;
  col: number;
  game: Game;
  value: number | undefined;
  mode: number | undefined;
  wrong: boolean;
  hint: undefined | number;
  isShowingSolution: boolean;
  user: undefined | number;
  notes: Set<number>;
  constructor(row: number, col: number, game: Game) {
    this.row = row;
    this.col = col;
    this.game = game;

    // fixed after initialization
    this.value = undefined;
    this.mode = undefined;

    // derived, only used when checking
    this.wrong = false;
    this.hint = undefined;
    this.isShowingSolution = false;

    // working data, edited by the user
    this.user = undefined;
    this.notes = new Set();
  }

  setUser(input: number) {
    if (this.isEditable()) {
      this.wrong = false;
      this.hint = undefined;
      if (this.user === input) {
        this.user = undefined;
        if (this.notes.size === 1) {
          // When we only have a single note we automatically
          // set the user value to that note. But here, we want
          // to switch to note mode. Therefore, we need to remove
          // the single note.
          this.notes.clear();
        }
      } else {
        this.user = input;
      }
      this.render();
    }
  }

  isActive() {
    return (
      this.game.activeFieldIndex &&
      this.game.activeFieldIndex.col === this.col &&
      this.game.activeFieldIndex.row === this.row
    );
  }

  isEditable() {
    return this.mode === FieldModes.USER;
  }

  setNote(value: number) {
    if (this.isEditable()) {
      this.wrong = false;
      this.hint = undefined;
      this.user = undefined;
      if (!this.notes.delete(value)) {
        this.notes.add(value);
      }
      this.render();
    }
  }

  toggleNoOrAllNotes() {
    if (!this.isEditable || this.user) {
      return;
    }

    if (this.notes.size === 0) {
      for (let i = 1; i <= this.game.size; i++) {
        this.notes.add(i);
      }
    } else if (this.notes.size === this.game.size) {
      this.notes.clear();
    }

    this.render();
  }

  clear() {
    if (this.isEditable()) {
      if (this.user) {
        this.user = undefined;
      } else {
        this.notes.clear();
      }

      this.wrong = false;
      this.hint = undefined;
      this.render();
    }
  }

  #isSolvedCorrectly() {
    if (!this.isEditable()) {
      return 1;
    }

    if (!this.user) {
      return 0;
    }

    if (this.user === this.value) {
      return 1;
    }

    return -1;
  }

  isSolved() {
    return this.#isSolvedCorrectly() === 1;
  }

  checkWrong(checkNotes = false) {
    const correct = this.#isSolvedCorrectly();
    switch (correct) {
      case -1:
        this.wrong = true;
        this.render();
        break;
      case 0:
        if (
          checkNotes &&
          this.notes.size != 0 &&
          !this.notes.has(this.value!)
        ) {
          this.wrong = true;
          this.render();
        }
        break;
      default:
        break;
    }
  }

  showSolution() {
    this.isShowingSolution = true;
    this.wrong = this.#isSolvedCorrectly() === -1;
    this.render();
  }

  setHint(number: number | undefined) {
    this.hint = number;
    if (number && this.notes.size === 0) {
      for (let i = 1; i <= this.game.size; i++) {
        this.notes.add(i);
      }
    }

    this.render();
  }

  copy() {
    const field = new Field(this.row, this.col, this.game);

    field.value = this.value;
    field.mode = this.mode;

    field.wrong = this.wrong;
    field.isShowingSolution = this.isShowingSolution;

    field.copyFrom(this);

    return field;
  }

  copyFrom(field: FieldUserData) {
    this.user = field.user;
    this.notes.clear();
    for (const note of field.notes) {
      this.notes.add(note);
    }
  }

  reset(template: FieldUserData | null = null) {
    this.user = undefined;
    this.notes.clear();
    this.wrong = false;
    this.hint = undefined;
    this.isShowingSolution = false;
    if (template) {
      this.copyFrom(template);
    }

    this.render();
  }

  render() {
    this.game.renderer.renderField(this);
  }

  toJsonArray() {
    if (this.mode === FieldModes.BLACK) {
      return [0]; // black empty field
    } else if (this.mode === FieldModes.BLACKKNOWN) {
      return [-this.value!]; // black known field
    } else if (this.mode === FieldModes.WHITEKNOWN) {
      return [this.value!]; // white known field
    } else if (this.user) {
      return [this.user]; // white field with user guess
    } else {
      return Array.from(this.notes); // white field with notes
    }
  }
}

export interface FieldIndex {
  row: number;
  col: number;
}

export interface FieldUserData {
  user?: number | undefined;
  notes: Iterable<number>;
}

export interface HistoryDataRead {
  gameState: string;
  created: number;
}

export interface HistoryData extends HistoryDataRead {
  checkerBoard: string;
  size: number;
  percentSolved: number;
}

export interface DumpedState<TData> {
  check_count: number;
  hint_count: number;
  data: TData;
}

export interface FieldRenderer {
  renderField(field: Field): void;
}

export type DumpedStateRead = DumpedState<FieldUserData[][] | HistoryDataRead>;

export type DumpedStateWrite = DumpedState<HistoryData>;

// class to store and modify the current game state
export class Game {
  size: number;
  data: Field[][];
  isSolved: boolean;
  activeFieldIndex: null | FieldIndex;
  check_count: number;
  hint_count: number;
  created: number;
  private checkerBoardDump: string | null = null;

  constructor(
    public renderer: FieldRenderer,
    size: number = 0
  ) {
    this.size = size;
    this.data = [];
    this.activeFieldIndex = null;
    this.isSolved = false;
    for (let r = 0; r < size; r++) {
      this.data.push([]);
      for (let c = 0; c < size; c++) {
        this.data[r].push(new Field(r, c, this));
      }
    }

    this.check_count = 0;
    this.hint_count = 0;
    this.created = Date.now();
  }

  get(row: number, col: number): Field {
    return this.data[row][col];
  }

  dumpState(): DumpedState<HistoryData> {
    const state = this.dumpStateBase64();
    const historyData: HistoryData = {
      gameState: state,
      checkerBoard: this.getCheckerBoardDump(),
      size: this.size,
      created: this.created,
      percentSolved: this.getPercentSolved(),
    };

    const result = {
      check_count: this.check_count,
      hint_count: this.hint_count,
      data: historyData,
    };

    return result;
  }

  private getPercentSolved(): number {
    let numUserFields = 0;
    let solved = 0;
    this.#getUserFields().forEach((f) => {
      numUserFields++;
      if (f.isSolved()) {
        solved += 1;
      } else if (f.notes.size === 0) {
        // blank field: no progress
      } else {
        solved += 1 - (f.notes.size - 1) / (this.size - 1);
      }
    });

    const percentSolved =
      numUserFields === 0 ? 100 : Math.floor((solved / numUserFields) * 100);
    return percentSolved;
  }

  private getCheckerBoardDump(): string {
    if (this.checkerBoardDump === null) {
      const cb: boolean[][] = [];
      for (const row of this.data) {
        cb.push(
          row.map(
            (f) =>
              f.mode === FieldModes.BLACK || f.mode === FieldModes.BLACKKNOWN
          )
        );
      }

      this.checkerBoardDump = EncoderModule.encodeGridToBase64Url(cb);
    }

    return this.checkerBoardDump;
  }

  async restoreStateAsync(dumpedState: FieldUserData[][] | DumpedStateRead) {
    if (Object.hasOwn(dumpedState, 'check_count')) {
      // new format including check and hint count
      const ds = dumpedState as DumpedStateRead;
      this.check_count = ds.check_count;
      this.hint_count = ds.hint_count;
      const data = ds.data;

      if (Object.hasOwn(data, 'gameState')) {
        // current format F2.2: data is HistoryData
        const historyData = data as HistoryDataRead;
        await this.restoreStateBase64Async(historyData.gameState);
        this.created = historyData.created;
      } else {
        // previous format F2.1: data is FieldUserData[][]
        await this.restoreStateAsync(data as FieldUserData[][]);
      }
    } else {
      // first format F1 (just field data) also used in
      // recursive call for format F2.1
      const ds = dumpedState as FieldUserData[][];
      ds.forEach((row: FieldUserData[], r: number) => {
        row.forEach((field, c) => {
          const gameField = this.get(r, c);
          gameField.copyFrom(field);
          gameField.render();
        });
      });
    }
  }

  #getEncoder() {
    return new BitmaskEncoder({
      compressionThreshold: 48,
      minCompressionRatio: 0.9,
      maxN: 12,
    });
  }

  #getUserFields() {
    return Array.from(this.loopFields(), (x) => x.field).filter(
      (x) => x.mode === FieldModes.USER
    );
  }

  private dumpStateBase64(): string {
    const encoder = this.#getEncoder();
    var data = this.getState();
    const encoded = encoder.encodeUncompressed(this.size, data);
    return encoded.base64Data;
  }

  private getState() {
    return this.#getUserFields().map((f) => (f.user ? [f.user] : f.notes));
  }

  async dumpStateBase64Async(): Promise<string> {
    const encoder = this.#getEncoder();

    var data = this.getState();

    const encoded = await encoder.encodeAsync(this.size, data);
    return encoded.base64Data;
  }

  async restoreStateBase64Async(base64Data: string) {
    const userFields = this.#getUserFields();
    const count = userFields.length;
    const decoded = await this.#getEncoder().decodeAsync(
      { base64Data, count },
      this.size
    );

    for (let i = 0; i < count; i++) {
      userFields[i].reset(toFieldUserData(decoded[i]));
    }
  }

  #setValues(row: number, col: number, mode: number, value: number) {
    const field = new Field(row, col, this);
    this.data[row][col] = field;
    this.data[row][col].mode = mode;
    this.data[row][col].value = value;
    field.render();
  }

  private *loopFields(): IterableIterator<{ field: Field } & FieldIndex> {
    for (let r = 0; r < this.size; r++) {
      for (let c = 0; c < this.size; c++) {
        const field = this.data[r][c];
        yield { field, row: r, col: c };
      }
    }
  }

  #forEachField(iteratorFunction: (f: Field, r: number, c: number) => void) {
    for (const { field, row, col } of this.loopFields()) {
      iteratorFunction(field, row, col);
    }
  }

  showSolution() {
    if (this.isSolved) {
      return;
    }

    this.isSolved = true;
    this.#unselectActiveField();
    this.#forEachField((field) => {
      field.showSolution();
    });
  }

  #checkWrong(checkNotes = false) {
    let result = false;
    this.#forEachField((field) => {
      field.checkWrong(checkNotes);
      if (field.wrong) {
        result = true;
      }
    });

    return result;
  }

  checkSolved() {
    if (this.isSolved) {
      return;
    }

    let finished = true;
    this.#forEachField((field) => {
      if (!field.user && field.notes.size == 1) {
        field.user = field.notes.values().next().value;
        field.notes.clear();
        field.render();
      }

      if (!field.isSolved()) {
        finished = false;
      }
    });

    this.isSolved = finished;
    if (this.isSolved) {
      this.#unselectActiveField();
    }
  }

  checkForHint() {
    this.checkSolved();

    function getResult(solved: boolean, wrong: boolean) {
      const result = { isSolved: solved, isWrong: wrong };
      return result;
    }

    if (this.isSolved) {
      return getResult(true, false);
    }

    this.hint_count++;
    if (this.#checkWrong(false) || this.#checkWrong(true)) {
      return getResult(false, true);
    }

    return getResult(false, false);
  }

  check() {
    this.checkSolved();
    if (this.isSolved) {
      return;
    }

    this.check_count++;
    this.#checkWrong();
  }

  restart() {
    this.isSolved = false;
    this.#forEachField((field) => field.reset());
  }

  getActiveField() {
    return this.activeFieldIndex
      ? this.get(this.activeFieldIndex.row, this.activeFieldIndex.col)
      : null;
  }

  #unselectActiveField() {
    const activeField = this.getActiveField();
    if (activeField) {
      this.activeFieldIndex = null;
      activeField.render();
    }
  }

  selectCell(row: number, col: number) {
    if (!this.isSolved && this.get(row, col).isEditable()) {
      // Reset previously selected field
      this.#unselectActiveField();

      // Change background of just selected field
      this.activeFieldIndex = { row, col };
      this.getActiveField()!.render();
    }
  }

  moveSelection(dx: number, dy: number) {
    if (!this.activeFieldIndex) {
      return;
    }
    const { row, col } = this.activeFieldIndex;
    var newCell = this.#findNextEditableCell(row, col, dy, dx);
    this.selectCell(newCell.row, newCell.col);
  }

  #findNextEditableCell(
    row: number,
    col: number,
    rowDelta: number,
    colDelta: number
  ) {
    let newRow = row;
    let newCol = col;
    do {
      newRow = (newRow + rowDelta + this.size) % this.size;
      newCol = (newCol + colDelta + this.size) % this.size;
    } while (
      !this.get(newRow, newCol).isEditable() &&
      (newRow !== row || newCol != col)
    );

    return { row: newRow, col: newCol };
  }

  // Parse game
  #parseGameV128(binary: string) {
    const size = parseInt(binary.substring(0, 5), 2);
    const pos = 5;

    const bitsPerNumber = Math.floor(Math.log2(size - 1)) + 1;
    const bitsPerField = 2 + bitsPerNumber; // black + known + number
    const result = new Game(this.renderer, size);

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

        const mode = isBlack
          ? isKnown
            ? FieldModes.BLACKKNOWN
            : FieldModes.BLACK
          : isKnown
            ? FieldModes.WHITEKNOWN
            : FieldModes.USER;

        result.#setValues(row, col, mode, value);
      }
    }

    return result;
  }

  #parseGameV002(binary: string) {
    const result = new Game(this.renderer, 9);
    if (binary.length < 6 * 81 || binary.length > 6 * 81 + 8) {
      return; // Invalid data
    }

    for (let i = 0; i < 81; i++) {
      const subBinary = binary.substring(i * 6, (i + 1) * 6);
      const mode = parseInt(subBinary.substring(0, 2), 2);
      const value = parseInt(subBinary.substring(2, 6), 2) + 1;
      result.#setValues(Math.floor(i / 9), i % 9, mode, value);
    }

    return result;
  }

  parseGame(code: string) {
    const decoded = base64GameCodeToBinary(code);

    let result: Game | null | undefined = null;
    switch (decoded.encodingVersion) {
      case 1:
        // not supported any more
        break;
      case 128:
        // 0b10000000: arbitrary size game encoding
        result = this.#parseGameV128(decoded.binary);
        break;
      case 2:
        result = this.#parseGameV002(decoded.binary);
        break;
      default:
        // Unknown encoding version
        break;
    }

    if (!result) {
      console.warn('Failed to parse game from code: ', code);
    }

    return result;
  }

  toJsonArray() {
    return this.data.map((row) => row.map((field) => field.toJsonArray()));
  }
}

function base64GameCodeToBinary(gameCode: string) {
  const base64urlCharacters =
    'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_';
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

function toFieldUserData(notes: Set<number>) {
  let user: number | undefined = undefined;

  // Single note is solved field.
  if (notes.size == 1) {
    for (let v of notes) {
      user = v;
    }
  }

  const userData = { user, notes };
  return userData;
}
