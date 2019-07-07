using System;
using System.IO;
using System.Text;

namespace AutoBundle {
    internal static class Program {

        public static void Main(string[] args) {
            if (args.Length != 1) {
                Console.Error.WriteLine("No argument found. The path of the folder that contains your .exe file (relative to where this application is being run from) needs to be supplied. Aborting");
                return;
            }

            var currDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, args[0]));
            var exeDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            Console.WriteLine("Finding .exe in " + currDir + "...");
            var exeFiles = currDir.GetFiles("*.exe");
            if (exeFiles.Length <= 0) {
                Console.Error.WriteLine("Couldn't find .exe file, aborting");
                return;
            } else if (exeFiles.Length > 1) {
                Console.Error.WriteLine("More than one .exe file found, aborting");
                return;
            }

            var exe = exeFiles[0];
            var exeName = Path.GetFileNameWithoutExtension(exe.Name);

            Console.WriteLine("Found " + exe + ", copying /precompiled...");
            var precompiled = new DirectoryInfo(Path.Combine(exeDir.FullName, "precompiled"));
            if (!precompiled.Exists) {
                Console.Error.WriteLine("/precompiled folder seems to have been moved, aborting");
                return;
            }

            foreach (var file in precompiled.EnumerateFiles()) {
                if (file.Name == "Kick") {
                    Console.WriteLine("Modifying Kick script...");

                    var newScript = new StringBuilder();
                    using (var reader = new StreamReader(file.OpenRead())) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (line.Contains("kick")) {
                                var newLine = line.Replace("kick", exeName);
                                Console.WriteLine("Changing " + line.Trim() + " to " + newLine.Trim());
                                line = newLine;
                            }
                            newScript.Append(line).Append("\n");
                        }
                    }

                    Console.WriteLine("Done modifying Kick script, saving as " + exeName);
                    var newKick = new FileInfo(Path.Combine(currDir.FullName, exeName));
                    if (newKick.Exists)
                        newKick.Delete();
                    using (var stream = newKick.CreateText()) {
                        stream.Write(newScript);
                    }
                } else if (file.Name.Contains("kick")) {
                    var newName = file.Name.Replace("kick", exeName);
                    Console.WriteLine("Changing file name " + file.Name + " to " + newName);
                    file.CopyTo(Path.Combine(currDir.FullName, newName), true);
                } else {
                    file.CopyTo(Path.Combine(currDir.FullName, file.Name), true);
                }
            }

            Console.WriteLine("Done");
        }

    }
}