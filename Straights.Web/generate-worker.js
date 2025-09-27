// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

importScripts('Straights.Web.js')

const ModulePromise = createWasmModule()

self.onmessage = e => async
{
    Module = await ModulePromise

    const edata = e.data
    let { status, resultString } = _run(edata)

    self.postMessage({
        status,
        message: resultString
    })
}

function _run(edata) {
    let status = -1
    let resultString = ''

    if (edata.method === 'generate') {
        ({ status, resultString } = _generate(edata.size, edata.difficulty))
    } else if (edata.method === 'hint') {
        ({ status, resultString } = _hint(edata.gameAsJson))
    }

    return { status, resultString }
}

function _hint(gameAsJson) {
    const bufferSize = Math.max(gameAsJson.length * 4, 4096)
    const bufferPtr = Module._Memory_Allocate(bufferSize)
    try {
        const gameLength = Module.stringToUTF8(gameAsJson, bufferPtr, bufferSize)

        const status = Module._Generator_Hint(bufferPtr, bufferSize, gameLength)
        const resultString = Module.UTF8ToString(bufferPtr)

        return { status, resultString }

    } finally {
        Module._Memory_Free(bufferPtr)
    }
}

function _generate(size, difficulty) {
    const bufferSize = 512
    const bufferPtr = Module._Memory_Allocate(bufferSize)

    try {
        const status = Module._Generator_Generate(size, difficulty, bufferPtr, bufferSize)

        const resultString = Module.UTF8ToString(bufferPtr)
        return { status, resultString }

    } finally {
        Module._Memory_Free(bufferPtr)
    }
}

