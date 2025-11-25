// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// TODO: declare an interface for the generate-str8ts modules
export function load_generate() {
    const worker = new Worker('generate-worker.js')

    function generate(size: number, difficulty: number): Promise<any> {
        return _run_in_worker({
            method: 'generate',
            size,
            difficulty
        })
    }

    function generateHint(gameAsJson: any): Promise<any> {
        return _run_in_worker({
            method: 'hint',
            gameAsJson: JSON.stringify(gameAsJson)
        })
    }

    return { generate, generateHint }

    function _run_in_worker(message:
         { method: "generate", size?: number, difficulty?: number } |
         { method: 'hint', gameAsJson: string }): Promise<any> {
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
