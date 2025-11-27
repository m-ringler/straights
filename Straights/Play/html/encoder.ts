export interface EncodedResult {
  base64Data: string;
  count: number; // Number of sets encoded
}

// Control byte values
const CONTROL_BYTE_UNCOMPRESSED = 0;
const CONTROL_BYTE_COMPRESSED = 1;

/**
 * Bitmask encoder with configurable compression settings
 */
export class BitmaskEncoder {
  readonly compressionThreshold: number;
  readonly minCompressionRatio: number;
  readonly maxN: number;

  /**
   * Create a new BitmaskEncoder instance
   * @param options Configuration options
   * @param options.compressionThreshold Minimum buffer size in bytes before attempting compression
   * @param options.minCompressionRatio Maximum compression ratio (compressed/original) to use compression
   * @param options.maxN Maximum value for n parameter, must be between 1 and 32
   */
  constructor(options: {
    compressionThreshold: number;
    minCompressionRatio: number;
    maxN: number;
  }) {
    this.compressionThreshold = options.compressionThreshold;
    this.minCompressionRatio = options.minCompressionRatio;
    this.maxN = options.maxN;

    if (this.maxN < 1 || this.maxN > 32) {
      throw new Error('maxN must be between 1 and 32');
    }
  }

  /**
   * Encode an iterable of sets as a bitmask with optional gzip compression
   * @param n - Number between 1 and maxN representing the size of each set's domain
   * @param sets - Iterable of up to n*n sets containing numbers between 1 and n (inclusive).
   * @returns EncodedResult containing compression flag and base64url-encoded data
   */
  async encode(n: number, sets: Iterable<Iterable<number>>): Promise<EncodedResult> {
    if (n < 1 || n > this.maxN) {
      throw new Error(`n must be between 1 and ${this.maxN}`);
    }

    // Convert sets to array for processing
    const setsArray = Array.from(sets);

    // Calculate total bits needed (including 1 control byte for flags)
    const bitsPerSet = n;
    const totalBits = setsArray.length * bitsPerSet;
    const totalBytes = Math.ceil(totalBits / 8) + 1; // +1 for control byte

    // Create buffer to hold control byte + all bitmasks
    const buffer = new Uint8Array(totalBytes);

    // Encode each set as a bitmask (starting after control byte)
    let bitOffset = 8; // Start after the first byte (control byte)
    for (const set of setsArray) {
      // Validate set values
      for (const value of set) {
        if (value < 1 || value > n) {
          throw new Error(`Set contains invalid value ${value}: must be between 1 and ${n}`);
        }
      }

      // Create bitmask for this set (bit i represents number i+1)
      let bitmask = 0;
      for (const value of set) {
        bitmask |= (1 << (value - 1));
      }

      // Write bitmask bits into buffer
      for (let bit = 0; bit < bitsPerSet; bit++) {
        const bitValue = (bitmask >> bit) & 1;
        if (bitValue) {
          const byteIndex = Math.floor(bitOffset / 8);
          const bitIndex = bitOffset % 8;
          buffer[byteIndex] |= (1 << bitIndex);
        }
        bitOffset++;
      }
    }

    // Check if we should compress
    if (totalBytes - 1 > this.compressionThreshold) {
      const dataToCompress = buffer.slice(1); // Exclude control byte
      const compressed = await compressData(dataToCompress, CONTROL_BYTE_COMPRESSED);
      const compressionRatio = (compressed.length - 1) / dataToCompress.length; // Subtract control byte from comparison
      if (compressionRatio < this.minCompressionRatio) {
        // Use compressed data (already has control byte set to compressed)
        const base64Data = toBase64Url(compressed);
        return { base64Data, count: setsArray.length };
      }
    }

    // Not compressed: set control byte to uncompressed
    buffer[0] = CONTROL_BYTE_UNCOMPRESSED;

    // Encode as base64url
    const base64Data = toBase64Url(buffer);

    return { base64Data, count: setsArray.length };
  }

