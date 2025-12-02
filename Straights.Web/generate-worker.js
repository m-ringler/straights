// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

importScripts('Straights.Web.js');

const ModulePromise = createWasmModule();

self.onmessage = async (e) => {
  Module = await ModulePromise;

  self.postMessage(_run(e.data));
};

function _run(data) {
  if (data.method === 'generate') {
    return _generate(data.size, data.difficulty);
  } else if (data.method === 'hint') {
    return _hint(data.gameAsJson);
  }

  return { status: -1, message: `Unknown method ${data.method}` };
}

function _hint(gameAsJson) {
  const bufferSize = Math.max(gameAsJson.length * 4, 4096);
  const bufferPtr = Module._Memory_Allocate(bufferSize);
  try {
    const gameLength = Module.stringToUTF8(gameAsJson, bufferPtr, bufferSize);

    const status = Module._Generator_Hint(bufferPtr, bufferSize, gameLength);
    const message = Module.UTF8ToString(bufferPtr);

    return { status, message };
  } finally {
    Module._Memory_Free(bufferPtr);
  }
}

function _generate(size, difficulty) {
  const bufferSize = 512;
  const bufferPtr = Module._Memory_Allocate(bufferSize);

  try {
    const status = Module._Generator_Generate(
      size,
      difficulty,
      bufferPtr,
      bufferSize
    );

    const message = Module.UTF8ToString(bufferPtr);
    return { status, message };
  } finally {
    Module._Memory_Free(bufferPtr);
  }
}
