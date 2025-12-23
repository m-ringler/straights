// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

// This provides the generate function
// from an API endpoint running on the server.

export type ApiResult = { status: number; message: string };

export async function generate(
  size: number,
  difficulty: number,
  gridLayout: number
): Promise<ApiResult> {
  const response = await fetch(
    `/generate?gridSize=${size}&difficulty=${difficulty - 1}&gridLayout=${gridLayout}`
  );
  const data = await response.json();
  return data as ApiResult;
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
  return data as ApiResult;
}
