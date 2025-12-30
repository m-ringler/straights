// checkerboard.ts
interface CheckerboardOptions {
  gridSize: number;
  cellSizePixels?: number;
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
    gridSize,
    cellSizePixels = 10,
    borderColor = '#133',
    trueColor = '#333',
    falseColor = 'white',
  } = options;

  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('Could not get canvas context');

  // Set canvas dimensions
  canvas.width = gridSize * cellSizePixels;
  canvas.height = gridSize * cellSizePixels;

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
