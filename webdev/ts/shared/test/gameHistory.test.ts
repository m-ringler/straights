// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as Sut from '../gameHistory';

// --- Mock Storage Provider ---
class MockStorage implements Sut.StorageProvider {
  private storage: Map<string, string> = new Map();

  getItem(key: string): string | null {
    return this.storage.get(key) ?? null;
  }

  setItem(key: string, value: string): void {
    this.storage.set(key, value);
  }

  removeItem(key: string): void {
    this.storage.delete(key);
  }

  key(index: number): string | null {
    return Array.from(this.storage.keys())[index] ?? null;
  }

  sortedKeys(): string[] {
    return Array.from(this.storage.keys()).sort();
  }

  toJson(): string {
    return JSON.stringify(Object.fromEntries(this.storage));
  }

  get length(): number {
    return this.storage.size;
  }
}

// --- Test Game ---
interface TestGameState {
  level: number;
  score: number;
}

class TestGame implements Sut.GameLike<TestGameState> {
  private state: TestGameState;
  created: number;

  constructor(initialState: TestGameState) {
    this.state = initialState;
    this.created = Date.now();
  }

  dumpState(): TestGameState {
    return { ...this.state };
  }

  async restoreStateAsync(state: TestGameState): Promise<void> {
    this.state = { ...state };
  }

  getState(): TestGameState {
    return this.state;
  }
}

