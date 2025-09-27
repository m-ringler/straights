// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export function load_generate() {
    const worker = new Worker('generate-worker.js')

    function generate(size, difficulty) {
        return _run_in_worker({
            method: 'generate',
            size,
            difficulty
        })
    }

    function generateHint(gameAsJson) {
        return _run_in_worker({
            method: 'hint',
            gameAsJson: JSON.stringify(gameAsJson)
        })
    }

    return { generate, generateHint }

    function _run_in_worker(message) {
        return new Promise((resolve, reject) => {
            worker.onmessage = (event) => {
                resolve(event.data)
            }

            worker.onerror = (error) => {
                reject(error)
            }

            worker.postMessage(message)
        })
    }
}
