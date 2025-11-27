// Type definitions for TC39 Stage 3 Uint8Array base64 proposal
// https://github.com/tc39/proposal-arraybuffer-base64

interface Uint8ArrayConstructor {
  fromBase64(string: string, options?: { alphabet?: 'base64' | 'base64url'; lastChunkHandling?: 'loose' | 'strict' | 'stop-before-partial' }): Uint8Array;
  fromHex(string: string): Uint8Array;
}

interface Uint8Array {
  toBase64(options?: { alphabet?: 'base64' | 'base64url'; omitPadding?: boolean }): string;
  toHex(): string;
  setFromBase64(string: string, options?: { alphabet?: 'base64' | 'base64url'; lastChunkHandling?: 'loose' | 'strict' | 'stop-before-partial' }): { read: number; written: number };
  setFromHex(string: string): { read: number; written: number };
}
