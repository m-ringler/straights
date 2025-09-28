// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const modes = { USER: 0, WHITEKNOWN: 1, BLACK: 2, BLACKKNOWN: 3 }

const MIN_GRID_SIZE_V128 = 4
const minCodeSizeV2 = 82
const minCodeSizeV128 = (
    8 /* ENCODINGVERSION */ +
    5 /* size */ +
    2 * MIN_GRID_SIZE_V128 * MIN_GRID_SIZE_V128 /* black, known */
) / 6

export const minCodeSize = Math.min(minCodeSizeV2, minCodeSizeV128)

class Field {
    constructor(row, col, game) {
        this.row = row
        this.col = col
        this.game = game

        // fixed after initialization
        this.value = undefined
        this.mode = undefined

        // derived, only used when checking
        this.wrong = false
        this.hint = undefined
        this.isShowingSolution = false

        // working data, edited by the user
        this.user = undefined
        this.notes = new Set()
    }

    getSelector() {
        return `#ce${this.row}_${this.col}`;
    }

    setUser(input) {
        if (this.isEditable()) {
            this.wrong = false
            this.hint = undefined
            if (this.user === input) {
                this.user = undefined
            } else {
                this.user = input
            }
            this.render()
        }
    }

    isActive() {
        return this.game.activeFieldIndex &&
            this.game.activeFieldIndex.col === this.col &&
            this.game.activeFieldIndex.row === this.row;
    }

    isEditable() {
        return this.mode === modes.USER;
    }

    setNote(value) {
        if (this.isEditable()) {
            this.wrong = false
            this.hint = undefined
            this.user = undefined
            if (!this.notes.delete(value)) {
                this.notes.add(value)
            }
            this.render()
        }
    }

    clear() {
        if (this.isEditable()) {
            if (this.user) {
                this.user = undefined
            } else {
                this.notes.clear()
            }

            this.wrong = false
            this.hint = undefined
            this.render()
        }
    }

    #isSolvedCorrectly() {
        if (!this.isEditable()) {
            return 1
        }

        if (!this.user) {
            return 0
        }

        if (this.user === this.value) {
            return 1
        }

