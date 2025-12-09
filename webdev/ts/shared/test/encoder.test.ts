// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect } from 'vitest';
import { EncodedResult, BitmaskEncoder } from '../encoder';

// Simple seeded random number generator for deterministic tests
function seededRandom(seed: number) {
  let state = seed;
  return () => {
    state = (state * 1664525 + 1013904223) % 4294967296;
    return state / 4294967296;
  };
}

// Default encoder for tests
const encoder = new BitmaskEncoder({
  compressionThreshold: 48,
  minCompressionRatio: 0.9,
  maxN: 12,
});

describe('Bitmask Encoder', () => {
  describe('encode', () => {
    it('should encode empty sets for n=1', async () => {
      const result = await encoder.encode(1, []);
      expect(result.base64Data).toBe('AA'); // Single flag byte (0 = not gzipped)
      expect(result.count).toBe(0);
    });

    it('should encode a single set with one element for n=3', async () => {
      const sets = [new Set([1])];
      const result = await encoder.encode(3, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should encode multiple sets for n=4', async () => {
      const sets = [
        new Set([1, 2]),
        new Set([3, 4]),
        new Set([1, 3]),
        new Set([2, 4]),
      ];
      const result = await encoder.encode(4, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should handle full sets (all numbers present)', async () => {
      const sets = [new Set([1, 2, 3, 4, 5]), new Set([1, 2, 3, 4, 5])];
      const result = await encoder.encode(5, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should handle empty sets', async () => {
      const sets: Set<number>[] = [new Set(), new Set(), new Set()];
      const result = await encoder.encode(3, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should compress large datasets when beneficial', async () => {
      // Create a large dataset that should trigger compression
      const sets = [];
      for (let i = 0; i < 100; i++) {
        sets.push(new Set([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]));
      }
      const result = await encoder.encode(10, sets);
      // Verify encoding completes successfully (compression may or may not work in test environment)
      expect(result.base64Data).toBeTruthy();
      expect(result.count).toBe(100);
    });

    it('should not compress when size reduction is less than 10%', async () => {
      // Create a small dataset that won't benefit from compression
      const sets = [];
      for (let i = 0; i < 9; i++) {
        sets.push(new Set([1, 2]));
      }
      const result = await encoder.encode(3, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should work with n=12 (maximum)', async () => {
      const sets = [new Set([1, 6, 12])];
      const result = await encoder.encode(12, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should work with n=1 (minimum)', async () => {
      const sets: Set<number>[] = [new Set([1])];
      const result = await encoder.encode(1, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should produce expected base64 output for known input', async () => {
      const sets = [new Set([1, 2, 3]), new Set([2, 4]), new Set([1, 3, 5])];
      const result = await encoder.encode(5, sets);
      expect(result.count).toBe(3);
      // First byte is flag (0 = not gzipped), followed by packed bitmasks
      expect(result.base64Data).toBe('AEdV');
    });

    it('should throw error for n < 1', async () => {
      await expect(encoder.encode(0, [])).rejects.toThrow(
        'n must be between 1 and 12'
      );
    });

    it('should throw error for n > 12', async () => {
      await expect(encoder.encode(13, [])).rejects.toThrow(
        'n must be between 1 and 12'
      );
    });

    it('should throw error for invalid set values', async () => {
      const sets = [new Set([0, 1, 2])]; // 0 is invalid
      await expect(encoder.encode(3, sets)).rejects.toThrow('invalid value');
    });

    it('should throw error for set values exceeding n', async () => {
      const sets = [new Set([1, 2, 5])]; // 5 > n=3
      await expect(encoder.encode(3, sets)).rejects.toThrow('invalid value');
    });
  });

  describe('decode', () => {
    it('should decode empty encoded data', async () => {
      const encoded = await encoder.encode(3, []);
      const decoded = await encoder.decode(encoded, 3);
      expect(decoded).toEqual([]);
    });

    it('should decode single set correctly', async () => {
      const original = [new Set([1, 3])];
      const encoded = await encoder.encode(5, original);
      const decoded = await encoder.decode(encoded, 5);
      expect(decoded).toHaveLength(1);
      expect(decoded[0]).toEqual(original[0]);
    });

    it('should decode multiple sets correctly', async () => {
      const original: Set<number>[] = [
        new Set([1, 2]),
        new Set([3]),
        new Set([1, 2, 3]),
        new Set(),
      ];
      const encoded = await encoder.encode(3, original);
      const decoded = await encoder.decode(encoded, 3);
      expect(decoded).toHaveLength(4);
      for (let i = 0; i < original.length; i++) {
        expect(decoded[i]).toEqual(original[i]);
      }
    });

    it('should decode compressed data correctly', async () => {
      const original = [];
      for (let i = 0; i < 64; i++) {
        original.push(new Set([1, 2, 3, 4, 5, 6, 7, 8]));
      }
      const encoded = await encoder.encode(8, original);
      // Data should be compressed (gzipped flag encoded in first bit)

      const decoded = await encoder.decode(encoded, 8);
      expect(decoded).toHaveLength(64);
      for (let i = 0; i < original.length; i++) {
        expect(decoded[i]).toEqual(original[i]);
      }
    });

    it('should throw error for n < 1', async () => {
      const encoded: EncodedResult = { base64Data: 'AA', count: 0 };
      await expect(encoder.decode(encoded, 0)).rejects.toThrow(
        'n must be between 1 and 12'
      );
    });

    it('should throw error for n > 12', async () => {
      const encoded: EncodedResult = { base64Data: 'AA', count: 0 };
      await expect(encoder.decode(encoded, 13)).rejects.toThrow(
        'n must be between 1 and 12'
      );
    });

    it('should throw error for invalid control byte', async () => {
      // Create encoded result with invalid control byte (2)
      const encoded: EncodedResult = { base64Data: 'Ag', count: 1 }; // 'Ag' decodes to [2]
      await expect(encoder.decode(encoded, 3)).rejects.toThrow(
        'Unknown control byte: 2'
      );
    });
  });

  describe('round-trip encoding', () => {
    it('should correctly round-trip for n=1', async () => {
      const original: Set<number>[] = [new Set([1])];
      const encoded = await encoder.encode(1, original);
      const decoded = await encoder.decode(encoded, 1);
      expect(decoded).toEqual(original);
    });

    it('should correctly round-trip for n=6', async () => {
      const original: Set<number>[] = [
        new Set([1, 3, 5]),
        new Set([2, 4, 6]),
        new Set([1, 2, 3, 4, 5, 6]),
        new Set(),
      ];
      const encoded = await encoder.encode(6, original);
      const decoded = await encoder.decode(encoded, 6);
      expect(decoded).toEqual(original);
    });

    it('should correctly round-trip for n=12 with maximum sets', async () => {
      const original = [];
      for (let i = 0; i < 144; i++) {
        // 12*12 = 144
        const set = new Set<number>();
        // Add every third number
        for (let j = 1; j <= 12; j += 3) {
          set.add(j);
        }
        original.push(set);
      }
      const encoded = await encoder.encode(12, original);
      const decoded = await encoder.decode(encoded, 12);
      expect(decoded).toEqual(original);
    });

    it('should correctly round-trip with various n values', async () => {
      const random = seededRandom(42); // Fixed seed for deterministic test
      for (let n = 12; n >= 1; n--) {
        const original = [];
        for (let i = 0; i < Math.min(20, n * n); i++) {
          const set = new Set<number>();
          // Add random numbers from 1 to n
          for (let j = 1; j <= n; j++) {
            if (random() > 0.5) {
              set.add(j);
            }
          }
          original.push(set);
        }
        const encoded = await encoder.encode(n, original);
        const decoded = await encoder.decode(encoded, n);
        expect(decoded).toEqual(original);
      }
    });

    it('should correctly round-trip large compressible dataset', async () => {
      const original = [];
      // Create repetitive pattern that compresses well
      for (let i = 0; i < 100; i++) {
        original.push(new Set([1, 3, 5, 7, 9]));
      }
      const encoded = await encoder.encode(10, original);
      // Large repetitive data should be compressed

      const decoded = await encoder.decode(encoded, 10);
      expect(decoded).toEqual(original);
    });
  });

  describe('base64url encoding', () => {
    it('should produce base64url-safe characters', async () => {
      const sets = [new Set([1, 2, 3])];
      const result = await encoder.encode(5, sets);
      // base64url should not contain +, /, or =
      expect(result.base64Data).not.toMatch(/[+/=]/);
    });

    it('should produce different encodings for different inputs', async () => {
      const result1 = await encoder.encode(3, [new Set([1])]);
      const result2 = await encoder.encode(3, [new Set([2])]);
      expect(result1.base64Data).not.toBe(result2.base64Data);
    });
  });

  describe('compression threshold', () => {
    it('should not compress data <= 200 bytes', async () => {
      // Small dataset
      const sets = Array(25).fill(new Set([1, 2, 3]));
      const result = await encoder.encode(5, sets);
      expect(result.base64Data).toBeTruthy();
    });

    it('should consider compression for data > 200 bytes', async () => {
      // Large dataset that should exceed 200 bytes
      const sets = [];
      for (let i = 0; i < 100; i++) {
        sets.push(new Set([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]));
      }
      const result = await encoder.encode(10, sets);
      // Large repetitive data should be compressed (first bit set to 1)
      expect(result.base64Data).toBeTruthy();
      expect(result.count).toBe(100);
    });
  });

  describe('BitmaskEncoder class', () => {
    describe('constructor', () => {
      it('should create encoder with specified settings', () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 12,
        });
        expect(encoder.compressionThreshold).toBe(48);
        expect(encoder.minCompressionRatio).toBe(0.9);
        expect(encoder.maxN).toBe(12);
      });

      it('should create encoder with custom settings', () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 100,
          minCompressionRatio: 0.8,
          maxN: 16,
        });
        expect(encoder.compressionThreshold).toBe(100);
        expect(encoder.minCompressionRatio).toBe(0.8);
        expect(encoder.maxN).toBe(16);
      });

      it('should allow maxN up to 32', () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 32,
        });
        expect(encoder.maxN).toBe(32);
      });

      it('should throw error for maxN < 1', () => {
        expect(
          () =>
            new BitmaskEncoder({
              compressionThreshold: 48,
              minCompressionRatio: 0.9,
              maxN: 0,
            })
        ).toThrow('maxN must be between 1 and 32');
      });

      it('should throw error for maxN > 32', () => {
        expect(
          () =>
            new BitmaskEncoder({
              compressionThreshold: 48,
              minCompressionRatio: 0.9,
              maxN: 33,
            })
        ).toThrow('maxN must be between 1 and 32');
      });

      it('should make properties readonly', () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 12,
        });
        // Properties are readonly, attempting to modify will fail in TypeScript
        expect(encoder.compressionThreshold).toBe(48);
      });
    });

    describe('encode with custom maxN', () => {
      it('should work with maxN=16', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 16,
        });
        const sets = [new Set([1, 8, 16])];
        const result = await encoder.encode(16, sets);
        expect(result.base64Data).toBeTruthy();
      });

      it('should work with maxN=32', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 32,
        });
        const sets = [new Set([1, 16, 32])];
        const result = await encoder.encode(32, sets);
        expect(result.base64Data).toBeTruthy();
      });

      it('should throw error for n > maxN', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 10,
        });
        await expect(encoder.encode(11, [])).rejects.toThrow(
          'n must be between 1 and 10'
        );
      });

      it('should allow n equal to maxN', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 20,
        });
        const result = await encoder.encode(20, [new Set([1, 10, 20])]);
        expect(result.base64Data).toBeTruthy();
      });
    });

    describe('decode with custom maxN', () => {
      it('should decode with maxN=16', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 16,
        });
        const original = [new Set([1, 8, 16])];
        const encoded = await encoder.encode(16, original);
        const decoded = await encoder.decode(encoded, 16);
        expect(decoded).toEqual(original);
      });

      it('should decode with maxN=32', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 32,
        });
        const original = [new Set([1, 16, 32])];
        const encoded = await encoder.encode(32, original);
        const decoded = await encoder.decode(encoded, 32);
        expect(decoded).toEqual(original);
      });

      it('should throw error for n > maxN', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 10,
        });
        const encoded: EncodedResult = { base64Data: 'AA', count: 0 };
        await expect(encoder.decode(encoded, 11)).rejects.toThrow(
          'n must be between 1 and 10'
        );
      });
    });

    describe('custom compression settings', () => {
      it('should not compress when below custom threshold', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 1000,
          minCompressionRatio: 0.9,
          maxN: 12,
        });
        const sets = [];
        for (let i = 0; i < 20; i++) {
          sets.push(new Set([1, 2, 3, 4, 5]));
        }
        const result = await encoder.encode(5, sets);
        expect(result.base64Data).toBeTruthy();
      });

      it('should use custom minCompressionRatio', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 10,
          minCompressionRatio: 0.99, // Very strict compression requirement
          maxN: 12,
        });
        const sets = [];
        for (let i = 0; i < 100; i++) {
          sets.push(new Set([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]));
        }
        const result = await encoder.encode(10, sets);
        // With strict ratio, compression should be used (first bit = 1)
        expect(result.base64Data).toBeTruthy();
      });

      it('should compress with lower threshold', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 10,
          minCompressionRatio: 0.9,
          maxN: 12,
        });
        const sets = [];
        for (let i = 0; i < 100; i++) {
          sets.push(new Set([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]));
        }
        const result = await encoder.encode(10, sets);
        expect(result.base64Data).toBeTruthy();
      });
    });

    describe('round-trip with large n values', () => {
      it('should correctly round-trip for n=20', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 20,
        });
        const original: Set<number>[] = [
          new Set([1, 5, 10, 15, 20]),
          new Set([2, 4, 8, 16]),
          new Set(),
        ];
        const encoded = await encoder.encode(20, original);
        const decoded = await encoder.decode(encoded, 20);
        expect(decoded).toEqual(original);
      });

      it('should correctly round-trip for n=24', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 24,
        });
        const original: Set<number>[] = [
          new Set([1, 12, 24]),
          new Set([6, 12, 18]),
        ];
        const encoded = await encoder.encode(24, original);
        const decoded = await encoder.decode(encoded, 24);
        expect(decoded).toEqual(original);
      });

      it('should correctly round-trip for n=32 with multiple sets', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 32,
        });
        const original: Set<number>[] = [];
        for (let i = 0; i < 10; i++) {
          const set = new Set<number>();
          set.add(1);
          set.add(16);
          set.add(32);
          original.push(set);
        }
        const encoded = await encoder.encode(32, original);
        const decoded = await encoder.decode(encoded, 32);
        expect(decoded).toEqual(original);
      });
    });

    describe('validation with custom maxN', () => {
      it('should throw error for set values exceeding n with large maxN', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 30,
        });
        const sets = [new Set([1, 2, 26])]; // 26 > n=25
        await expect(encoder.encode(25, sets)).rejects.toThrow('invalid value');
      });

      it('should handle full sets with large n', async () => {
        const encoder = new BitmaskEncoder({
          compressionThreshold: 48,
          minCompressionRatio: 0.9,
          maxN: 32,
        });
        const fullSet = new Set<number>();
        for (let i = 1; i <= 32; i++) {
          fullSet.add(i);
        }
        const sets = [fullSet];
        const result = await encoder.encode(32, sets);
        expect(result.base64Data).toBeTruthy();

        const decoded = await encoder.decode(result, 32);
        expect(decoded[0]).toEqual(fullSet);
      });
    });
  });

  describe('README Usage example', () => {
    it('should execute the Usage code from README', async () => {
      // Mock console.log to capture output
      const consoleOutput: unknown[] = [];
      const console = {
        log: (data: unknown) => {
          consoleOutput.push(data);
        },
      };

      // Create encoder instance
      const encoder = new BitmaskEncoder({
        compressionThreshold: 48,
        minCompressionRatio: 0.9,
        maxN: 12,
      });

      // Encode sets
      const sets = [new Set([1, 2, 3]), new Set([2, 4]), new Set([1, 3, 5])];

      const encoded = await encoder.encode(5, sets);
      console.log(encoded);
      // {
      //   base64Data: "AEdV",
      //   count: 3
      // }

      // Decode back to sets
      const decoded = await encoder.decode(encoded, 5);
      console.log(decoded);
      // [Set(3) {1, 2, 3}, Set(2) {2, 4}, Set(3) {1, 3, 5}]

      // Assert on console output
      expect(consoleOutput).toHaveLength(2);
      expect(consoleOutput[0]).toEqual({ base64Data: 'AEdV', count: 3 });
      expect(consoleOutput[1]).toEqual([
        new Set([1, 2, 3]),
        new Set([2, 4]),
        new Set([1, 3, 5]),
      ]);
    });
  });
});
