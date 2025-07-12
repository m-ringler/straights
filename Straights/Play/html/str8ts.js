// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// Constants
const modes = { USER: 0, KNOWN: 1, BLACK: 2, BLACKKNOWN: 3 }
const colorsLight = {
  WRONG: '#b22222',
  SOLUTION: '#cc9900',
  USER: '#003366',
  KNOWN: '#000000',
  BUTTONDOWN: '#335',
  BUTTONUP: '#b1cffc',
  WHITE: '#ffffff',
  BLACK: '#000000',
  FIELDSELECTED: '#c7ddff',
  FIELDUNSELECTED: '#ffffff'
}

const colorsDark = {
  WRONG: '#b22222',
  SOLUTION: '#cc9900',
  USER: '#222244',
  KNOWN: '#000000',
  BUTTONDOWN: '#777',/* color-button-active */
  BUTTONUP: '#555',  /* color-button-background */
  WHITE: '#aaaaaa',
  BLACK: '#000000',
  FIELDSELECTED: '#7379bf',
  FIELDUNSELECTED: '#aaaaaa' /* same as WHITE */
}

const colors =
    window.matchMedia('(prefers-color-scheme: dark)').matches
    ? colorsDark
    : colorsLight

const dialogs = { WELCOME: 1, GENERATED: 2, LOADING: 3, SOLUTION: 4, EMPTY: 5 }
const MIN_GRID_SIZE = 4
const MAX_GRID_SIZE = 12
const DEFAULT_GRID_SIZE = 9
const minCodeSizeV2 = 82
const minCodeSizeV129 = (
  8 /* ENCODINGVERSION */ +
    5 /* size */ +
    2 * MIN_GRID_SIZE * MIN_GRID_SIZE /* black, known */
) /
    6
const minCodeSize = Math.min(minCodeSizeV2, minCodeSizeV129)// Minimum code size for a valid game
const MAX_NUMBER_OF_STORED_GAMES = 50

// Variables
let starttime
let timer
let count = 0
let noteMode = false
let game
let activeRow; let activeCol
let showSolution = false
let undoStack = []
let gameCode
let gameUrl
let difficulty = 3
let currentGridSize = 12
let generateGridSize = 9

// Element class
class Field {
  constructor (row, col) {
    this.row = row
    this.col = col

    // fixed after initialization
    this.value = undefined
    this.mode = undefined

    // derived, only used when checking
    this.wrong = false
    this.solution = false

    // working data, edited by the user
    this.user = undefined
    this.notes = []
  }

  getSelector()
  {
    return `#ce${this.row}_${this.col}`;
  }

  setUser (input) {
    if (this.mode === modes.USER) {
      this.wrong = false
      if (this.user === input) {
        this.user = undefined
      } else {
        this.user = input
      }
      this.render()
    }
  }

  isActive () {
    return activeCol === this.col && activeRow === this.row;
  }

  setNote (value) {
    if (this.mode === modes.USER) {
      this.user = undefined
      if (this.notes.indexOf(value) > -1) {
        this.notes.splice(this.notes.indexOf(value), 1)
      } else {
        this.notes.push(value)
      }
      this.render()
    }
  }

  clear () {
    if (this.mode === modes.USER) {
      if (this.user) {
        this.user = undefined
      } else if (this.notes.length != 0) {
        // remove last note
        this.notes.splice(this.notes.length - 1, 1)
      }

      this.wrong = false
      this.render()
    }
  }

  checkUser (setColor) {
    if (this.mode !== modes.USER) return true
    if (!this.user) return false
    if (this.user === this.value) return true
    if (setColor) {
      this.wrong = true
      this.render()
    }
    return false
  }

  showSolution () {
    this.solution = true
    this.render()
  }

  restart () {
    this.user = undefined
    this.notes = []
    this.render()
  }

  copy () {
    const field = new Field(this.row, this.col)

    field.value = this.value
    field.mode = this.mode

    field.wrong = this.wrong
    field.solution = this.solution

    field.copyFrom(this)

    return field
  }

  copyFrom(field) {
    this.user = field.user
    this.notes = [...field.notes]
  }

  getElement () {
    return $(this.getSelector())
  }

  reset () {
    this.getElement().empty()
    this.getElement().css('background-color', colors.WHITE)
  }

