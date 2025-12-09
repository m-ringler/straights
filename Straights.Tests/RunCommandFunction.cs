// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

internal sealed class RunCommandFunction<TCommand>(int returnValue)
{
    public TCommand? InvokedCommand { get; private set; }

    public int Invoke(TCommand command)
    {
        this.InvokedCommand = command;
        return returnValue;
    }
}
