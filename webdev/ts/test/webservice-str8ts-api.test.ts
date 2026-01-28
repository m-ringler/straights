// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { generate, generateHint, ApiResult } from '../webservice/str8ts-api';

describe('str8ts-api', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('generate', () => {
    it('should fetch with correct parameters', async () => {
      const mockResponse: ApiResult = { status: 200, message: 'Success' };
      const fetchSpy = vi
        .spyOn(global, 'fetch')
        .mockResolvedValue(
          new Response(JSON.stringify(mockResponse), { status: 200 })
        );

      await generate(9, 5, 0);

      expect(fetchSpy).toHaveBeenCalledWith(
        '/generate?gridSize=9&difficulty=4&gridLayout=0'
      );
    });

    it('should return valid ApiResult', async () => {
      const mockResponse: ApiResult = {
        status: 200,
        message: 'Puzzle generated',
      };
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify(mockResponse), { status: 200 })
      );

      const result = await generate(9, 5, 0);

      expect(result).toEqual(mockResponse);
      expect(result.status).toBe(200);
      expect(result.message).toBe('Puzzle generated');
    });

    it('should adjust difficulty by subtracting 1', async () => {
      const mockResponse: ApiResult = { status: 200, message: 'Success' };
      const fetchSpy = vi
        .spyOn(global, 'fetch')
        .mockResolvedValue(
          new Response(JSON.stringify(mockResponse), { status: 200 })
        );

      await generate(9, 3, 1);

      expect(fetchSpy).toHaveBeenCalledWith(
        '/generate?gridSize=9&difficulty=2&gridLayout=1'
      );
    });

    it('should throw error when response is missing status', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify({ message: 'Error' }), { status: 200 })
      );

      await expect(generate(9, 5, 0)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should throw error when response is missing message', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify({ status: 200 }), { status: 200 })
      );

      await expect(generate(9, 5, 0)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should throw error when status is not a number', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify({ status: '200', message: 'Error' }), {
          status: 200,
        })
      );

      await expect(generate(9, 5, 0)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should throw error when message is not a string', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify({ status: 200, message: 123 }), {
          status: 200,
        })
      );

      await expect(generate(9, 5, 0)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should throw error when response is null', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify(null), { status: 200 })
      );

      await expect(generate(9, 5, 0)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });
  });

  describe('generateHint', () => {
    const mockGame: number[][][] = [
      [
        [1, 2],
        [3, 4],
      ],
    ];

    it('should fetch with correct method and headers', async () => {
      const mockResponse: ApiResult = { status: 200, message: 'Hint provided' };
      const fetchSpy = vi
        .spyOn(global, 'fetch')
        .mockResolvedValue(
          new Response(JSON.stringify(mockResponse), { status: 200 })
        );

      await generateHint(mockGame);

      expect(fetchSpy).toHaveBeenCalledWith('/hint', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(mockGame),
      });
    });

    it('should return valid ApiResult', async () => {
      const mockResponse: ApiResult = { status: 200, message: 'Hint provided' };
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify(mockResponse), { status: 200 })
      );

      const result = await generateHint(mockGame);

      expect(result).toEqual(mockResponse);
      expect(result.status).toBe(200);
      expect(result.message).toBe('Hint provided');
    });

    it('should throw error when response is invalid', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify({ status: 200 }), { status: 200 })
      );

      await expect(generateHint(mockGame)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should throw error when response is not an object', async () => {
      vi.spyOn(global, 'fetch').mockResolvedValue(
        new Response(JSON.stringify('invalid'), { status: 200 })
      );

      await expect(generateHint(mockGame)).rejects.toThrow(
        'Invalid API response: expected ApiResult with status and message properties'
      );
    });

    it('should serialize complex game structure correctly', async () => {
      const complexGame: number[][][] = [
        [
          [1, 2],
          [3, 4],
        ],
        [
          [5, 6],
          [7, 8],
        ],
      ];
      const mockResponse: ApiResult = { status: 200, message: 'Hint provided' };
      const fetchSpy = vi
        .spyOn(global, 'fetch')
        .mockResolvedValue(
          new Response(JSON.stringify(mockResponse), { status: 200 })
        );

      await generateHint(complexGame);

      expect(fetchSpy).toHaveBeenCalledWith('/hint', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(complexGame),
      });
    });
  });
});
