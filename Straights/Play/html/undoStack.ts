// SPDX-FileCopyrightText: 2020 Luis Walter, 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export class UndoStack {
  constructor(handleLengthChanged) {
    this.stack = []
    this.#onLengthChanged = handleLengthChanged
    this.#raiseLengthChanged()
  }

  #onLengthChanged

  #raiseLengthChanged() {
    this.#onLengthChanged(this.length)
  }

  push(item) {
    this.stack.push(item)
    this.#raiseLengthChanged()
  }

  pop() {
    const item = this.stack.pop()
    this.#raiseLengthChanged()
    return item
  }

  clear() {
    this.stack = []
    this.#raiseLengthChanged()
  }

  get length() {
    return this.stack.length
  }
}
