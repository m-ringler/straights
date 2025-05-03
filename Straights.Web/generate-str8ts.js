// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

function load_generate () {
  const worker = new Worker('generate-worker.js')

  async function generate (size, difficulty) {
    return new Promise((resolve, reject) => {
      worker.onmessage = (event) => {
        resolve(event.data)
      }

      worker.onerror = (error) => {
        reject(error)
      }

      worker.postMessage({
        size,
        difficulty
      })
    })
  }

  return generate
}
