// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Web;

using System.Runtime.InteropServices;
using System.Text;

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
    /// <param name="result">A pointer to the buffer where the generated game code will be stored.</param>
    /// <param name="resultLength">The length of the buffer pointed to by <paramref name="result"/>.</param>
    /// <returns>
    /// Returns 0 if the game code was successfully generated and marshaled to the buffer.
    /// Returns -1 if an error occurred during generation or marshaling.
    /// </returns>
    /// <seealso cref="Memory.Allocate(nuint)"/>
    [UnmanagedCallersOnly(EntryPoint = "Generator_Generate")]
    public static int Generate(int size, int difficulty, nint result, int resultLength)
    {
        return GenerateAndMarshal(size, difficulty, result, resultLength);
    }

    private static int GenerateAndMarshal(int size, int difficulty, nint result, int resultLength)
    {
        try
        {
            var r = GenerateCore(size, difficulty);
            var bytes = Encoding.UTF8.GetBytes(r);
            if (bytes.Length > resultLength)
            {
                throw new ArgumentException(
                    $"Buffer size {resultLength} is too small to hold the result, required size: {bytes.Length}.");
            }

            Marshal.Copy(bytes, 0, result, bytes.Length);
            return 0; // Success
        }
        catch (Exception ex)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(ex.Message);
                if (bytes.Length <= resultLength)
                {
                    Marshal.Copy(bytes, 0, result, bytes.Length);
                }
            }
            catch
            {
            }

            return -1;
        }
    }

    private static string GenerateCore(int size, int difficulty)
    {
        return Straights.Solver.Play.GenerateGameCode(size, difficulty);
    }
}