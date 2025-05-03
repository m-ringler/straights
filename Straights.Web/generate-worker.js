// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

importScripts('Straights.Web.js')

const ModulePromise = createWasmModule()

self.onmessage = (event) => {
  ModulePromise.then((Module) => {
    const { size, difficulty } = event.data

    const bufferSize = 512
    const bufferPtr = Module._Memory_Allocate(bufferSize)

    const status = Module._Generator_Generate(size, difficulty, bufferPtr, bufferSize)

    const resultString = Module.UTF8ToString(bufferPtr)
    Module._Memory_Free(bufferPtr)

    self.postMessage({
      status,
      message: resultString
    })
  })
}
