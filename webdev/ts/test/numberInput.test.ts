// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { NumberInput } from '../shared/numberInput';

describe('NumberInput', () => {
  let handleNumberAsync: (num: number) => Promise<void>;
  let input: NumberInput;

  beforeEach(() => {
    vi.useFakeTimers(); // Mock timers
    handleNumberAsync = vi.fn((_: number) => Promise.resolve()); // Mock callback
    input = new NumberInput(handleNumberAsync, 500);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  // Helper to advance timers
  const advanceTime = (ms: number) => {
    vi.advanceTimersByTime(ms);
  };

  it('should throw if digit is invalid', async () => {
    await expect(
      async () => await input.handleDigitAsync(10, 20)
    ).rejects.toThrow('Digit must be between 0 and 9');
  });

  it('should throw if single digit exceeds maxNumber', async () => {
    await expect(
      async () => await input.handleDigitAsync(5, 3)
    ).rejects.toThrow('Digit 5 exceeds maxNumber 3');
  });

  it('should output single digit if valid', async () => {
    await input.handleDigitAsync(3, 20);
    advanceTime(600);
    expect(handleNumberAsync).toHaveBeenCalledWith(3);
  });

  it('should output two-digit number if valid', async () => {
    await input.handleDigitAsync(1, 20);
    await input.handleDigitAsync(2, 20);
    advanceTime(500);
    expect(handleNumberAsync).toHaveBeenCalledWith(12);
  });

  it('should split and output if number exceeds maxNumber', async () => {
    await input.handleDigitAsync(1, 15);
    await input.handleDigitAsync(9, 15); // 19 > 15 → outputs 1, restarts with 9
    advanceTime(500);
    expect(handleNumberAsync).toHaveBeenCalledWith(1);
    // Now currentNumber is 9, waiting for timeout
    advanceTime(500);
    expect(handleNumberAsync).toHaveBeenCalledWith(9);
  });

  it('should output immediately if appending another digit would exceed maxNumber', async () => {
    await input.handleDigitAsync(9, 20); // 9*10 > 20 → outputs 9 immediately
    expect(handleNumberAsync).toHaveBeenCalledWith(9);
  });

  it('should reset state after timeout', async () => {
    await input.handleDigitAsync(1, 20);
    advanceTime(500);
    expect(handleNumberAsync).toHaveBeenCalledWith(1);
    expect(input['currentNumber']).toBe(0); // Check internal state
  });

  it('should clear timeout on new digit', async () => {
    await input.handleDigitAsync(1, 20);
    advanceTime(250); // Halfway through timeout
    await input.handleDigitAsync(2, 20); // Should clear the previous timeout
    advanceTime(500);
    expect(handleNumberAsync).toHaveBeenCalledWith(12);
  });

  it('should reset manually', async () => {
    await input.handleDigitAsync(1, 20);
    input.reset();
    advanceTime(1000);
    expect(handleNumberAsync).not.toHaveBeenCalled();
  });
});