  /**
   * Decode a bitmask-encoded result back to an array of sets
   * @param encoded - The encoded result from encode()
   * @param n - Number between 1 and maxN representing the size of each set's domain
   * @returns Array of sets containing the decoded numbers
   */
  async decode(encoded: EncodedResult, n: number): Promise<Set<number>[]> {
    if (n < 1 || n > this.maxN) {
      throw new Error(`n must be between 1 and ${this.maxN}`);
    }

    // Decode from base64url
    let buffer = fromBase64Url(encoded.base64Data);

    // Handle empty data
    if (buffer.length === 0 || encoded.count === 0) {
      return [];
    }

    // Read control byte from first byte
    const controlByte = buffer[0];
    if (controlByte !== CONTROL_BYTE_UNCOMPRESSED && controlByte !== CONTROL_BYTE_COMPRESSED) {
      throw new Error(`Unknown control byte: ${controlByte}`);
    }
    let bitmaskBuffer = buffer.slice(1);
    if (controlByte === CONTROL_BYTE_COMPRESSED) {
      bitmaskBuffer = new Uint8Array(await decompressData(bitmaskBuffer));
    }

    // Calculate how many sets we have from the encoded count
    const numSets = encoded.count;
    const bitsPerSet = n;

    // Decode each set from the bitmask
    const sets: Set<number>[] = [];
    let bitOffset = 0;

    for (let i = 0; i < numSets; i++) {
      const set = new Set<number>();

      // Read bitsPerSet bits to reconstruct the bitmask
      let bitmask = 0;
      for (let bit = 0; bit < bitsPerSet; bit++) {
        const byteIndex = Math.floor(bitOffset / 8);
        const bitIndex = bitOffset % 8;
        const bitValue = (bitmaskBuffer[byteIndex] >> bitIndex) & 1;
        if (bitValue) {
          bitmask |= (1 << bit);
        }
        bitOffset++;
      }

      // Convert bitmask back to set of numbers
      let testBit = 1;
      for (let value = 1; value <= n; value++) {
        if (bitmask & testBit) {
          set.add(value);
        }
        testBit <<= 1;
      }

      sets.push(set);
    }

    return sets;
  }
}

/**
 * Compress data using gzip and prepend control byte
 */
async function compressData(data: Uint8Array, controlByte: number): Promise<Uint8Array> {
  const stream = new ReadableStream({
    start(controller) {
      controller.enqueue(data);
      controller.close();
    }
  });

  const compressedStream = stream.pipeThrough(
    new CompressionStream('gzip')
  );

  const chunks: Uint8Array[] = [];
  const reader = compressedStream.getReader();

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    chunks.push(value);
  }

  // Combine chunks into single Uint8Array with control byte prepended
  const totalLength = chunks.reduce((acc, chunk) => acc + chunk.length, 0);
  const result = new Uint8Array(1 + totalLength);
  result[0] = controlByte;
  let offset = 1;
  for (const chunk of chunks) {
    result.set(chunk, offset);
    offset += chunk.length;
  }

  return result;
}

/**
 * Decompress gzip data
 */
async function decompressData(data: Uint8Array): Promise<Uint8Array> {
  const stream = new ReadableStream({
    start(controller) {
      controller.enqueue(data);
      controller.close();
    }
  });

  const decompressedStream = stream.pipeThrough(
    new DecompressionStream('gzip')
  );

  const chunks: Uint8Array[] = [];
  const reader = decompressedStream.getReader();

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    chunks.push(value);
  }

  // Combine chunks into single Uint8Array
  const totalLength = chunks.reduce((acc, chunk) => acc + chunk.length, 0);
  const result = new Uint8Array(totalLength);
  let offset = 0;
  for (const chunk of chunks) {
    result.set(chunk, offset);
    offset += chunk.length;
  }

  return result;
}

/**
 * Convert Uint8Array to base64url string
 */
function toBase64Url(data: Uint8Array): string {
  // Handle empty array
  if (data.length === 0) {
    return '';
  }
  
  // Use native method if available (modern browsers)
  if (typeof (data as any).toBase64 === 'function') {
    return (data as any).toBase64({ alphabet: 'base64url' });
  }
  
  // Fallback for environments without native support
  const base64 = btoa(String.fromCharCode(...data));
  return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

/**
 * Convert base64url string to Uint8Array
 */
function fromBase64Url(base64url: string): Uint8Array {
  // Handle empty string
  if (base64url.length === 0) {
    return new Uint8Array(0);
  }
  
  // Use native method if available (modern browsers)
  if (typeof (Uint8Array as any).fromBase64 === 'function') {
    return (Uint8Array as any).fromBase64(base64url, { alphabet: 'base64url' });
  }
  
  // Fallback for environments without native support
  // Convert base64url to standard base64
  let base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
  // Add padding
  while (base64.length % 4) {
    base64 += '=';
  }
  
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes;
}
