// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import type { Game } from "./game"
import type { DumpedState } from "./game"

const MAX_NUMBER_OF_STORED_GAMES = 50 

type GameState = {
    timestamp: number,
    data: DumpedState,
}

export function saveGameState(key: string, game: Game) {
    migrate()
    const gameState: GameState = {
        timestamp: Date.now(),
        data: game.dumpState(),
    }

    const gameStateString = JSON.stringify(gameState)
    localStorage.setItem('history.' + key, gameStateString)

    // Ensure only the latest MAX_NUMBER_OF_STORED_GAMES are kept
    const prefixedKeys = getPrefixedHistoryKeys()
    if (prefixedKeys.length > MAX_NUMBER_OF_STORED_GAMES) {
        console.info("Cropping game history")
        const keysByAge: { key: string, timestamp: number }[] = []
        for (const pk of prefixedKeys) {
            const gameState = loadGameStateData(pk)
            if (!gameState) {
                console.debug(`Removing corrupt history entry ${pk}`)
                localStorage.removeItem(pk)
            } else {
                keysByAge.push({ key: pk, timestamp: gameState.timestamp })
            }
        }

        keysByAge.sort(
            (a, b) => a.timestamp - b.timestamp)
        for (let i = 0; i < keysByAge.length - MAX_NUMBER_OF_STORED_GAMES; i++) {
            const oldKey = keysByAge[i].key
            console.debug(`Removing old history entry ${oldKey}`)
            localStorage.removeItem(oldKey)
        }
    }
}

export function restoreGameState(key: string, game: Game): void {
    migrate()
    const savedGameState = loadGameStateData('history.' + key)
    if (savedGameState) {
        game.restoreState(savedGameState.data)
    }
}

export function getLatestGameKey(): string | null {
    migrate()
    const prefixedKeys = getPrefixedHistoryKeys()
    let latestKey: string | null = null
    let latestTimestamp = 0

    prefixedKeys.forEach(prefixedKey => {
        const gameState = loadGameStateData(prefixedKey)
        if (gameState && gameState.timestamp > latestTimestamp) {
            latestTimestamp = gameState.timestamp
            latestKey = prefixedKey.substring('history.'.length)
        }
    })

    return latestKey
}

function getPrefixedHistoryKeys(): string[] {
    return Object.keys(localStorage).filter(k => k.startsWith('history.'))
}

function loadGameStateData(prefixedKey: string): GameState | null {
    if (!prefixedKey.startsWith('history.')) {
        return null
    }

    return loadGameStateDataCore(prefixedKey)
}

function loadGameStateDataCore(prefixedKey: string): GameState | null {
    try {
        const gameStateString = localStorage.getItem(prefixedKey)
        if (!gameStateString) {
            return null
        }

        const result = JSON.parse(gameStateString) as GameState

        return result.timestamp && result.data ? result : null
    } catch (e) {
        console.warn('Error loading game state from localStorage for key:', prefixedKey, e)
        return null
    }
}

function migrate(): void {
    if (localStorage.getItem('version')) {
        return
    }

    const keys = Object.keys(localStorage)
    for (const key of keys) {
        if (key.startsWith('history.')) {
            continue
        }

        if (key.startsWith('generate.')) {
            continue
        }

        const item = loadGameStateDataCore(key)
        if (!item) {
            console.debug(`Removing unknown local storage key ${key}`)
        }
        else {
            console.debug(`Migrating game history entry ${key}`)

            const gameStateString = localStorage.getItem(key)
            if (gameStateString) {
                localStorage.setItem('history.' + key, gameStateString)
            }
        }

        localStorage.removeItem(key)
    }

    localStorage.setItem('version', '1')
    console.info('Migrated game history data')
}