// --- Tests ---
describe('GameHistory', () => {
  let storage: MockStorage;
  let game: TestGame;
  let history: Sut.GameHistory<TestGameState>;

  beforeEach(() => {
    storage = new MockStorage();
    game = new TestGame({ level: 1, score: 0 });
    history = new Sut.GameHistory(
      storage,
      3,
      'history.',
      'version',
      'generate.'
    );
  });

  it('migrates and restores unversioned game state', async () => {
    // Save the initial state
    const ts1 = 1765132672690;
    const ts2 = 1765132674000;
    const storedData1 = `{"timestamp":${ts1},"data":{"level":3,"score":89}}`;
    const storedData2 = `{"timestamp":${ts2},"data":{"level":67,"score":15}}`;
    storage.setItem('testKey', storedData1);
    storage.setItem('generate.foo', 'bar');
    storage.setItem('noJson', '{ foo');
    storage.setItem('otherKey', storedData2);
    storage.setItem('noTimestamp', '{ "data": { "level": 1, "score": 2 } }');
    storage.setItem('noData', '{ "timestamp": 1234567890 }');

    // Restore the saved state
    await history.restoreGameStateAsync('testKey', game);

    // Check if the state was restored correctly
    expect(game.getState()).toEqual({ level: 3, score: 89 });

    expect(storage.sortedKeys()).toEqual([
      'generate.foo',
      'history.otherKey',
      'history.testKey',
      'version',
    ]);
    expect(storage.getItem('generate.foo')).toBe('bar');
    expect(storage.getItem('history.testKey')).toEqual(storedData1);
    expect(storage.getItem('history.otherKey')).toEqual(storedData2);
    expect(storage.getItem('version')).toBe('1');
  });

  it('restores version 1 game state', async () => {
    // Save the initial state
    const ts1 = 1765132672690;
    const ts2 = 1765132674000;
    const storedData1 = `{"timestamp":${ts1},"data":{"level":3,"score":89}}`;
    const storedData2 = `{"timestamp":${ts2},"data":{"level":67,"score":15}}`;
    storage.setItem('version', '1');
    storage.setItem('history.testKey', storedData1);
    storage.setItem('generate.foo', 'bar');
    storage.setItem('history.otherKey', storedData2);

    // Restore the saved state
    await history.restoreGameStateAsync('testKey', game);

    // Check if the state was restored correctly
    expect(game.getState()).toEqual({ level: 3, score: 89 });

    expect(storage.sortedKeys()).toEqual([
      'generate.foo',
      'history.otherKey',
      'history.testKey',
      'version',
    ]);
    expect(storage.getItem('generate.foo')).toBe('bar');
    expect(storage.getItem('history.testKey')).toEqual(storedData1);
    expect(storage.getItem('history.otherKey')).toEqual(storedData2);
    expect(storage.getItem('version')).toBe('1');
  });

  it('saves and restores game state', async () => {
    // Save the initial state
    history.saveGameState('testKey', game);

    // Modify the game state
    await game.restoreStateAsync({ level: 2, score: 100 });

    // Restore the saved state
    await history.restoreGameStateAsync('testKey', game);

    // Check if the state was restored correctly
    expect(game.getState()).toEqual({ level: 1, score: 0 });

    expect(Array.from(storage.sortedKeys()).sort()).toEqual([
      'history.testKey',
      'version',
    ]);
    expect(storage.getItem('history.testKey')).toMatch(
      /^\{"timestamp":\d+,"data":\{"level":1,"score":0\}\}$/
    );
    expect(storage.getItem('version')).toBe('1');
  });

  it('retrieves the latest game key', () => {
    // Save multiple states
    const dateSpy = vi.spyOn(Date, 'now');
    const now = 20000000;

    dateSpy.mockReturnValue(now);
    history.saveGameState('key1', game);

    dateSpy.mockReturnValue(now + 1);
    history.saveGameState('key2', game);

    dateSpy.mockReturnValue(now + 2);
    history.saveGameState('key3', game);

    // Check if the latest key is returned
    expect(history.getLatestGameKey()).toBe('key3');
  });

  it('enforces storage limit', () => {
    // Save more states than the limit
    history.saveGameState('key1', game);
    history.saveGameState('key2', game);
    history.saveGameState('key3', game);
    history.saveGameState('key4', game);
    history.saveGameState('key5', game);

    // Only the 3 most recent keys should remain
    const keys = storage.sortedKeys();
    expect(keys).toEqual([
      'history.key3',
      'history.key4',
      'history.key5',
      'version',
    ]);
  });

  it('returns null for non-existent keys', () => {
    expect(history.getLatestGameKey()).toBeNull();
    expect(storage.getItem('history.nonexistent')).toBeNull();
  });

  it('retrieves all saved games', async () => {
    // Save multiple game states
    const dateSpy = vi.spyOn(Date, 'now');
    const now = 20000000;

    dateSpy.mockReturnValue(now);
    history.saveGameState('key1', game);

    dateSpy.mockReturnValue(now + 1000);
    await game.restoreStateAsync({ level: 5, score: 250 });
    history.saveGameState('key2', game);

    dateSpy.mockReturnValue(now + 2000);
    await game.restoreStateAsync({ level: 10, score: 500 });
    history.saveGameState('key3', game);

    // Retrieve all saved games
    const allGames = history.getAllSavedGames();

    // Verify the correct number of games were retrieved
    expect(allGames).toHaveLength(3);

    // Verify all games are present with correct data
    const gamesByKey = new Map(allGames.map((g) => [g.key, g.data]));

    expect(gamesByKey.get('key1')).toEqual({
      timestamp: now,
      data: { level: 1, score: 0 },
    });
    expect(gamesByKey.get('key2')).toEqual({
      timestamp: now + 1000,
      data: { level: 5, score: 250 },
    });
    expect(gamesByKey.get('key3')).toEqual({
      timestamp: now + 2000,
      data: { level: 10, score: 500 },
    });

    dateSpy.mockRestore();
  });

  it('returns empty array when no games are saved', () => {
    const allGames = history.getAllSavedGames();
    expect(allGames).toEqual([]);
  });

  it('skips corrupt game states when retrieving all saved games', () => {
    // Save valid game states
    const dateSpy = vi.spyOn(Date, 'now');
    const now = 20000000;

    dateSpy.mockReturnValue(now);
    history.saveGameState('key1', game);

    dateSpy.mockReturnValue(now + 1000);
    history.saveGameState('key2', game);

    // Add a corrupt entry directly to storage
    storage.setItem('history.corrupt', '{ invalid json');
    storage.setItem('history.noData', '{ "timestamp": 1234567890 }');
    storage.setItem('history.noTimestamp', '{ "data": { "level": 1 } }');

    // Retrieve all saved games
    const allGames = history.getAllSavedGames();

    // Only valid games should be returned
    expect(allGames).toHaveLength(2);
    expect(allGames.map((g) => g.key)).toEqual(['key1', 'key2']);

    dateSpy.mockRestore();
  });
});
