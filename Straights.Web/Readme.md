# Straights.Web

[![Straights.Web Build](https://github.com/m-ringler/straights/actions/workflows/straights-web.yml/badge.svg)](https://github.com/m-ringler/straights/actions/workflows/straights-web.yml)

Straights.Web is a client-only web application that lets you generate and play straights puzzles.

The C# project builds a web assembly module that is used to generate straight puzzles; this web assembly module is then integrated with static web content from [straights play](../Straights/Play/html).

The result is a webapp that can be deployed as a static website.

When served via HTTPS, Straights.Web is a [progressive web app](https://en.wikipedia.org/wiki/Progressive_web_app) that can be installed on users' devices as a standalone app (Add to home sceen). It will then remain fully functional when offline.

## WebAssembly from C&#35;

WebAssembly is generated using NativeAOT via LLVM and Emscripten. This approach is documented at the dotnet runtimelab's github project site:

* [NativeAot-LLVM Sample](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT-LLVM/samples/NativeLibrary)
* [Compiling webassembly](https://github.com/dotnet/runtimelab/blob/ce43f371e1761fe6335f8c63d344e605b892ebe9/docs/using-nativeaot/compiling.md#webassembly)

## Compiling

* Download and activate the
[Emscripten SDK](https://emscripten.org/docs/getting_started/downloads.html#sdk-download-and-install).
* Install a dotnet SDK that matches the SDK version in [global.json](../global.json).
* Then run:

~~~sh
dotnet publish -r browser-wasm -c Release
~~~

## Running

~~~sh
emrun publish/index.html
~~~

## Deploying to a web server

Copy the contents of the publish folder to a subdirectory of the htdocs / wwwroot of your web server. Configure the web server so that it

* serves `.wasm` files as mime type `application/wasm`,
* uses gzip content-encoding for `.wasm` and `js` files.

[.htaccess](.htaccess) contains corresponding directives for Apache web servers.
