// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

const MAX_NUMBER_OF_STORED_GAMES = 50

export function saveGameState(key, game) {
    migrate()
    const gameState = {
        timestamp: Date.now(),
        data: game.dumpState(),
    }

    const gameStateString = JSON.stringify(gameState)
    localStorage.setItem('history.' + key, gameStateString)

    // Ensure only the latest MAX_NUMBER_OF_STORED_GAMES are kept
    const prefixedKeys = getPrefixedHistoryKeys()
    if (prefixedKeys.length > MAX_NUMBER_OF_STORED_GAMES) {
        console.info("Cropping game history")
        const keysByAge = []
        for (const pk of prefixedKeys) {
            const gameState = loadGameStateData(pk)
            if (!gameState) {
                console.debug(`Removing corrupt history entry ${pk}`)
                localStorage.removeItem(pk)
            }

            keysByAge.push({ key: pk, timestamp: gameState.timestamp })
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

export function restoreGameState(key, game) {
    migrate()
    const savedGameState = loadGameStateData('history.' + key)
    if (savedGameState) {
        game.restoreState(savedGameState.data)
    }
}

export function getLatestGameKey() {
    migrate()
    const prefixedKeys = getPrefixedHistoryKeys()
    let latestKey = null
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

function getPrefixedHistoryKeys() {
    return Object.keys(localStorage).filter(k => k.startsWith('history.'))
}

function loadGameStateData(prefixedKey) {
    if (!prefixedKey.startsWith('history.')) {
        return null
    }

    return loadGameStateDataCore(prefixedKey)
}

function loadGameStateDataCore(prefixedKey) {
    try {
        const gameStateString = localStorage.getItem(prefixedKey)
        if (!gameStateString) {
            return null
        }

        const result = JSON.parse(gameStateString)

        return result.timestamp && result.data ? result : null
    } catch (e) {
        console.warn('Error loading game state from localStorage for key:', prefixedKey, e)
        return null
    }
}

function migrate() {
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
            localStorage.setItem('history.' + key, gameStateString)
        }

        localStorage.removeItem(key)
    }

    localStorage.setItem('version', '1')
    console.info('Migrated game history data')
}
