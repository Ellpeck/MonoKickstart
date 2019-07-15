# MonoKickstart with AutoBundle

This is a fork of [MonoKickstart](https://github.com/MonoGame/MonoKickstart). It contains an application that you can use to bundle a standalone Mono "kick" application to run C# programs on GNU/Linux and Mac OSX without depending on a system installation of Mono.

# How to Use AutoBundle
AutoBundle is a small program by Ellpeck that quickly and easily bundles the `/precompiled` folder (which is a lightweight, standalone version of Mono) with your application, allowing it to run on Mac and Linux. AutoBundle creates various files, including a bash script named after your application that can be executed on Mac and Linux to start it.

To use AutoBundle, simply execute `AutoBundle.exe` with a single argument: The path of the folder that contains your application's `.exe` file, relative to where AutoBundle is being called from.

If you want to include AutoBundle in your build process, you can do so by adding this to your `.csproj` file and changing the `relative\path\to\MonoKickstart` to point to your clone of this repository:
```xml
<Target Name="AfterBuild" Condition="'$(Configuration)' == 'Release'">
    <Exec Command="relative\path\to\MonoKickstart\AutoBundle.exe bin\Release" />
</Target>
```

*Note that, when moving `AutoBundle.exe`, the `/precompiled` folder also needs to be moved.*