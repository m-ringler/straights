// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export class NumberInput {
  private handleNumber: (num: number) => void;

  constructor(
    handleNumber: (num: number) => void
  ) {
    this.handleNumber = handleNumber;
  }

  private firstDigit: number | null = null;
  private digitTimer: ReturnType<typeof setTimeout> | undefined = undefined;
  private twoDigitTimeout = 500;

  handleDigit(digit: number, maxNumber: number) {
    if (this.firstDigit == null) {
      if (maxNumber < 10 || digit !== 1) {
        this.handleNumber(digit);
      } else {
        this.firstDigit = digit;
        this.digitTimer = setTimeout(() => {
          this.handleNumber(digit);
          this.firstDigit = null;
        }, this.twoDigitTimeout);
      }
    } else {
      clearTimeout(this.digitTimer);
      const firstNum = this.firstDigit;
      const combinedNum = this.firstDigit * 10 + digit;
      this.firstDigit = null;
      if (combinedNum <= maxNumber) {
        this.handleNumber(combinedNum);
      } else {
        this.handleNumber(firstNum);
        this.handleDigit(digit, maxNumber);
      }
    }
  }
}