// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// Global Constants
function _getButtonColors(darkMode) {
    const buttonColorsLight = {
        BUTTONDOWN: '#335',
        BUTTONUP: '#b1cffc'
    }

    const buttonColorsDark = {
        BUTTONDOWN: '#e7d9cdff',/* color-button-active */
        BUTTONUP: '#555'  /* color-button-background */
    }

    return darkMode ? buttonColorsDark : buttonColorsLight
}

const _darkMode = window.matchMedia('(prefers-color-scheme: dark)').matches

const _buttonColors = _getButtonColors(_darkMode)

const dialogs = { WELCOME: 1, GENERATED: 2, LOADING: 3, SOLUTION: 4, RESTART: 5, ABOUT: 6, HINT: 7 }
const MIN_GRID_SIZE = 4
const MAX_GRID_SIZE = 12
const DEFAULT_GRID_SIZE = 9
const DEFAULT_DIFFICULTY = 3

// Global Variables
let _starttime
let _timer
let _noteMode = false
let _minCodeSize
let _game
let _gameCode
let _gameHistory
let _gameUrl
let _difficulty = DEFAULT_DIFFICULTY
let _currentGridSize = 12
let _generateGridSize = DEFAULT_GRID_SIZE
let _undoStack
let _hintField

// imported functions
let _generate
let _generateHint

const _modulePromise = _importModules()

async function _importModules() {
    const undoStackModule = await import('./undoStack.js')
    _undoStack = new undoStackModule.UndoStack(_renderUndoButton)

    const gameModule = await import('./game.js')
    _game = new gameModule.Game($, _darkMode)
    _minCodeSize = gameModule.minCodeSize

    _gameHistory = await import('./gameHistory.js')

    const generateModule = await import('./generate-str8ts.js')
    loadedFunctions = generateModule.load_generate()
    _generate = loadedFunctions.generate
    _generateHint = loadedFunctions.generateHint
}

// Button Functions
function restart() {
    showDialog(false)
    _game.restart()
    _undoStack.clear()
}

function toggleNoteMode() {
    _noteMode = !_noteMode
    const color = (_noteMode) ? _buttonColors.BUTTONDOWN : _buttonColors.BUTTONUP
    $('#notes').css('background-color', color)
}

function _renderCounters() {
    $('#check-counter').text(_game.check_count)
    $('#hint-counter').text(_game.hint_count)
}

function check() {
    _game.check()
    _renderCounters()
    _saveState()
}

async function hint() {
    closeHint()

    const checkResult = _game.checkForHint()
    _renderCounters()

    if (!(checkResult.isSolved || checkResult.isWrong)) {
        await generateAndDisplayHint()
    }

    _saveState()

    async function generateAndDisplayHint() {
        const hintResponse = await _generateHint(_game.toJsonArray())
        if (hintResponse.status === 0 && hintResponse.message) {
            const hintData = JSON.parse(hintResponse.message)
            _hintField = _game.get(hintData.y, hintData.x)
            _hintField.setHint(hintData.number)

            // hintData.rule is either ColumnNameInPascalCase
            // or BlockNameInPascalCase.
            const ruleWords = hintData.rule.split(/(?=[A-Z])/)
            ruleType = ruleWords[0]
            ruleName = ruleWords.slice(1).join(" ")
            ruleTarget = ruleType == 'Block'
                ? `${hintData.direction} block`
                : ((hintData.direction == 'horizontal') ? "row" : "column")
            $('#hint-text').html(`Hint: ${hintData.number} can be removed by applying the <a href="https://github.com/m-ringler/straights/wiki/Rules-of-Str8ts#${hintData.rule}" target="rules">${ruleName} rule</a> to the ${ruleTarget}.`)
            _positionHintDialog()
            showDialog(dialogs.HINT)
        }
        else if (hintResponse.message) {
            console.error("Failed to generate a hint:", hintResponse.message)
        }
    }
}

function closeHint() {
    if (_hintField) {
        _hintField.setHint(undefined)
        _hintField = null
        showDialog(false)
    }
}

function _positionHintDialog() {
    const field = _hintField.getElement()
    const dialog = $('#hint-dialog');
    _positionPopup(field, dialog)
}

