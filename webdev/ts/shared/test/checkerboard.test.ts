// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

import { describe, it, expect, beforeEach } from 'vitest';
import { renderCheckerboard } from '../checkerboard';

interface CanvasDrawingRecord {
  coords: [number, number, number, number];
  fill?: string;
  stroke?: string;
}

class MockCanvasContext {
  private records: CanvasDrawingRecord[] = [];
  fillStyle: string = '';
  strokeStyle: string = '';

  fillRect(x: number, y: number, width: number, height: number): void {
    this.records.push({
      coords: [x, y, width, height],
      fill: this.fillStyle,
    });
  }

  strokeRect(x: number, y: number, width: number, height: number): void {
    this.records.push({
      coords: [x, y, width, height],
      stroke: this.strokeStyle,
    });
  }

  getRecords(): CanvasDrawingRecord[] {
    return this.records;
  }

  getRecordsAsStrings(): string[] {
    return this.records.map((record) => {
      const fill = record.fill ? `fill:${record.fill}` : '';
      const stroke = record.stroke ? `stroke:${record.stroke}` : '';
      const coords = `[${record.coords.join(',')}]`;
      const attrs = [coords, fill, stroke].filter(Boolean).join(' ');
      return `${attrs}`;
    });
  }
}

class MockCanvas {
  width: number = 0;
  height: number = 0;
  private mockContext: MockCanvasContext;

  constructor() {
    this.mockContext = new MockCanvasContext();
  }

  getContext(contextType: string): MockCanvasContext | null {
    if (contextType === '2d') {
      return this.mockContext;
    }
    return null;
  }

  getRecords(): CanvasDrawingRecord[] {
    return this.mockContext.getRecords();
  }

  getRecordsAsStrings(): string[] {
    return this.mockContext.getRecordsAsStrings();
  }
}

describe('renderCheckerboard', () => {
  let mockCanvas: MockCanvas;

  beforeEach(() => {
    mockCanvas = new MockCanvas();
  });

  it('should render a simple 2x2 checkerboard with default options', () => {
    const data: boolean[][] = [
      [true, false],
      [false, true],
    ];

    renderCheckerboard(mockCanvas as any, data, {});

    expect(mockCanvas.width).toBe(100);
    expect(mockCanvas.height).toBe(100);
    expect(mockCanvas.getRecordsAsStrings()).toMatchSnapshot();
  });

  it('should render a 2x2 checkerboard with custom colors', () => {
    const data: boolean[][] = [
      [true, false],
      [false, true],
    ];

    renderCheckerboard(mockCanvas as any, data, {
      trueColor: '#FF0000',
      falseColor: '#00FF00',
      borderColor: '#0000FF',
    });

    expect(mockCanvas.getRecordsAsStrings()).toMatchSnapshot();
  });

  it('should render a 3x3 checkerboard', () => {
    const data: boolean[][] = [
      [true, false, true],
      [false, true, false],
      [true, false, true],
    ];

    renderCheckerboard(mockCanvas as any, data, {
      gridSizePixels: 300,
    });

    expect(mockCanvas.width).toBe(300);
    expect(mockCanvas.height).toBe(300);
    expect(mockCanvas.getRecordsAsStrings()).toMatchSnapshot();
  });

  it('should render a single cell checkerboard', () => {
    const data: boolean[][] = [[true]];

    renderCheckerboard(mockCanvas as any, data, {
      gridSizePixels: 50,
    });

    expect(mockCanvas.width).toBe(50);
    expect(mockCanvas.height).toBe(50);
    expect(mockCanvas.getRecordsAsStrings()).toMatchSnapshot();
  });
});
