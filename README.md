# Interop.DbgEng.dll

A .Net interop library to use the [Microsoft Debugger Engine](https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/debugger-engine-overview).

This library is created by translating the `dbgeng.h` to C# code that utilizes [Source Generated COM interop](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/comwrappers-source-generation).

## Get started

1. Add a package reference to [DumbPrograms.DbgEng](https://www.nuget.org/packages/DumbPrograms.DbgEng) in your project.
1. Get an `IDebugClient` from the static `Interop.DbgEng.IDebugClient.Create()` method.
1. Voila!

## Build

Open the `DbgEngIdl.slnx` file in Visual Studio.
