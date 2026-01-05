// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export class NumberInput {
  private currentNumber: number = 0;
  private digitTimer: ReturnType<typeof setTimeout> | undefined = undefined;

  constructor(
    private handleNumberAsync: (num: number) => Promise<void>,
    public digitTimeout: number = 500
  ) {}

  async handleDigitAsync(digit: number, maxNumber: number) {
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
      await this.handleNumberAsync(previousNumber);
      this.currentNumber = digit;
    }

    // If appending another digit would exceed maxNumber, finalize currentNumber
    if (this.currentNumber * 10 > maxNumber) {
      await this.handleNumberAsync(this.currentNumber);
      this.currentNumber = 0;
      return;
    }

    // Set a new timeout to finalize the number if no more digits arrive
    this.digitTimer = setTimeout(async () => {
      await this.handleNumberAsync(this.currentNumber);
      this.currentNumber = 0;
    }, this.digitTimeout);
  }

  reset() {
    clearTimeout(this.digitTimer);
    this.currentNumber = 0;
  }
}
