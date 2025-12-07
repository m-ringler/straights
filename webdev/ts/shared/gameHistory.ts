// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export interface StorageProvider {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  key(index: number): string | null;
  get length(): number;
}

export interface GameLike<TState> {
  dumpState(): TState;
  restoreState(state: TState): void;
}

type GameState<TState> = {
  timestamp: number;
  data: TState;
};

export class GameHistory<TState> {
  private readonly maxNumberOfStoredGames: number;
  private readonly storagePrefix: string;
  private readonly versionKey: string;
  private readonly generatePrefix: string;
  private readonly storage: StorageProvider;

  constructor(
    storage: StorageProvider,
    maxNumberOfStoredGames: number = 50,
    storagePrefix: string = 'history.',
    versionKey: string = 'version',
    generatePrefix: string = 'generate.'
  ) {
    if (!storagePrefix) {
      throw new Error('storagePrefix must not be empty');
    }

    if (!versionKey) {
      throw new Error('versionKey must not be empty');
    }

    if (!generatePrefix) {
      throw new Error('generatePrefix must not be empty');
    }

    this.storage = storage;
    this.maxNumberOfStoredGames = maxNumberOfStoredGames;
    this.storagePrefix = storagePrefix;
    this.versionKey = versionKey;
    this.generatePrefix = generatePrefix;
  }

  public saveGameState(key: string, game: GameLike<TState>): void {
    this.migrate();
    const gameState: GameState<TState> = {
      timestamp: Date.now(),
      data: game.dumpState(),
    };
    const gameStateString = JSON.stringify(gameState);
    this.storage.setItem(this.storagePrefix + key, gameStateString);

    this.ensureStorageLimit();
  }

  public restoreGameState(key: string, game: GameLike<TState>): void {
    this.migrate();
    const savedGameState = this.loadGameStateData(this.storagePrefix + key);
    if (savedGameState) {
      game.restoreState(savedGameState.data);
    }
  }

  public getLatestGameKey(): string | null {
    this.migrate();
    const prefixedKeys = this.getPrefixedHistoryKeys();
    let latestKey: string | null = null;
    let latestTimestamp = 0;

    prefixedKeys.forEach((prefixedKey) => {
      const gameState = this.loadGameStateData(prefixedKey);
      if (gameState && gameState.timestamp > latestTimestamp) {
        latestTimestamp = gameState.timestamp;
        latestKey = prefixedKey.substring(this.storagePrefix.length);
      }
    });

    return latestKey;
  }

  private getPrefixedHistoryKeys(): string[] {
    return this.getKeys((key) => key.startsWith(this.storagePrefix));
  }

  private getKeys(include: (arg0: string) => boolean): string[] {
    const keys: string[] = [];
    for (let i = 0; i < this.storage.length; i++) {
      const key = this.storage.key(i);
      if (key && include(key)) {
        keys.push(key);
      }
    }
    return keys;
  }

  private loadGameStateData(prefixedKey: string): GameState<TState> | null {
    if (!prefixedKey.startsWith(this.storagePrefix)) {
      return null;
    }
    return this.loadGameStateDataCore(prefixedKey);
  }

  private loadGameStateDataCore(prefixedKey: string): GameState<TState> | null {
    try {
      const gameStateString = this.storage.getItem(prefixedKey);
      if (!gameStateString) {
        return null;
      }
      const result = JSON.parse(gameStateString) as GameState<TState>;
      return result.timestamp && result.data ? result : null;
    } catch (e) {
      console.warn(
        'Error loading game state from storage for key:',
        prefixedKey,
        e
      );
      return null;
    }
  }

  private ensureStorageLimit(): void {
    const prefixedKeys = this.getPrefixedHistoryKeys();
    if (prefixedKeys.length > this.maxNumberOfStoredGames) {
      console.info('Cropping game history');
      const keysByAge: { key: string; timestamp: number }[] = [];

      for (const pk of prefixedKeys) {
        const gameState = this.loadGameStateData(pk);
        if (!gameState) {
          console.debug(`Removing corrupt history entry ${pk}`);
          this.storage.removeItem(pk);
        } else {
          keysByAge.push({ key: pk, timestamp: gameState.timestamp });
        }
      }

      keysByAge.sort((a, b) => a.timestamp - b.timestamp);
      for (let i = 0; i < keysByAge.length - this.maxNumberOfStoredGames; i++) {
        const oldKey = keysByAge[i].key;
        console.debug(`Removing old history entry ${oldKey}`);
        this.storage.removeItem(oldKey);
      }
    }
  }

  private migrate(): void {
    if (this.storage.getItem(this.versionKey)) {
      return;
    }

    const keysToMigrate: string[] = this.getKeys(
      (key) =>
        !key.startsWith(this.storagePrefix) &&
        !key.startsWith(this.generatePrefix)
    );

    for (const key of keysToMigrate) {
      const item = this.storage.getItem(key);
      if (!item) {
        console.debug(`Removing unknown storage key ${key}`);
      } else {
        try {
          const parsed = JSON.parse(item) as {
            timestamp?: number;
            data?: unknown;
          };
          if (parsed.timestamp && parsed.data) {
            console.debug(`Migrating game history entry ${key}`);
            this.storage.setItem(this.storagePrefix + key, item);
          } else {
            console.debug(`Removing unknown storage key ${key}`);
          }
        } catch (e) {
          console.debug(`Removing unknown storage key ${key}`);
        }
      }
      this.storage.removeItem(key);
    }

    this.storage.setItem(this.versionKey, '1');
    console.info('Migrated game history data');
  }
}

// Example usage:
// const history = new GameHistory<MyGameState>(localStorage);
// history.saveGameState('key', myGame);
// history.restoreGameState('key', myGame);
// const latestKey = history.getLatestGameKey();
