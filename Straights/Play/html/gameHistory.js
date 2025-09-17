// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const MAX_NUMBER_OF_STORED_GAMES = 50

export function saveGameState(key, game) {
  const gameState = {
    timestamp: Date.now(),
    data: game.dumpState(),
  }
  const gameStateString = JSON.stringify(gameState)
  localStorage.setItem(key, gameStateString)

  // Ensure only the latest MAX_NUMBER_OF_STORED_GAMES are kept
  const keys = Object.keys(localStorage)
  if (keys.length > MAX_NUMBER_OF_STORED_GAMES) {
    const keysByAge = keys.sort((a, b) => JSON.parse(localStorage.getItem(a)).timestamp - JSON.parse(localStorage.getItem(b)).timestamp)
    for (let i = 0; i < keysByAge.length - MAX_NUMBER_OF_STORED_GAMES; i++) {
      localStorage.removeItem(keysByAge[i])
    }
  }
}

export function restoreGameState(key, game) {
  const savedGameState = loadGameStateData(gameCode)
  if (savedGameState) {
    game.restoreState(savedGameState.data)
  }
}

export function getLatestGameKey() {
  const keys = Object.keys(localStorage)
  let latestKey = null
  let latestTimestamp = 0

  keys.forEach(key => {
      const gameState = loadGameStateData(key)
      if (gameState && gameState.timestamp > latestTimestamp) {
        latestTimestamp = gameState.timestamp
        latestKey = key
      }
    }
  )

  return latestKey
}

function loadGameStateData(key) {
  try {
    const gameStateString = localStorage.getItem(key)
    if (!gameStateString) {
        return null
    }

    return JSON.parse(gameStateString)
  } catch (e) {
    console.warn('Error loading game state from localStorage for key:', key, e)
    return null
  }
}