  render () {
    this.getElement().empty()
    if (this.mode === modes.USER) {
      this.getElement().css(
        'background-color',
        this.isActive() ? colors.FIELDSELECTED : colors.FIELDUNSELECTED)

      if (this.solution) {
        if (this.user === this.value) {
          this.getElement().css('color', colors.SOLUTION)
        } else {
          this.getElement().css('color', colors.WRONG)
        }
        this.getElement().text(this.value)
      } else {
        if (this.user) {
          if (this.wrong) {
            this.getElement().css('color', colors.WRONG)
          } else {
            this.getElement().css('color', colors.USER)
          }
          this.getElement().text(this.user)
        } else if (this.notes.length > 0) {
          this.getElement().css('color', colors.USER)
          let notes = '<table class="mini" cellspacing="0">'
          for (let i = 1; i <= currentGridSize; i++) {
            if ((i - 1) % 3 === 0) notes += '<tr>'
            if (this.notes.indexOf(i) >= 0) {
              notes += `<td>${i}</td>`
            } else {
              notes += `<td class="transparent">${i}</td>`
            }
            if (i % 3 === 0) notes += '</tr>'
          }
          notes += '</table>'
          this.getElement().append(notes)
        }
      }
    } else if (this.mode === modes.BLACKKNOWN) {
      this.getElement().css('color', colors.WHITE)
      this.getElement().css('background-color', colors.BLACK)
      this.getElement().text(this.value)
    } else if (this.mode === modes.KNOWN) {
      this.getElement().css('background-color', colors.WHITE)
      this.getElement().css('color', colors.KNOWN)
      this.getElement().text(this.value)
    } else {
      this.getElement().css('background-color', colors.BLACK)
    }
  }
}

// class to store and modify the current game state
class Game {
  constructor () {
    this.data = []
    for (let r = 0; r < currentGridSize; r++) {
      this.data.push([])
      for (let c = 0; c < currentGridSize; c++) {
        this.data[r].push(new Field())
      }
    }
  }

  get (row, col) {
    return this.data[row][col]
  }

  setValues (row, col, mode, value) {
    const field = new Field(row, col)
    this.data[row][col] = field
    this.data[row][col].mode = mode
    this.data[row][col].value = value
    field.render()
  }

  forEach (iteratorFunction) {
    for (let r = 0; r < currentGridSize; r++) {
      for (let c = 0; c < currentGridSize; c++) {
        iteratorFunction(this.data[r][c], r, c)
      }
    }
  }
}

// Button Functions
function restart () {
  showDialog(false)
  game.forEach(field => {
    field.restart()
  })
}
function toggleNoteMode () {
  noteMode = !noteMode
  const color = (noteMode) ? colors.BUTTONDOWN : colors.BUTTONUP
  $('#notes').css('background-color', color)
}
function check () {
  count++
  $('#counter').text(count)
  game.forEach(field => {
    field.checkUser(setColor = true)
  })
}
function solution () {
  showDialog(false)
  showSolution = true
  clearInterval(timer)
  game.forEach(field => {
    field.showSolution()
  })
}
function undo () {
  if (undoStack.length > 0 && !showSolution) {
    const field = undoStack.pop()
    gameField = game.get(field.row, field.col)
    gameField.copyFrom(field)
    selectCell(field.row, field.col)
  }
}

// Parse game
function parseGameV128 (binary) {
  const size = parseInt(binary.substring(0, 5), 2)
  changeGridSize(size)
  const pos = 5

  const bitsPerNumber = Math.floor(Math.log2(size - 1)) + 1
  const bitsPerField = 2 + bitsPerNumber // black + known + number
  const game = new Game()

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
        : (isKnown ? modes.KNOWN : modes.USER)

      game.setValues(row, col, mode, value)
    }
  }

  return game
}

function parseGame (code) {
  game = new Game()
  const base64urlCharacters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_'
  let binary = ''
  for (let i = 0; i < code.length; i++) {
    b = base64urlCharacters.indexOf(code.charAt(i)).toString(2)
    while (b.length < 6) b = '0' + b
    binary += b
  }
  const encodingVersion = parseInt(binary.substring(0, 8), 2)
  binary = binary.substring(8)
  switch (encodingVersion) {
    case 1:
      changeGridSize(9)
      if (binary.length < (6 * 81)) return // Invalid data
      for (let i = 0; i < 81; i++) {
        const subBinary = binary.substring(i * 6, (i + 1) * 6)
        const mode = parseInt(subBinary.substring(0, 2), 2)
        const value = parseInt(subBinary.substring(2, 6), 2) + 1
        game.setValues(Math.floor(i / 9), i % 9, mode, value)
      }
      binary = binary.substring(6 * 81)
      let counter = 0
      while (binary.length >= 7 && counter < (4 - difficulty) * 3.5) {
        const position = parseInt(binary.substring(0, 7), 2)
        game.get(Math.floor(position / 9), position % 9).mode = modes.KNOWN
        game.get(Math.floor(position / 9), position % 9).render()
        binary = binary.substring(7)
        counter++
      }
      break
    case 128: // 0b10000000: arbitrary size game encoding
      game = parseGameV128(binary)
      break
    default:
      changeGridSize(9)
      if (binary.length < (6 * 81) || binary.length > (6 * 81 + 8)) return // Invalid data
      for (let i = 0; i < 81; i++) {
        const subBinary = binary.substring(i * 6, (i + 1) * 6)
        const mode = parseInt(subBinary.substring(0, 2), 2)
        const value = parseInt(subBinary.substring(2, 6), 2) + 1
        game.setValues(Math.floor(i / 9), i % 9, mode, value)
      }
  }
}

