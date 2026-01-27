namespace Enterspeed.Query.Sdk.Tests;

using System.IO;
using System.Runtime.CompilerServices;
using VerifyTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyXunit.Verifier.DerivePathInfo(
            (sourceFile, projectDirectory, type, method) => new PathInfo(
                directory: Path.Combine(projectDirectory, "__snapshots__"),
                typeName: type.Name,
                methodName: method.Name));
    }
}
