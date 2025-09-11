// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const buttonColorsLight = {
  BUTTONDOWN: '#335',
  BUTTONUP: '#b1cffc'
}

const buttonColorsDark = {
  BUTTONDOWN: '#777',/* color-button-active */
  BUTTONUP: '#555'  /* color-button-background */
}

const darkMode = window.matchMedia('(prefers-color-scheme: dark)').matches

const buttonColors = darkMode ? buttonColorsDark : buttonColorsLight

const dialogs = { WELCOME: 1, GENERATED: 2, LOADING: 3, SOLUTION: 4, EMPTY: 5 }
const MIN_GRID_SIZE = 4
const MAX_GRID_SIZE = 12
const DEFAULT_GRID_SIZE = 9

// Variables
let starttime
let timer
let count = 0
let noteMode = false
let game
let minCodeSize
let gameCode
let gameUrl
let difficulty = 3
let currentGridSize = 12
let generateGridSize = 9
let undoStack
let gameHistory
let generate

const modulePromise = importModules()

async function importModules() {
  const undoStackModule = await import('./undoStack.js');
  undoStack = new undoStackModule.UndoStack(updateUndoButton);

  const gameModule = await import('./game.js');
  game = new gameModule.Game($, darkMode)
  minCodeSize = gameModule.minCodeSize

  gameHistory = await import('./gameHistory.js');

  const generateModule = await import('./generate-str8ts.js');
  generate = generateModule.load_generate()
}

// Button Functions
function restart() {
  showDialog(false)
  game.restart()
  undoStack.clear()
}

function toggleNoteMode() {
  noteMode = !noteMode
  const color = (noteMode) ? buttonColors.BUTTONDOWN : buttonColors.BUTTONUP
  $('#notes').css('background-color', color)
}

function check() {
  count++
  $('#counter').text(count)
  game.checkWrong()
}

function solution() {
  showDialog(false)
  clearInterval(timer)
  game.showSolution()
  undoStack.clear()
}

function undo() {
  if (undoStack.length > 0 && !game.isSolved) {
    const field = undoStack.pop()
    gameField = game.get(field.row, field.col)
    gameField.copyFrom(field)
    game.selectCell(field.row, field.col)
  }
}

function updateUndoButton(length) {
  const $undo = $('#undo')
  if (length == 0 || game.isSolved) {
    $undo.prop('disabled', true)
    $undo.attr('disabled', 'disabled') // Ensure attribute is present for CSS to update
  } else {
    $undo.prop('disabled', false)
    $undo.removeAttr('disabled') // Ensure attribute is removed for CSS to update
  }
}

// General Functions
function changeGridSize(newGridSize) {
  if (newGridSize == currentGridSize) {
    return
  }

  currentGridSize = Math.min(newGridSize, MAX_GRID_SIZE)
  currentGridSize = Math.max(currentGridSize, MIN_GRID_SIZE)
  showHideButtonsAndCells()
}

function showHideButtonsAndCells() {
  for (let i = 1; i <= currentGridSize; i++) {
    $(`td[data-button="bn${i}"]`).show()
  }

  for (let i = currentGridSize + 1; i <= MAX_GRID_SIZE; i++) {
    $(`td[data-button="bn${i}"]`).hide()
  }

  for (let r = 0; r < currentGridSize; r++) {
    $('#r' + r).show()
    for (let c = 0; c < currentGridSize; c++) {
      $(`#ce${r}_${c}`).show()
    }
    for (let c = currentGridSize; c < MAX_GRID_SIZE; c++) {
      $(`#ce${r}_${c}`).hide()
    }
  }
  for (let r = currentGridSize; r < MAX_GRID_SIZE; r++) {
    $('#r' + r).hide()
  }
}

function setup() {
  for (let r = 0; r < MAX_GRID_SIZE; r++) {
    let row = `<tr class="row" id="r${r}" row="${r}">`
    for (let c = 0; c < MAX_GRID_SIZE; c++) {
      row += `<td class="cell" id="ce${r}_${c}" row="${r}" col="${c}"></td>`
    }
    row += '</tr>'
    $('.container').append(row)
  }
}

function restartTimer() {
  starttime = (new Date()).getTime()
  timer = setInterval(function () {
    const diff = (new Date()).getTime() - starttime
    const minutes = Math.floor(diff / 60000)
    const seconds = Math.floor(diff / 1000 - minutes * 60)
    $('#time').text(((minutes < 10) ? '0' : '') + minutes + ':' + ((seconds < 10) ? '0' : '') + seconds)
  }, 1000)
}

function getURLParameter(name) {
  if (!window.location.search) return null
  const urlParams = new URLSearchParams(window.location.search)
  return urlParams.get(name)
}

