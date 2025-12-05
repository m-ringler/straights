// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export class UndoStack<ItemType> {
  stack: ItemType[];
  constructor(handleLengthChanged: (length: number) => void) {
    this.stack = [];
    this.#onLengthChanged = handleLengthChanged;
    this.#raiseLengthChanged();
  }

  #onLengthChanged: (length: number) => void;

  #raiseLengthChanged() {
    this.#onLengthChanged(this.length);
  }

  push(item: ItemType) {
    this.stack.push(item);
    this.#raiseLengthChanged();
  }

  pop(): ItemType | undefined {
    const item = this.stack.pop();
    this.#raiseLengthChanged();
    return item;
  }

  clear(): void {
    this.stack = [];
    this.#raiseLengthChanged();
  }

  get length(): number {
    return this.stack.length;
  }
}
