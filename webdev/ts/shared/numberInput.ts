// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export class NumberInput {
  private handleNumber: (num: number) => void;
  private currentNumber: number = 0;
  private digitTimer: ReturnType<typeof setTimeout> | undefined = undefined;
  private readonly digitTimeout: number;

  constructor(handleNumber: (num: number) => void, digitTimeout: number = 500) {
    this.handleNumber = handleNumber;
    this.digitTimeout = digitTimeout;
  }

  handleDigit(digit: number, maxNumber: number) {
    if (digit < 0 || digit > 9) {
      throw new Error('Digit must be between 0 and 9');
    }

    // If the digit itself exceeds maxNumber, reject it
    if (digit > maxNumber) {
      throw new Error(`Digit ${digit} exceeds maxNumber ${maxNumber}`);
    }

    // Clear any pending timeout
    if (this.digitTimer) {
      clearTimeout(this.digitTimer);
    }

    // Update the current number
    const previousNumber = this.currentNumber;
    this.currentNumber = this.currentNumber * 10 + digit;

    if (this.currentNumber === 0) {
      return; // Ignore leading zeros
    }

    // If the number exceeds maxNumber, finalize the previous number and restart with the last digit
    if (this.currentNumber > maxNumber) {
      this.handleNumber(previousNumber);
      this.currentNumber = digit;
    }

    // If appending another digit would exceed maxNumber, finalize currentNumber
    if (this.currentNumber * 10 > maxNumber) {
      this.handleNumber(this.currentNumber);
      this.currentNumber = 0;
      return;
    }

    // Set a new timeout to finalize the number if no more digits arrive
    this.digitTimer = setTimeout(() => {
      this.handleNumber(this.currentNumber);
      this.currentNumber = 0;
    }, this.digitTimeout);
  }

  reset() {
    clearTimeout(this.digitTimer);
    this.currentNumber = 0;
  }
}
