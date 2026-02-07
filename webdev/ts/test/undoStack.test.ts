// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, vi } from 'vitest';
import { UndoStack } from '../shared/undoStack';

describe('UndoStack', () => {
  it('should initialize with empty stack and call handler with length 0', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);

    expect(undoStack.length).toBe(0);
    expect(lengthChangedHandler).toHaveBeenCalledWith(0);
    expect(lengthChangedHandler).toHaveBeenCalledTimes(1);
  });

  it('should push items and call handler with updated length', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);
    lengthChangedHandler.mockClear();

    undoStack.push('item1');
    expect(undoStack.length).toBe(1);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(1);

    undoStack.push('item2');
    expect(undoStack.length).toBe(2);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(2);

    undoStack.push('item3');
    expect(undoStack.length).toBe(3);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(3);

    expect(lengthChangedHandler).toHaveBeenCalledTimes(3);
  });

  it('should pop items in LIFO order and call handler with updated length', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);
    undoStack.push('item1');
    undoStack.push('item2');
    undoStack.push('item3');
    lengthChangedHandler.mockClear();

    const popped1 = undoStack.pop();
    expect(popped1).toBe('item3');
    expect(undoStack.length).toBe(2);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(2);

    const popped2 = undoStack.pop();
    expect(popped2).toBe('item2');
    expect(undoStack.length).toBe(1);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(1);

    const popped3 = undoStack.pop();
    expect(popped3).toBe('item1');
    expect(undoStack.length).toBe(0);
    expect(lengthChangedHandler).toHaveBeenLastCalledWith(0);

    expect(lengthChangedHandler).toHaveBeenCalledTimes(3);
  });

  it('should return undefined when popping from empty stack', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);

    const popped = undoStack.pop();
    expect(popped).toBeUndefined();
    expect(undoStack.length).toBe(0);
  });

  it('should clear the stack and call handler', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);
    undoStack.push('item1');
    undoStack.push('item2');
    undoStack.push('item3');
    lengthChangedHandler.mockClear();

    undoStack.clear();

    expect(undoStack.length).toBe(0);
    expect(lengthChangedHandler).toHaveBeenCalledWith(0);
    expect(lengthChangedHandler).toHaveBeenCalledTimes(1);
  });

  it('should work with different data types', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack<number | string | object>(
      lengthChangedHandler
    );

    undoStack.push(42);
    undoStack.push('hello');
    undoStack.push({ id: 1, name: 'test' });

    expect(undoStack.pop()).toEqual({ id: 1, name: 'test' });
    expect(undoStack.pop()).toBe('hello');
    expect(undoStack.pop()).toBe(42);
  });

  it('should handle multiple push and pop cycles', () => {
    const lengthChangedHandler = vi.fn();
    const undoStack = new UndoStack(lengthChangedHandler);

    // First cycle
    undoStack.push('a');
    undoStack.push('b');
    expect(undoStack.pop()).toBe('b');

    // Second cycle
    undoStack.push('c');
    undoStack.push('d');
    expect(undoStack.pop()).toBe('d');
    expect(undoStack.pop()).toBe('c');

    // Third cycle
    undoStack.push('e');
    expect(undoStack.pop()).toBe('e');
    expect(undoStack.pop()).toBe('a');
  });
});