function _positionPopup(target, popup) {
    // Get the position of the field relative to the viewport
    const targetPos = target[0].getBoundingClientRect();
    const windowHeight = $(window).height();
    const windowWidth = $(window).width();

    // Determine the vertical position
    let popupTop;
    if ((targetPos.top + targetPos.height / 2) > windowHeight / 2) {
        popupTop = targetPos.top + window.scrollY - popup.outerHeight();
    } else {
        popupTop = targetPos.top + window.scrollY + targetPos.height;
    }

    // Determine the horizontal position
    let popupLeft;
    if ((targetPos.left + targetPos.width / 2) > windowWidth / 2) {
        popupLeft = targetPos.left + window.scrollX - popup.outerWidth();
    } else {
        popupLeft = targetPos.left + window.scrollX + targetPos.width;
    }

    // Set the position of the dialog
    popup.css({
        position: 'absolute',
        top: popupTop,
        left: popupLeft
    });
}

function solution() {
    showDialog(false)
    clearInterval(_timer)
    _game.showSolution()
    _undoStack.clear()
}

function undo() {
    if (_undoStack.length > 0 && !_game.isSolved) {
        const field = _undoStack.pop()
        const gameField = _game.get(field.row, field.col)
        gameField.copyFrom(field)
        gameField.wrong = false
        _game.selectCell(field.row, field.col)
        gameField.render()
    }
}

function _renderUndoButton(length) {
    const undoButton = $('#undo')
    if (length == 0 || _game.isSolved) {
        undoButton.prop('disabled', true)
        undoButton.attr('disabled', 'disabled') // Ensure attribute is present for CSS to update
    } else {
        undoButton.prop('disabled', false)
        undoButton.removeAttr('disabled') // Ensure attribute is removed for CSS to update
    }
}

// General Functions
function _changeGridSize(newGridSize) {
    if (newGridSize == _currentGridSize) {
        return
    }

    _currentGridSize = Math.min(newGridSize, MAX_GRID_SIZE)
    _currentGridSize = Math.max(_currentGridSize, MIN_GRID_SIZE)
    _showHideButtonsAndCells()
}

function _showHideButtonsAndCells() {
    for (let i = 1; i <= _currentGridSize; i++) {
        $(`td[data-button="bn${i}"]`).show()
    }

    for (let i = _currentGridSize + 1; i <= MAX_GRID_SIZE; i++) {
        $(`td[data-button="bn${i}"]`).hide()
    }

    for (let r = 0; r < _currentGridSize; r++) {
        $('#r' + r).show()
        for (let c = 0; c < _currentGridSize; c++) {
            $(`#ce${r}_${c}`).show()
        }
        for (let c = _currentGridSize; c < MAX_GRID_SIZE; c++) {
            $(`#ce${r}_${c}`).hide()
        }
    }
    for (let r = _currentGridSize; r < MAX_GRID_SIZE; r++) {
        $('#r' + r).hide()
    }
}

function _createGrid() {
    for (let r = 0; r < MAX_GRID_SIZE; r++) {
        let row = `<tr class="row" id="r${r}" row="${r}">`
        for (let c = 0; c < MAX_GRID_SIZE; c++) {
            row += `<td class="cell" id="ce${r}_${c}" row="${r}" col="${c}"></td>`
        }
        row += '</tr>'
        $('.container').append(row)
    }
}

function _restartTimer() {
    _starttime = (new Date()).getTime()
    _timer = setInterval(function () {
        const diff = (new Date()).getTime() - _starttime
        const minutes = Math.floor(diff / 60000)
        const seconds = Math.floor(diff / 1000 - minutes * 60)
        $('#time').text(((minutes < 10) ? '0' : '') + minutes + ':' + ((seconds < 10) ? '0' : '') + seconds)
    }, 1000)
}

function _getURLParameter(name) {
    if (!window.location.search) return null
    const urlParams = new URLSearchParams(window.location.search)
    return urlParams.get(name)
}

async function loadNewGame() {
    showDialog(dialogs.LOADING)
    clearInterval(_timer)
    $('#generate-button').prop('disabled', true)
    try {
        const data = await _generate(_generateGridSize, _difficulty)
        if (data.status === 0 && data.message.length > _minCodeSize) {
            console.log('Game:', data.message)
            _gameUrl = window.location.href.split('?')[0] + '?code=' + data.message
            _gameCode = data.message
            showDialog(dialogs.GENERATED)
            return
        } else {
            console.error('Error generating game:', data.message)
        }
    } catch (error) {
        console.error('Error fetching game:', error)
    }
    showDialog(false)
    $('#generate-button').prop('disabled', false)
}

