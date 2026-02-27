namespace Straights.Image.Tests;

using System.Runtime.CompilerServices;

public static class Init
{
    [ModuleInitializer]
    public static void Initialize()
    {
        NativeLibraryLoader.EnsureInitialized();
    }
}
