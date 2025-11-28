// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { BitmaskEncoder } from './encoder.js';
import type { EncodedResult } from './encoder.js';

const modes = {
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

    getSelector() {
        return `#ce${this.row}_${this.col}`;
    }

    setUser(input: number) {
        if (this.isEditable()) {
            this.wrong = false;
            this.hint = undefined;
            if (this.user === input) {
                this.user = undefined;
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
        return this.mode === modes.USER;
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

    getElement() {
        return this.game.$(this.getSelector());
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

    #getBackgroundColor() {
        const colors = this.game.colors;
        if (this.mode === modes.BLACK || this.mode === modes.BLACKKNOWN) {
            return colors.BG_BLACK;
        }

        if (this.mode === modes.WHITEKNOWN) {
            return colors.BG_WHITEKNOWN;
        }

        if (this.hint) {
            return colors.BG_HINT;
        }

        if (this.isActive()) {
            return this.wrong
                ? colors.BG_USER_WRONG_ACTIVE
                : colors.BG_USER_ACTIVE;
        }

        return this.wrong ? colors.BG_USER_WRONG : colors.BG_USER;
    }

    #getTextColor() {
        const colors = this.game.colors;
        if (this.mode === modes.BLACKKNOWN || this.mode === modes.BLACK) {
            return colors.FG_BLACK;
        }

        if (!this.isEditable()) {
            return colors.FG_WHITEKNOWN;
        }

        if (this.wrong) {
            return colors.FG_USER_WRONG;
        }

        if (this.isShowingSolution) {
            if (this.user !== this.value) {
                return colors.FG_SOLUTION;
            }
        }

        return colors.FG_USER;
    }

    render() {
        const element = this.getElement();
        element.empty();
        element.css('background-color', this.#getBackgroundColor());
        element.css('color', this.#getTextColor());
        if (this.isEditable()) {
            if (this.isShowingSolution) {
                element.text(this.value!);
            } else {
                if (this.user) {
                    element.text(this.user);
                } else if (this.notes.size > 0) {
                    let notes = '<table class="mini" cellspacing="0">';
                    for (let i = 1; i <= this.game.size; i++) {
                        if ((i - 1) % 3 === 0) notes += '<tr>';
                        if (this.notes.has(i)) {
                            const class_attribute =
                                this.hint === i ? ' class="hint"' : '';
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
        } else if (this.mode === modes.BLACKKNOWN) {
            element.text(this.value!);
        } else if (this.mode === modes.WHITEKNOWN) {
            element.text(this.value!);
        }
    }

    toJsonArray() {
        if (this.mode === modes.BLACK) {
            return [0]; // black empty field
        } else if (this.mode === modes.BLACKKNOWN) {
            return [-this.value!]; // black known field
        } else if (this.mode === modes.WHITEKNOWN) {
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
    user: number | undefined;
    notes: Iterable<number>;
}

export type DumpedState = {
    check_count: number;
    hint_count: number;
    data: FieldUserData[][];
};

// class to store and modify the current game state
export class Game {
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
    $: JQueryStatic;
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
    darkMode: boolean;
    size: number;
    data: Field[][];
    isSolved: boolean;
    activeFieldIndex: null | FieldIndex;
    check_count: number;
    hint_count: number;

    constructor($: any, darkMode: boolean, size: number = 0) {
        this.$ = $;
        this.colors = darkMode ? Game.gameColorsDark : Game.gameColorsLight;
        this.darkMode = darkMode;
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
    }

    get(row: number, col: number): Field {
        return this.data[row][col];
    }

    dumpState(): DumpedState {
        const fieldData = this.data.map((row) =>
            row.map((field) => ({
                user: field.user,
                notes: Array.from(field.notes),
            }))
        );

        const result = {
            check_count: this.check_count,
            hint_count: this.hint_count,
            data: fieldData,
        };

        return result;
    }

    restoreState(dumpedState: FieldUserData[][] | DumpedState) {
        if (Object.hasOwn(dumpedState, 'check_count')) {
            // new format including check and hint count
            const ds = dumpedState as DumpedState;
            this.check_count = ds.check_count;
            this.hint_count = ds.hint_count;
            this.restoreState(ds.data);
        } else {
            // old format (just field data) also used in
            // recursive call from above
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
            (x) => x.mode === modes.USER
        );
    }

    async dumpStateBase64(): Promise<string> {
        const encoder = this.#getEncoder();

        var data = this.#getUserFields().map((f) =>
            f.user ? [f.user] : f.notes
        );

        const encoded = await encoder.encode(this.size, data);
        return encoded.base64Data;
    }

    async restoreStateBase64(base64Data: string) {
        const userFields = this.#getUserFields();
        const count = userFields.length;
        const decoded = await this.#getEncoder().decode(
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
        const result = new Game(this.$, this.darkMode, size);

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

                const mode = isBlack
                    ? isKnown
                        ? modes.BLACKKNOWN
                        : modes.BLACK
                    : isKnown
                      ? modes.WHITEKNOWN
                      : modes.USER;

                result.#setValues(row, col, mode, value);
            }
        }

        return result;
    }

    #parseGameV002(binary: string) {
        const result = new Game(this.$, this.darkMode, 9);
        if (binary.length < 6 * 81 || binary.length > 6 * 81 + 8) return; // Invalid data
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
        switch (decoded.encodingVersion) {
            case 1:
                // not supported any more
                return null;
            case 128:
                // 0b10000000: arbitrary size game encoding
                return this.#parseGameV128(decoded.binary);
            default:
                return this.#parseGameV002(decoded.binary);
        }
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
