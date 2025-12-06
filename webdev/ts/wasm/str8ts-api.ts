// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// TODO: declare an interface for the str8ts-api modules
export type ApiResult = { status: number; message: string };

const worker = new Worker('str8ts-api-worker.js');

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
    const handleMessage = (event: MessageEvent<ApiResult>) => {
      worker.removeEventListener('error', handleError);
      resolve(event.data);
    };

    const handleError = (error: ErrorEvent) => {
      worker.removeEventListener('message', handleMessage);
      reject(error);
    };

    worker.addEventListener('message', handleMessage, { once: true });
    worker.addEventListener('error', handleError, { once: true });

    worker.postMessage(message);
  });
}
