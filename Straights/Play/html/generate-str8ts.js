// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// This provides the generate function
// from an API endpoint running on the server.
export function load_generate() {
  async function generate(size, difficulty) {
    const response = await fetch(`/generate?gridSize=${size}&difficulty=${difficulty - 1}`)
    const data = await response.json()
    return data
  }

  async function generateHint(gameAsJson) {
    const response = await fetch('/hint', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(gameAsJson)
    })
    const data = await response.json()
    return data
  }

  return { generate, generateHint }
}
