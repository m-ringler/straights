// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.CommandLine.IO;
using System.Text;

internal sealed class ConsoleStub : IConsole
{
    public StandardStreamWriterStub Out { get; } = new();

    public StandardStreamWriterStub Error { get; } = new();

    IStandardStreamWriter IStandardOut.Out => this.Out;

    IStandardStreamWriter IStandardError.Error => this.Error;

    public bool IsOutputRedirected => false;

    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => false;

    public sealed class StandardStreamWriterStub : IStandardStreamWriter
    {
        public StringBuilder Buffer { get; } = new();

        public void Write(string? value)
        {
            this.Buffer.Append(value);
        }
    }
}