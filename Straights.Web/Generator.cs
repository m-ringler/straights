// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Web;

using System.Runtime.InteropServices;
using System.Text;
using static Straights.Solver.Play;

/// <summary>
/// Provides methods for generating game codes for the Straights game.
/// </summary>
public class Generator
{
    /// <summary>
    /// Entry point for generating a game code. This method is intended to be called
    /// from JavaScript code and marshals the result back to the caller.
    /// </summary>
    /// <param name="size">The size of the game board to generate.</param>
    /// <param name="difficulty">The difficulty level of the game to generate.</param>
    /// <param name="gridLayout">The grid layout of the game to generate.</param>
    /// <param name="result">A pointer to the buffer where the generated game code will be stored.</param>
    /// <param name="resultLength">The length of the buffer pointed to by <paramref name="result"/>.</param>
    /// <returns>
    /// Returns 0 if the game code was successfully generated and marshaled to the buffer.
    /// Returns -1 if an error occurred during generation or marshaling.
    /// </returns>
    /// <seealso cref="Memory.Allocate(nuint)"/>
    [UnmanagedCallersOnly(EntryPoint = "Generator_Generate")]
    public static int Generate(
        int size,
        int difficulty,
        int gridLayout,
        nint result,
        int resultLength
    )
    {
        return new StringOp(result, resultLength).RunAndMarshal(() =>
            GenerateGameCode(size, difficulty, gridLayout)
        );
    }

    /// <summary>
    /// Entry point for generating a hint. This method is intended to be called
    /// from JavaScript code and marshals the result back to the caller.
    /// </summary>
    /// <param name="buffer">
    /// A pointer to the buffer containing the game json,
    /// will be reused to store the result hint or the exception message.
    /// </param>
    /// <param name="bufferLength">
    /// The length of the buffer pointed to by <paramref name="buffer"/>.
    /// </param>
    /// <param name="gameLength">
    /// The length of the game json in the buffer pointed to by <paramref name="buffer"/>.
    /// </param>
    /// <returns>
    /// Returns 0 if the game code was successfully generated and marshaled to the buffer.
    /// Returns -1 if an error occurred during generation or marshaling.
    /// </returns>
    /// <seealso cref="Memory.Allocate(nuint)"/>
    [UnmanagedCallersOnly(EntryPoint = "Generator_Hint")]
    public static int Hint(nint buffer, int bufferLength, int gameLength)
    {
        var gameAsJson =
            Marshal.PtrToStringUTF8(buffer, gameLength)
            ?? throw new ArgumentNullException(nameof(buffer));

        return new StringOp(buffer, bufferLength).RunAndMarshal(() =>
            GenerateHint(gameAsJson)
        );
    }

    private sealed class StringOp(nint buffer, int bufferLength)
    {
        public int RunAndMarshal(Func<string> operation)
        {
            try
            {
                this.Clear();
                this.WriteAsUTF8(operation());
                return 0; // Success
            }
            catch (Exception ex)
            {
                this.TryWriteAsUTF8(ex.Message);

                return -1;
            }
        }

        private void TryWriteAsUTF8(string s)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(s);
                if (bytes.Length <= bufferLength)
                {
                    Marshal.Copy(bytes, 0, buffer, bytes.Length);
                }
            }
            catch { }
        }

        private void WriteAsUTF8(string r)
        {
            var bytes = Encoding.UTF8.GetBytes(r);
            if (bytes.Length > bufferLength)
            {
                throw new ArgumentException(
                    $"Buffer size {bufferLength} is too small to hold the result, required size: {bytes.Length}."
                );
            }

            Marshal.Copy(bytes, 0, buffer, bytes.Length);
        }

        private unsafe void Clear()
        {
            nuint count;
            checked
            {
                count = (nuint)bufferLength;
            }

            NativeMemory.Clear((void*)buffer, count);
        }
    }
}
