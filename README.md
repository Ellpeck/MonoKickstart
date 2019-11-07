# MonoKickstart with AutoBundle

This is a fork of [MonoKickstart](https://github.com/MonoGame/MonoKickstart). It contains an application that you can use to bundle a standalone Mono "kick" application to run C# programs on GNU/Linux and Mac OSX without depending on a system installation of Mono.

# How to Use AutoBundle
AutoBundle is a small program by Ellpeck that quickly and easily bundles the `/precompiled` folder (which is a lightweight, standalone version of Mono) with your application, allowing it to run on Mac and Linux. AutoBundle creates various files, including a bash script named after your application that can be executed on Mac and Linux to start it.

To include AutoBundle in your build process, you first have to add it to your project, either through your NuGet package manager or by adding it to your `.csproj` file as follows. Keep in mind to update the `Version` to the most recent one. You can find the package on the [NuGet website](https://www.nuget.org/packages/AutoBundle) as well.
```xml
<ItemGroup>
    <PackageReference Include="AutoBundle" Version="VERSION" />
</ItemGroup>
```
When your project configuration is set to `Release`, AutoBundle will now automatically locate your program's `.exe` file in the output directory and copy the necessary files to it.