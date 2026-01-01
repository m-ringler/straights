// SPDX-FileCopyrightText: 2025-2026 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

export interface CheckerboardOptions {
  gridSizePixels?: number;
  borderColor?: string;
  trueColor?: string;
  falseColor?: string;
}

export function renderCheckerboard(
  canvas: HTMLCanvasElement,
  data: boolean[][],
  options: CheckerboardOptions
) {
  const {
    gridSizePixels = 100,
    borderColor = '#133',
    trueColor = '#333',
    falseColor = 'white',
  } = options;

  const gridSize = data.length;
  const cellSizePixels = (gridSizePixels * 1.0) / gridSize;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('Could not get canvas context');

  // Set canvas dimensions
  canvas.width = gridSizePixels;
  canvas.height = gridSizePixels;

  // Render grid
  for (let y = 0; y < gridSize; y++) {
    for (let x = 0; x < gridSize; x++) {
      const cell = data[y][x];
      ctx.fillStyle = cell ? trueColor : falseColor;
      ctx.fillRect(
        x * cellSizePixels,
        y * cellSizePixels,
        cellSizePixels,
        cellSizePixels
      );
      ctx.strokeStyle = borderColor;
      ctx.strokeRect(
        x * cellSizePixels,
        y * cellSizePixels,
        cellSizePixels,
        cellSizePixels
      );
    }
  }
}
