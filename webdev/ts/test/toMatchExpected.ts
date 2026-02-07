// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { expect } from 'vitest';

type AnyObject = Record<string, any>;

declare module 'vitest' {
  interface Assertion<T = any> {
    toMatchExpected(expected: AnyObject): T;
  }
}

function filterObject(obj: AnyObject, keys: Set<string>): AnyObject {
  return Object.fromEntries(
    Object.entries(obj).filter(([key]) => keys.has(key))
  );
}

function deepFilter(actual: AnyObject, expected: AnyObject): AnyObject {
  const expectedKeys = new Set(Object.keys(expected));
  const filtered = filterObject(actual, expectedKeys);

  for (const key of expectedKeys) {
    if (
      typeof actual[key] === 'object' &&
      actual[key] !== null &&
      typeof expected[key] === 'object' &&
      expected[key] !== null
    ) {
      filtered[key] = deepFilter(actual[key], expected[key]);
    }
  }

  return filtered;
}

expect.extend({
  toMatchExpected(received: AnyObject, expected: AnyObject) {
    const filteredReceived = deepFilter(received, expected);
    const pass = this.equals(filteredReceived, expected);

    return {
      pass,
      message: () =>
        pass
          ? `Expected object to not match the filtered structure of the received object.`
          : `Expected: ${this.utils.printExpected(expected)}\n` +
            `Received (filtered): ${this.utils.printReceived(filteredReceived)}`,
    };
  },
});