async function loadNewGame() {
  showDialog(dialogs.LOADING)
  clearInterval(timer)
  $('#generate-button').prop('disabled', true)
  try {
    const data = await generate(generateGridSize, difficulty)
    if (data.status === 0 && data.message.length > minCodeSize) {
      console.log('Game:', data.message)
      gameUrl = window.location.href.split('?')[0] + '?code=' + data.message
      gameCode = data.message
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

function changeDifficulty() {
  difficulty = Number($('#difficulty-slider').val())
  $('#difficulty').text(difficulty)
}

function changeGenerateSize() {
  generateGridSize = Number($('#grid-size-slider').val())
  $('#grid-size').text(generateGridSize)
}

async function startGame() {
  if (gameCode && gameCode.length > minCodeSize) {
    undoStack.clear()
    count = 0
    $('#counter').text(count)
    $('.container').removeClass('finished')
    showDialog(false)

    game = game.parseGame(gameCode)
    changeGridSize(game.size)

    gameHistory.restoreGameState(gameCode, game)

    restartTimer()
  } else {
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
  $('#empty-dialog').hide()
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
        window.history.pushState({}, '', gameUrl)
        startGame()
        break
      case dialogs.SOLUTION:
        if (!game.isSolved) {
          $('#solution-dialog').show()
        } else {
          $('.dialog-outer-container').hide()
        }
        break
      case dialogs.EMPTY:
        if (!game.isSolved) {
          $('#empty-dialog').show()
        } else {
          $('.dialog-outer-container').hide()
        }
        break
    }
  } else {
    $('.dialog-outer-container').hide()
  }
}

function onKeyDown(e) {
  if (game.isSolved) return

  let handled = false
  const key = e.key
  if (handleCursorKey(e)) {
    handled = true
  } else if (key >= '0' && key <= '9') {
    handleNumberKey(Number(key))
    handled = true
  } else if (key == 'n') {
    toggleNoteMode()
    handled = true
  } else if (key == 'z' && e.ctrlKey) {
    undo()
    handled = true
  } else if (key === 'Backspace' || key === 'Delete') {
    handleDelete()
    handled = true
  }

  if (handled) {
    e.preventDefault()
  }
}

function handleCursorKey(e) {
  switch (e.which) {
    case 37: // left
      game.moveSelection(-1, 0)
      break
    case 38: // up
      game.moveSelection(0, -1)
      break
    case 39: // right
      game.moveSelection(1, 0)
      break
    case 40: // down
      game.moveSelection(0, 1)
      break
    default:
      return false
  }

  return true
}

let firstDigit = null
let digitTimer = null
const twoDigitTimeout = 500
function handleNumberKey(num) {
  if (firstDigit == null) {
    if (currentGridSize < 10 || num !== 1) {
      handleNumberInput(num)
    } else {
      firstDigit = num
      digitTimer = setTimeout(() => {
        handleNumberInput(firstDigit)
        firstDigit = null
      }, twoDigitTimeout)
    }
  } else {
    clearTimeout(digitTimer)
    const firstNum = firstDigit
    const combinedNum = firstDigit * 10 + num
    firstDigit = null
    if (combinedNum <= currentGridSize) {
      handleNumberInput(combinedNum)
    } else {
      handleNumberInput(firstNum)
      handleNumberKey(num)
    }
  }
}

function handleNumberInput(num) {
  if (num < 1 || num > currentGridSize) {
    return
  }

  const activeField = game.getActiveField()
  if (!activeField || !activeField.isEditable()) {
    return
  }

  undoStack.push(activeField.copy())

  if (noteMode) {
    activeField.setNote(num)
  } else {
    activeField.setUser(num)
    game.checkSolved()

    if (game.isSolved) {
      undoStack.clear()
      $('.container').addClass('finished')
      onResize()
      clearInterval(timer)
    }
  }

  gameHistory.saveGameState(gameCode, game)
}

function handleDelete() {
  const field = game.getActiveField()
  if (!field || !field.isEditable()) {
    return
  }

  undoStack.push(field.copy())
  field.clear()
}

$(document).ready(async function () {
  await modulePromise
  setup()
  onResize()
  handleGameLoad()
  $('td[id^="ce"]').click(function () { // Game fields
    const row = Number($(this).attr('row'))
    const col = Number($(this).attr('col'))
    game.selectCell(row, col)
  })
  $('td[data-button^="bn"]').click(function () { // Number buttons
    const num = Number($(this).text())
    handleNumberInput(num)
  })
  onResize()
})

window.addEventListener('popstate', function () {
  handleGameLoad(popstate = true)
})

function handleGameLoad(popstate = false) {
  const code = getURLParameter('code')
  const currentKey = window.location.href

  if (code && code.length > minCodeSize) {
    gameUrl = currentKey
    gameCode = code
    if (popstate) {
      startGame()
    }
    else {
      showDialog(dialogs.GENERATED)
    }
  } else {
    const latestKey = gameHistory.getLatestGameKey()
    if (latestKey) {
      // Reload the current page with the latest game code
      window.location.href = window.location.href.split('?')[0] + '?code=' + latestKey
      return
    }

    // Nothing stored, show welcome dialog.
    showDialog(dialogs.WELCOME)
  }
}

$(document).keydown(onKeyDown)

$(window).resize(onResize)
function onResize() {
  if (window.innerWidth / 2 - 45 < $('.controls').position().left) { // Large screen
    $('#buttons-small').hide()
    $('#buttons-large').show()
    $('.cell').css({ 'font-size': '22pt', width: '41px', height: '41px' })
    $('.mini').css('font-size', '9pt')
  } else { // Small screen
    const cellwidth = Math.min(Math.floor(window.innerWidth / currentGridSize - 2), 41)
    $('#buttons-small').show()
    $('#buttons-large').hide()
    $('.container').css({ margin: '5px 2px' })
    $('.controls').css({ margin: '0px 2px' })
    $('.cell').css({ 'font-size': '17pt', width: `${cellwidth}px`, height: `${cellwidth}px` })
    $('.mini').css('font-size', '8pt')
  }
}

