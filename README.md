This is a fork of MonoKickstart. It contains an application that you can use to bundle a standalone Mono "kick" application to run C# programs on GNU/Linux and Mac OSX without depending on a system installation of Mono.

To use the program, simply run `AutoBundle.exe` and supply the path of the folder that your `.exe` file is in (relative to where you're running AutoBundle from) as the only argument. Multiple files will then be created, including a script that has the same name as your `.exe`, which is the file that can be run on Linux and Mac to start your application.

Note that, if you move `AutoBundle.exe`, the `precompiled` folder needs to be moved with it.