// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Web;

using System.Runtime.InteropServices;

/// <summary>
/// Provides methods for memory management on the JavaScript side.
/// </summary>
public unsafe class Memory
{
    /// <summary>
    /// Frees a block of unmanaged memory that was previously allocated.
    /// </summary>
    /// <param name="p">A pointer to the block of memory to be freed.</param>
    /// <remarks>
    /// This method is intended to be used with unmanaged code and should be called
    /// only when the memory is no longer needed to avoid memory leaks or undefined behavior.
    /// </remarks>
    /// <seealso cref="System.Runtime.InteropServices.NativeMemory.Free(void*)"/>
    [UnmanagedCallersOnly(EntryPoint = "Memory_Free")]
    public static void Free(void* p)
    {
        NativeMemory.Free(p);
    }

    /// <summary>
    /// Allocates a block of memory of the specified size and initializes it to zero.
    /// </summary>
    /// <param name="size">The size, in bytes, of the memory block to allocate.</param>
    /// <returns>A pointer to the allocated memory block.</returns>
    /// <remarks>
    /// This method uses <see cref="NativeMemory.Alloc(nuint)"/> to allocate the memory
    /// and <see cref="NativeMemory.Clear"/> to initialize it to zero.
    /// The caller is responsible for freeing the allocated memory when it is no longer needed.
    /// </remarks>
    [UnmanagedCallersOnly(EntryPoint = "Memory_Allocate")]
    public static void* Allocate(nuint size)
    {
        var result = NativeMemory.Alloc(size);
        NativeMemory.Clear(result, size);
        return result;
    }
}