        return -1
    }

    isSolved() {
        return this.#isSolvedCorrectly() === 1
    }

    checkWrong(checkNotes = false) {
        const correct = this.#isSolvedCorrectly()
        switch (correct) {
            case -1:
                this.wrong = true
                this.render()
                break;
            case 0:
                if (checkNotes
                        && this.notes.size != 0
                        && !this.notes.has(this.value)) {
                    this.wrong = true
                    this.render()
                }
                break;
            default:
                break;
        }
    }

    showSolution() {
        this.isShowingSolution = true
        this.wrong = this.#isSolvedCorrectly() === -1
        this.render()
    }

    setHint(number) {
        this.hint = number
        if (number && this.notes.size === 0) {
            for (let i = 1; i <= this.game.size; i++) {
                this.notes.add(i)
            }
        }

        this.render()
    }

    copy() {
        const field = new Field(this.row, this.col, this.game)

        field.value = this.value
        field.mode = this.mode

        field.wrong = this.wrong
        field.isShowingSolution = this.isShowingSolution

        field.copyFrom(this)

        return field
    }

    copyFrom(field) {
        this.user = field.user
        this.notes.clear()
        for (const note of field.notes) {
            this.notes.add(note)
        }
    }

    getElement() {
        return this.game.$(this.getSelector())
    }

    reset() {
        this.user = undefined
        this.notes.clear()
        this.wrong = false
        this.hint = undefined
        this.isShowingSolution = false
        this.render()
    }

    #getBackgroundColor() {
        const colors = this.game.colors
        if (this.mode === modes.BLACK ||
            this.mode === modes.BLACKKNOWN) {
            return colors.BG_BLACK
        }

        if (this.mode === modes.WHITEKNOWN) {
            return colors.BG_WHITEKNOWN
        }

        if (this.hint) {
            return colors.BG_HINT
        }

        if (this.isActive()) {
            return this.wrong
                ? colors.BG_USER_WRONG_ACTIVE
                : colors.BG_USER_ACTIVE
        }

        return this.wrong
            ? colors.BG_USER_WRONG
            : colors.BG_USER
    }

    #getTextColor() {
        const colors = this.game.colors
        if (this.mode === modes.BLACKKNOWN || this.mode === modes.BLACK) {
            return colors.FG_BLACK
        }

        if (!this.isEditable()) {
            return colors.FG_WHITEKNOWN
        }

        if (this.wrong) {
            return colors.FG_USER_WRONG
        }

        if (this.isShowingSolution) {
            if (this.user !== this.value) {
                return colors.FG_SOLUTION
            }
        }

        return colors.FG_USER
    }

    render() {
        const element = this.getElement()
        element.empty()
        element.css('background-color', this.#getBackgroundColor())
        element.css('color', this.#getTextColor())
        if (this.isEditable()) {
            if (this.isShowingSolution) {
                element.text(this.value)
            } else {
                if (this.user) {
                    element.text(this.user)
                } else if (this.notes.size > 0) {
                    let notes = '<table class="mini" cellspacing="0">'
                    for (let i = 1; i <= this.game.size; i++) {
                        if ((i - 1) % 3 === 0) notes += '<tr>'
                        if (this.notes.has(i)) {
                            const class_attribute = this.hint === i ? ' class="hint"' : ''
                            notes += `<td${class_attribute}>${i}</td>`
                        } else {
                            notes += `<td class="transparent">${i}</td>`
                        }
                        if (i % 3 === 0) notes += '</tr>'
                    }
                    notes += '</table>'
                    element.append(notes)
                }
            }
        } else if (this.mode === modes.BLACKKNOWN) {
            element.text(this.value)
        } else if (this.mode === modes.WHITEKNOWN) {
            element.text(this.value)
        }
    }

    toJsonArray() {
        if (this.mode === modes.BLACK) {
            return [0]; // black empty field
        } else if (this.mode === modes.BLACKKNOWN) {
            return [-this.value]; // black known field
        } else if (this.mode === modes.WHITEKNOWN) {
            return [this.value]; // white known field
        } else if (this.user) {
            return [this.user]; // white field with user guess
        } else {
            return Array.from(this.notes); // white field with notes
        }
    }
}

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
    }

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
    }

    constructor($, darkMode, size = 0) {
        this.$ = $
        this.colors = darkMode ? Game.gameColorsDark : Game.gameColorsLight
        this.size = size
        this.data = []
        this.activeFieldIndex = null
        this.isSolved = false
        for (let r = 0; r < size; r++) {
            this.data.push([])
            for (let c = 0; c < size; c++) {
                this.data[r].push(new Field(r, c, this))
            }
        }
    }

    get(row, col) {
        return this.data[row][col]
    }

    dumpState() {
        return game.data.map(row =>
            row.map(field => ({
                user: field.user,
                notes: Array.from(field.notes),
            })))
    }

    restoreState(dumpedState) {
        dumpedState.forEach((row, r) => {
            row.forEach((field, c) => {
                const gameField = game.get(r, c)
                gameField.copyFrom(field)
                gameField.render()
            })
        })
    }

    #setValues(row, col, mode, value) {
        const field = new Field(row, col, this)
        this.data[row][col] = field
        this.data[row][col].mode = mode
        this.data[row][col].value = value
        field.render()
    }

    #forEachField(iteratorFunction) {
        for (let r = 0; r < this.size; r++) {
            for (let c = 0; c < this.size; c++) {
                iteratorFunction(this.data[r][c], r, c)
            }
        }
    }

    showSolution() {
        if (this.isSolved) {
            return
        }

        this.isSolved = true
        this.#unselectActiveField()
        this.#forEachField(field => {
            field.showSolution()
        })
    }

    checkWrong(checkNotes = false) {
        let result = false
        this.#forEachField(field => {
            field.checkWrong(checkNotes)
            if (field.wrong) {
                result = true
            }
        })

        return result
    }

    checkSolved() {
        let finished = true
        this.#forEachField(field => {
            if (!field.isSolved()) {
                finished = false
            }
        })

        this.isSolved = finished
        if (this.isSolved) {
            this.#unselectActiveField()
        }
    }

    restart() {
        if (this.isSolved) {
            return
        }

        this.#forEachField(field => {
            field.reset()
        })
    }

    getActiveField() {
        return this.activeFieldIndex
            ? this.get(this.activeFieldIndex.row, this.activeFieldIndex.col)
            : null;
    }

    #unselectActiveField() {
        const activeField = this.getActiveField()
        if (activeField) {
            this.activeFieldIndex = null
            activeField.render()
        }
    }

    selectCell(row, col) {
        if (!this.isSolved && this.get(row, col).isEditable()) {
            // Reset previously selected field
            this.#unselectActiveField()

            // Change background of just selected field
            this.activeFieldIndex = { row, col }
            this.getActiveField().render()
        }
    }

    moveSelection(dx, dy) {
        if (!this.activeFieldIndex) {
            return
        }
        const { row, col } = this.activeFieldIndex
        var newCell = this.#findNextEditableCell(row, col, dy, dx)
        this.selectCell(newCell.row, newCell.col)
    }

    #findNextEditableCell(row, col, rowDelta, colDelta) {
        let newRow = row
        let newCol = col
        do {
            newRow = (newRow + rowDelta + this.size) % this.size
            newCol = (newCol + colDelta + this.size) % this.size
        }
        while (
            !this.get(newRow, newCol).isEditable() &&
            (newRow !== row || newCol != col))

        return { row: newRow, col: newCol }
    }

    // Parse game
    #parseGameV128(binary) {
        const size = parseInt(binary.substring(0, 5), 2)
        const pos = 5

        const bitsPerNumber = Math.floor(Math.log2(size - 1)) + 1
        const bitsPerField = 2 + bitsPerNumber // black + known + number
        const game = new Game(this.$, this.darkMode, size)

        for (let row = 0; row < size; row++) {
            for (let col = 0; col < size; col++) {
                const fieldStart = pos + (row * size + col) * bitsPerField
                const isBlack = binary[fieldStart] === '1'
                const isKnown = binary[fieldStart + 1] === '1'

                const numberBits = binary.substring(
                    fieldStart + 2,
                    fieldStart + 2 + bitsPerNumber
                )
                const value = parseInt(numberBits, 2) + 1

                const mode = isBlack
                    ? (isKnown ? modes.BLACKKNOWN : modes.BLACK)
                    : (isKnown ? modes.WHITEKNOWN : modes.USER)

                game.#setValues(row, col, mode, value)
            }
        }

        return game
    }

    #parseGameV001(binary) {
        game = new Game(this.$, darkMode, 9)
        if (binary.length < (6 * 81)) return // Invalid data
        for (let i = 0; i < 81; i++) {
            const subBinary = binary.substring(i * 6, (i + 1) * 6)
            const mode = parseInt(subBinary.substring(0, 2), 2)
            const value = parseInt(subBinary.substring(2, 6), 2) + 1
            game.#setValues(Math.floor(i / 9), i % 9, mode, value)
        }
        binary = binary.substring(6 * 81)
        let counter = 0
        while (binary.length >= 7 && counter < (4 - difficulty) * 3.5) {
            const position = parseInt(binary.substring(0, 7), 2)
            game.get(Math.floor(position / 9), position % 9).mode = modes.WHITEKNOWN
            game.get(Math.floor(position / 9), position % 9).render()
            binary = binary.substring(7)
            counter++
        }

        return game
    }

    #parseGameV002(binary) {
        game = new Game(this.$, darkMode, 9)
        if (binary.length < (6 * 81) || binary.length > (6 * 81 + 8)) return // Invalid data
        for (let i = 0; i < 81; i++) {
            const subBinary = binary.substring(i * 6, (i + 1) * 6)
            const mode = parseInt(subBinary.substring(0, 2), 2)
            const value = parseInt(subBinary.substring(2, 6), 2) + 1
            game.#setValues(Math.floor(i / 9), i % 9, mode, value)
        }

        return game
    }

    #decode(code) {
        const base64urlCharacters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_'
        let binary = ''
        for (let i = 0; i < code.length; i++) {
            let b = base64urlCharacters.indexOf(code.charAt(i)).toString(2)
            while (b.length < 6) b = '0' + b
            binary += b
        }
        const encodingVersion = parseInt(binary.substring(0, 8), 2)
        binary = binary.substring(8)

        return { encodingVersion, binary }
    }

    parseGame(code) {
        const decoded = this.#decode(code)
        switch (decoded.encodingVersion) {
            case 1:
                return this.#parseGameV001(decoded.binary)
            case 128:
                // 0b10000000: arbitrary size game encoding
                return this.#parseGameV128(decoded.binary)
            default:
                return this.#parseGameV002(decoded.binary);
        }
    }

    toJsonArray() {
        return this.data.map(row =>
            row.map(field => field.toJsonArray())
        );
    }
}
