// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { BitmaskEncoder } from './encoder.js';
import * as EncoderModule from './encoder.js';
import * as GameReader from './gameReader.js';

export const FieldModes = GameReader.FieldModes;

const MIN_GRID_SIZE_V128 = 4;
const minCodeSizeV2 = 82;
const minCodeSizeV128 =
  (8 /* ENCODINGVERSION */ +
    5 /* size */ +
    2 * MIN_GRID_SIZE_V128 * MIN_GRID_SIZE_V128) /* black, known */ /
  6;

export const minCodeSize = Math.min(minCodeSizeV2, minCodeSizeV128);

export class Field {
  wrong: boolean;
  hint: undefined | number;
  isShowingSolution: boolean;
  user: undefined | number;
  notes: Set<number>;
  constructor(
    public readonly row: number,
    public readonly col: number,
    public readonly mode: number,
    public readonly value: number | undefined,
    public readonly game: Game
  ) {
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

  isActive(): boolean {
    return (
      this.game.activeFieldIndex !== null &&
      this.game.activeFieldIndex.col === this.col &&
      this.game.activeFieldIndex.row === this.row
    );
  }

  isEditable(): boolean {
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

  private isSolvedCorrectly() {
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
    return this.isSolvedCorrectly() === 1;
  }

  checkWrong(checkNotes = false) {
    const correct = this.isSolvedCorrectly();
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
    this.wrong = this.isSolvedCorrectly() === -1;
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
    const field = new Field(
      this.row,
      this.col,
      this.mode,
      this.value,
      this.game
    );

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
  checkerboard: string;
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
  private checkerboardDump: string | null = null;

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
        // initialize all fields as black
        this.data[r].push(new Field(r, c, FieldModes.BLACK, undefined, this));
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
      checkerboard: this.getCheckerboardDump(),
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
    this.getUserFields().forEach((f) => {
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

  private getCheckerboardDump(): string {
    if (this.checkerboardDump === null) {
      const cb: boolean[][] = [];
      for (const row of this.data) {
        cb.push(
          row.map(
            (f) =>
              f.mode === FieldModes.BLACK || f.mode === FieldModes.BLACKKNOWN
          )
        );
      }

      this.checkerboardDump = EncoderModule.encodeGridToBase64Url(cb);
    }

    return this.checkerboardDump;
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

  private getEncoder() {
    return new BitmaskEncoder({
      compressionThreshold: 48,
      minCompressionRatio: 0.9,
      maxN: 12,
    });
  }

  private getUserFields() {
    return Array.from(this.loopFields(), (x) => x.field).filter(
      (x) => x.mode === FieldModes.USER
    );
  }

  private dumpStateBase64(): string {
    const encoder = this.getEncoder();
    var data = this.getState();
    const encoded = encoder.encodeUncompressed(this.size, data);
    return encoded.base64Data;
  }

  private getState() {
    return this.getUserFields().map((f) => (f.user ? [f.user] : f.notes));
  }

  async dumpStateBase64Async(): Promise<string> {
    const encoder = this.getEncoder();

    var data = this.getState();

    const encoded = await encoder.encodeAsync(this.size, data);
    return encoded.base64Data;
  }

  async restoreStateBase64Async(base64Data: string) {
    const userFields = this.getUserFields();
    const count = userFields.length;
    const decoded = await this.getEncoder().decodeAsync(
      { base64Data, count },
      this.size
    );

    for (let i = 0; i < count; i++) {
      userFields[i].reset(toFieldUserData(decoded[i]));
    }
  }

  private *loopFields(): IterableIterator<{ field: Field } & FieldIndex> {
    for (let r = 0; r < this.size; r++) {
      for (let c = 0; c < this.size; c++) {
        const field = this.data[r][c];
        yield { field, row: r, col: c };
      }
    }
  }

  private forEachField(
    iteratorFunction: (f: Field, r: number, c: number) => void
  ) {
    for (const { field, row, col } of this.loopFields()) {
      iteratorFunction(field, row, col);
    }
  }

  showSolution() {
    if (this.isSolved) {
      return;
    }

    this.isSolved = true;
    this.unselectActiveField();
    this.forEachField((field) => {
      field.showSolution();
    });
  }

  private checkWrong(checkNotes = false) {
    let result = false;
    this.forEachField((field) => {
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
    this.forEachField((field) => {
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
      this.unselectActiveField();
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
    if (this.checkWrong(false) || this.checkWrong(true)) {
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
    this.checkWrong();
  }

  restart() {
    this.isSolved = false;
    this.forEachField((field) => field.reset());
  }

  getActiveField() {
    return this.activeFieldIndex
      ? this.get(this.activeFieldIndex.row, this.activeFieldIndex.col)
      : null;
  }

  private unselectActiveField() {
    const activeField = this.getActiveField();
    if (activeField) {
      this.activeFieldIndex = null;
      activeField.render();
    }
  }

  setActiveField(row: number, col: number) {
    if (!this.isSolved && this.get(row, col).isEditable()) {
      // Reset previously selected field
      this.unselectActiveField();

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
    var newCell = this.findNextEditableField(row, col, dy, dx);
    this.setActiveField(newCell.row, newCell.col);
  }

  private findNextEditableField(
    row: number,
    col: number,
    rowDelta: number,
    colDelta: number
  ) {
    const move = (value: number, delta: number) => {
      return (value + delta + this.size) % this.size;
    };

    let newRow = move(row, rowDelta);
    let newCol = move(col, colDelta);

    let success = this.get(newRow, newCol).isEditable();

    if (!success && rowDelta != 0) {
      // If new field is not editable, try to find the next
      // editable field in the target column by moving further
      // in the direction of rowDelta.
      const one = Math.sign(rowDelta);
      for (let stepped = 0; stepped < this.size; stepped++) {
        newRow = move(newRow, one);
        if (this.get(newRow, newCol).isEditable()) {
          success = true;
          break;
        }
      }
    }

    if (!success && colDelta != 0) {
      // If new field is not editable, try to find the next
      // editable field in the target row by moving further
      // in the direction of colDelta.
      const one = Math.sign(colDelta);
      for (let stepped = 0; stepped < this.size; stepped++) {
        newCol = move(newCol, one);
        if (this.get(newRow, newCol).isEditable()) {
          success = true;
          break;
        }
      }
    }

    return success ? { row: newRow, col: newCol } : { row, col };
  }

  parseGameCode(base64urlEncodedGameCode: string): Game | null {
    return GameReader.createGame(
      base64urlEncodedGameCode,
      (n) => new GameBuilder(new Game(this.renderer, n))
    );
  }

  toJsonArray() {
    return this.data.map((row) => row.map((field) => field.toJsonArray()));
  }
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

class GameBuilder {
  constructor(private game: Game) {}

  setField(row: number, col: number, mode: number, value: number) {
    const field = new Field(row, col, mode, value, this.game);
    this.game.data[row][col] = field;
    field.render();
  }

  getGame() {
    return this.game;
  }
}