// General Functions
function changeGridSize (newGridSize) {
  if (newGridSize == currentGridSize) {
    return
  }

  currentGridSize = Math.min(newGridSize, MAX_GRID_SIZE)
  currentGridSize = Math.max(currentGridSize, MIN_GRID_SIZE)
  showHideButtonsAndCells()
}

function showHideButtonsAndCells () {
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

function setup () {
  for (let r = 0; r < MAX_GRID_SIZE; r++) {
    let row = `<tr class="row" id="r${r}" row="${r}">`
    for (let c = 0; c < MAX_GRID_SIZE; c++) {
      row += `<td class="cell" id="ce${r}_${c}" row="${r}" col="${c}"></td>`
    }
    row += '</tr>'
    $('.container').append(row)
  }
}

function restartTimer () {
  starttime = (new Date()).getTime()
  timer = setInterval(function () {
    const diff = (new Date()).getTime() - starttime
    const minutes = Math.floor(diff / 60000)
    const seconds = Math.round(diff / 1000 - minutes * 60)
    $('#time').text(((minutes < 10) ? '0' : '') + minutes + ':' + ((seconds < 10) ? '0' : '') + seconds)
  }, 1000)
}

function getURLParameter (name) {
  if (!window.location.search) return null
  const urlParams = new URLSearchParams(window.location.search)
  return urlParams.get(name)
}

generate = load_generate()

async function loadNewGame () {
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

function changeDifficulty () {
  difficulty = Number($('#difficulty-slider').val())
  $('#difficulty').text(difficulty)
}

function changeGenerateSize () {
  generateGridSize = Number($('#grid-size-slider').val())
  $('#grid-size').text(generateGridSize)
}

async function startGame () {
  if (gameCode && gameCode.length > minCodeSize) {
    showSolution = false
    undoStack = []
    activeCol = undefined
    activeRow = undefined
    count = 0
    $('#counter').text(count)
    $('.container').removeClass('finished')
    showDialog(false)
    if (game) {
      game.forEach(field => {
        field.reset()
      })
    }
    parseGame(gameCode)

    const savedGameState = loadGameStateFromLocalStorage(gameCode)
    if (savedGameState) {
      restoreGameState(savedGameState)
    }

    restartTimer()
  } else {
    await loadNewGame()
  }
}

function loadNewGameAgain () {
  showDialog(dialogs.WELCOME)
  $('#cancel-new-game').show()
}

function showDialog (dialog) {
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
        if (!showSolution) {
          $('#solution-dialog').show()
        } else {
          $('.dialog-outer-container').hide()
        }
        break
      case dialogs.EMPTY:
        if (!showSolution) {
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

function selectCell (row, col) {
  if (!showSolution && game.get(row, col).mode == modes.USER) {
    // Reset previously selected field
    if (typeof activeRow !== 'undefined') {
      const previousField =  game.get(activeRow, activeCol)
      activeRow = row
      activeCol = col
      previousField.render()
    }

    // Change background of just selected field
    activeRow = row
    activeCol = col
    var selectedField = game.get(activeRow, activeCol)
    selectedField.render()
  }
}

function findNextCell (row, col, rowDelta, colDelta) {
  let newRow = row
  let newCol = col
  do {
    newRow = (newRow + rowDelta + currentGridSize) % currentGridSize
    newCol = (newCol + colDelta + currentGridSize) % currentGridSize
  }
  while (
    game.get(newRow, newCol).mode != modes.USER &&
        (newRow !== row || newCol != col))

  return [newRow, newCol]
}

function onKeyDown (e) {
  if (showSolution) return

  let handled = false
  const key = e.key
  if (e.keyCode >= 37 && e.keyCode <= 40) {
    handleCursorKeys(e)
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

function handleCursorKeys (e) {
  if (typeof activeRow === 'undefined') {
    return
  }

  let newRow = activeRow
  let newCol = activeCol

  switch (e.which) {
    case 37: // left
      [newRow, newCol] = findNextCell(activeRow, activeCol, 0, -1)
      break
    case 38: // up
      [newRow, newCol] = findNextCell(activeRow, activeCol, -1, 0)
      break
    case 39: // right
      [newRow, newCol] = findNextCell(activeRow, activeCol, 0, 1)
      break
    case 40: // down
      [newRow, newCol] = findNextCell(activeRow, activeCol, 1, 0)
      break
    default: return
  }

  selectCell(newRow, newCol)
}

let firstDigit = null
let digitTimer = null
const twoDigitTimeout = 500
function handleNumberKey (num) {
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

function saveGameStateToLocalStorage (key) {
  const gameState = {
    timestamp: Date.now(),
    data: game.data.map(row =>
      row.map(field => ({
        value: field.value,
        user: field.user,
        notes: field.notes,
        mode: field.mode,
        wrong: field.wrong
      }))
    )
  }
  const gameStateString = JSON.stringify(gameState)
  localStorage.setItem(key, gameStateString)

  // Ensure only the latest MAX_NUMBER_OF_STORED_GAMES are kept
  const keys = Object.keys(localStorage)
  if (keys.length > MAX_NUMBER_OF_STORED_GAMES) {
    const oldestKey = keys.sort((a, b) => JSON.parse(localStorage.getItem(a)).timestamp - JSON.parse(localStorage.getItem(b)).timestamp)[0]
    localStorage.removeItem(oldestKey)
  }
}

function loadGameStateFromLocalStorage (key) {
  const gameStateString = localStorage.getItem(key)
  if (!gameStateString) return null
  try {
    return JSON.parse(gameStateString)
  } catch (e) {
    console.error('Error parsing game state from localStorage for key:', key, e)
    return null
  }
}

function getLatestGameKeyFromLocalStorage () {
  const keys = Object.keys(localStorage)
  let latestKey = null
  let latestTimestamp = 0

  keys.forEach(key => {
    const gameStateString = localStorage.getItem(key)
    if (gameStateString) {
      const gameState = JSON.parse(gameStateString)
      if (gameState.timestamp > latestTimestamp) {
        latestTimestamp = gameState.timestamp
        latestKey = key
      }
    }
  })

  return latestKey
}

function restoreGameState (gameState) {
  gameState.data.forEach((row, r) => {
    row.forEach((field, c) => {
      const gameField = game.get(r, c)
      gameField.copyFrom(field)
      gameField.render()
    })
  })
}

function handleNumberInput (num) {
  if (num < 1 || num > currentGridSize) {
    return
  }

  if (!showSolution && typeof activeRow !== 'undefined' &&
        game.get(activeRow, activeCol).mode == modes.USER) {
    undoStack.push(game.get(activeRow, activeCol).copy())
    if (noteMode) {
      game.get(activeRow, activeCol).setNote(num)
    } else {
      game.get(activeRow, activeCol).setUser(num)
      let finished = true
      game.forEach(field => {
        if (!field.checkUser(setColor = false)) finished = false
      })
      if (finished) {
        showSolution = true
        $('.container').addClass('finished')
        onResize()
        clearInterval(timer)
      }
    }
  }
  saveGameStateToLocalStorage(gameCode)
}

function handleDelete () {
  if (!showSolution && typeof activeRow !== 'undefined') {
    const field = game.get(activeRow, activeCol)
    if (field.mode != modes.USER) {
      return
    }

    undoStack.push(field.copy())
    field.clear()
  }
}

$(document).ready(function () {
  setup()
  onResize()
  handleGameLoad()
  $('td[id^="ce"]').click(function () { // Game fields
    const row = Number($(this).attr('row'))
    const col = Number($(this).attr('col'))
    selectCell(row, col)
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
    if (popstate)
    {
        startGame()
    }
    else
    {
        showDialog(dialogs.GENERATED)
    }
  } else {
    const latestKey = getLatestGameKeyFromLocalStorage()
    if (latestKey) {
      window.location.href = window.location.href.split('?')[0] + '?code=' + latestKey
      return
    }
    showDialog(dialogs.WELCOME)
  }
}

$(document).keydown(onKeyDown)

$(window).resize(onResize)
function onResize () {
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