function _loadSettings() {
    function loadSetting(sliderId, storageKey, defaultValue) {
        const slider = $(`#${sliderId}`)
        const storedValue = localStorage.getItem(storageKey)

        let validatedValue = defaultValue
        if (storedValue !== null) {
            const value = Number(storedValue)
            const min = Number(slider.attr('min'))
            const max = Number(slider.attr('max'))

            if (value >= min && value <= max) {
                validatedValue = value
            }
        }

        slider.val(validatedValue)
        $(`#${sliderId.replace('-slider', '')}`).text(validatedValue)
        return validatedValue
    }

    try {
        _generateGridSize = loadSetting('grid-size-slider', 'generate.gridSize', DEFAULT_GRID_SIZE)
        _difficulty = loadSetting('difficulty-slider', 'generate.difficulty', DEFAULT_DIFFICULTY)
    } catch (error) {
        console.warn('Failed to load settings:', error, "Using defaults.")
        _generateGridSize = DEFAULT_GRID_SIZE
        _difficulty = DEFAULT_DIFFICULTY
    }
}

function changeDifficulty() {
    _difficulty = Number($('#difficulty-slider').val())
    $('#difficulty').text(_difficulty)
    localStorage.setItem('generate.difficulty', _difficulty)
}

function changeGenerateSize() {
    _generateGridSize = Number($('#grid-size-slider').val())
    $('#grid-size').text(_generateGridSize)
    localStorage.setItem('generate.gridSize', _generateGridSize)
}

async function _startGame() {
    let hasGame = false
    if (_gameCode && _gameCode.length > _minCodeSize) {
        _undoStack.clear()
        $('.container').removeClass('finished')
        showDialog(false)

        _game = _game.parseGame(_gameCode)
        if (_game) {
            hasGame = true

            _changeGridSize(_game.size)

            _gameHistory.restoreGameState(_gameCode, _game)

            _restartTimer()
            _renderCounters()
        }
    }

    if (!hasGame) {
        await loadNewGame()
    }
}

function loadNewGameAgain() {
    showDialog(dialogs.WELCOME)
    $('#cancel-new-game').show()
}

function showDialog(dialog) {
    $('#welcome-dialog').hide()
    $('#start-dialog').hide()
    $('#loading-dialog').hide()
    $('#solution-dialog').hide()
    $('#restart-dialog').hide()
    $('#about-dialog').hide()
    $('#hint-dialog').hide()
    if (dialog != dialogs.HINT) {
        closeHint()
    }

    if (dialog) {
        $('.dialog-outer-container').show()
        switch (dialog) {
            case dialogs.LOADING:
                $('#loading-dialog').show()
                break
            case dialogs.WELCOME:
                $('#welcome-dialog').show()
                break
            case dialogs.GENERATED:
                window.history.pushState({}, '', _gameUrl)
                _startGame()
                break
            case dialogs.SOLUTION:
                if (!_game.isSolved) {
                    $('#solution-dialog').show()
                } else {
                    $('.dialog-outer-container').hide()
                }
                break
            case dialogs.RESTART:
                if (!_game.isSolved) {
                    $('#restart-dialog').show()
                } else {
                    $('.dialog-outer-container').hide()
                }
                break
            case dialogs.ABOUT:
                $('#current-game').attr('href', window.location.href)
                $('#about-dialog').show()
                break
            case dialogs.HINT:
                $('#hint-dialog').show()
                break
        }
    } else {
        $('.dialog-outer-container').hide()
    }
}

function _onKeyDown(e) {
    if (_game.isSolved) return

    let handled = false
    const key = e.key
    if (_handleCursorKey(e)) {
        handled = true
    } else if (key >= '0' && key <= '9') {
        _handleNumberKey(Number(key))
        handled = true
    } else if (key == 'n') {
        toggleNoteMode()
        handled = true
    } else if (key == 'z' && e.ctrlKey) {
        undo()
        handled = true
    } else if (key === 'Backspace' || key === 'Delete') {
        _handleDelete()
        handled = true
    }

    if (handled) {
        e.preventDefault()
    }
}

function _handleCursorKey(e) {
    switch (e.which) {
        case 37: // left
            _game.moveSelection(-1, 0)
            break
        case 38: // up
            _game.moveSelection(0, -1)
            break
        case 39: // right
            _game.moveSelection(1, 0)
            break
        case 40: // down
            _game.moveSelection(0, 1)
            break
        default:
            return false
    }

    return true
}

