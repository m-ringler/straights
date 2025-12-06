import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { NumberInput } from '../numberInput';

describe('NumberInput', () => {
  let handleNumber: (num: number) => void;
  let input: NumberInput;

  beforeEach(() => {
    vi.useFakeTimers(); // Mock timers
    handleNumber = vi.fn(); // Mock callback
    input = new NumberInput(handleNumber, 500);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  // Helper to advance timers
  const advanceTime = (ms: number) => {
    vi.advanceTimersByTime(ms);
  };

  it('should throw if digit is invalid', () => {
    expect(() => input.handleDigit(10, 20)).toThrow(
      'Digit must be between 0 and 9'
    );
  });

  it('should throw if single digit exceeds maxNumber', () => {
    expect(() => input.handleDigit(5, 3)).toThrow(
      'Digit 5 exceeds maxNumber 3'
    );
  });

  it('should output single digit if valid', () => {
    input.handleDigit(3, 20);
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(3);
  });

  it('should output two-digit number if valid', () => {
    input.handleDigit(1, 20);
    input.handleDigit(2, 20);
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(12);
  });

  it('should split and output if number exceeds maxNumber', () => {
    input.handleDigit(1, 15);
    input.handleDigit(9, 15); // 19 > 15 → outputs 1, restarts with 9
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(1);
    // Now currentNumber is 9, waiting for timeout
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(9);
  });

  it('should output immediately if appending another digit would exceed maxNumber', () => {
    input.handleDigit(9, 20); // 9*10 > 20 → outputs 9 immediately
    expect(handleNumber).toHaveBeenCalledWith(9);
  });

  it('should reset state after timeout', () => {
    input.handleDigit(1, 20);
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(1);
    expect(input['currentNumber']).toBe(0); // Check internal state
  });

  it('should clear timeout on new digit', () => {
    input.handleDigit(1, 20);
    advanceTime(250); // Halfway through timeout
    input.handleDigit(2, 20); // Should clear the previous timeout
    advanceTime(500);
    expect(handleNumber).toHaveBeenCalledWith(12);
  });

  it('should reset manually', () => {
    input.handleDigit(1, 20);
    input.reset();
    advanceTime(1000);
    expect(handleNumber).not.toHaveBeenCalled();
  });
});
