// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// TODO: declare an interface for the str8ts-api modules
export type ApiResult = { status: number; message: string };

const worker = new Worker('generate-worker.js');

export function generate(size: number, difficulty: number): Promise<ApiResult> {
  return run_in_worker({
    method: 'generate',
    size,
    difficulty,
  });
}

export function generateHint(gameAsJson: number[][][]): Promise<ApiResult> {
  return run_in_worker({
    method: 'hint',
    gameAsJson: JSON.stringify(gameAsJson),
  });
}

function run_in_worker(
  message:
    | { method: 'generate'; size?: number; difficulty?: number }
    | { method: 'hint'; gameAsJson: string }
): Promise<ApiResult> {
  return new Promise((resolve, reject) => {
    worker.onmessage = (event) => {
      resolve(event.data as ApiResult);
    };

    worker.onerror = (error) => {
      reject(error);
    };

    worker.postMessage(message);
  });
}