let _firstDigit = null
let _digitTimer = null
const _twoDigitTimeout = 500
function _handleNumberKey(num) {
    if (_firstDigit == null) {
        if (_currentGridSize < 10 || num !== 1) {
            _handleNumberInput(num)
        } else {
            _firstDigit = num
            _digitTimer = setTimeout(() => {
                _handleNumberInput(_firstDigit)
                _firstDigit = null
            }, _twoDigitTimeout)
        }
    } else {
        clearTimeout(_digitTimer)
        const firstNum = _firstDigit
        const combinedNum = _firstDigit * 10 + num
        _firstDigit = null
        if (combinedNum <= _currentGridSize) {
            _handleNumberInput(combinedNum)
        } else {
            _handleNumberInput(firstNum)
            _handleNumberKey(num)
        }
    }
}

function _handleNumberInput(num) {
    if (num < 1 || num > _currentGridSize) {
        return
    }

    const activeField = _game.getActiveField()
    if (!activeField || !activeField.isEditable()) {
        return
    }

    _undoStack.push(activeField.copy())

    if (_noteMode) {
        activeField.setNote(num)
    } else {
        activeField.setUser(num)
        _game.checkSolved()

        if (_game.isSolved) {
            _undoStack.clear()
            $('.container').addClass('finished')
            _onResize()
            clearInterval(_timer)
        }
    }

    _saveState()
}

function _saveState() {
    _gameHistory.saveGameState(_gameCode, _game)
}

function _handleDelete() {
    const field = _game.getActiveField()
    if (!field || !field.isEditable()) {
        return
    }

    _undoStack.push(field.copy())
    field.clear()
}

async function copyCurrentLink() {
    try {
        await navigator.clipboard.writeText(window.location.href)
        const copyBtn = $('.copy-link')
        copyBtn.text('Link copied!')
        setTimeout(() => copyBtn.text('🔗'), 1000)
    } catch (err) {
        console.error('Failed to copy:', err)
    }
}

$(document).ready(async function () {
    await _modulePromise
    _createGrid()
    _onResize()
    _loadSettings()
    _handleGameLoad()
    $('td[id^="ce"]').click(function () { // Game fields
        const row = Number($(this).attr('row'))
        const col = Number($(this).attr('col'))
        _game.selectCell(row, col)
    })
    $('td[data-button^="bn"]').click(function () { // Number buttons
        const num = Number($(this).text())
        _handleNumberInput(num)
    })
    _onResize()
})

window.addEventListener('popstate', function () {
    _handleGameLoad(popstate = true)
})

function _handleGameLoad(popstate = false) {
    const code = _getURLParameter('code')
    const currentKey = window.location.href

    if (code && code.length > _minCodeSize) {
        _gameUrl = currentKey
        _gameCode = code
        if (popstate) {
            _startGame()
        } else {
            showDialog(dialogs.GENERATED)
        }
    } else {
        const latestKey = _gameHistory.getLatestGameKey()
        if (latestKey) {
            // Reload the current page with the latest game code
            window.location.href = window.location.href.split('?')[0] + '?code=' + latestKey
            return
        }

        // Nothing stored, show welcome dialog.
        showDialog(dialogs.WELCOME)
    }
}

$(document).keydown(_onKeyDown)

$(window).resize(_onResize)
function _onResize() {
    closeHint()
    if (window.innerWidth / 2 - 45 < $('.controls').position().left) { // Large screen
        $('#buttons-small').hide()
        $('#buttons-large').show()
        $('.cell').css({ 'font-size': '22pt', width: '41px', height: '41px' })
        $('.mini').css('font-size', '9pt')
        $('#hint-dialog').css('width', '235px')
    } else { // Small screen
        const cellwidth = Math.min(Math.floor(window.innerWidth / _currentGridSize - 2), 41)
        $('#buttons-small').show()
        $('#buttons-large').hide()
        $('.container').css({ margin: '5px 2px' })
        $('.controls').css({ margin: '0px 2px' })
        $('.cell').css({ 'font-size': '17pt', width: `${cellwidth}px`, height: `${cellwidth}px` })
        $('.mini').css('font-size', '8pt')
        $('#hint-dialog').css('width', '150px')
    }
}

