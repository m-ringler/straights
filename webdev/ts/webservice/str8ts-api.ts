// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// This provides the generate function
// from an API endpoint running on the server.

export type ApiResult = { status: number; message: string };

function isApiResult(data: unknown): data is ApiResult {
  return (
    typeof data === 'object' &&
    data !== null &&
    'status' in data &&
    typeof (data as Record<string, unknown>).status === 'number' &&
    'message' in data &&
    typeof (data as Record<string, unknown>).message === 'string'
  );
}

export async function generate(
  size: number,
  difficulty: number,
  gridLayout: number
): Promise<ApiResult> {
  const response = await fetch(
    `/generate?gridSize=${size}&difficulty=${difficulty - 1}&gridLayout=${gridLayout}`
  );
  const data = await response.json();
  if (!isApiResult(data)) {
    throw new Error(
      `Invalid API response: expected ApiResult with status and message properties`
    );
  }
  return data;
}

export async function generateHint(
  gameAsJson: number[][][]
): Promise<ApiResult> {
  const response = await fetch('/hint', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(gameAsJson),
  });
  const data = await response.json();
  if (!isApiResult(data)) {
    throw new Error(
      `Invalid API response: expected ApiResult with status and message properties`
    );
  }
  return data;
}
