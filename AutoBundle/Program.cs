using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AutoBundle {
    internal static class Program {

        public static void Main(string[] args) {
            if (args.Length < 1) {
                Console.Error.WriteLine("No argument found. The path of the folder that contains your .exe file (relative to where this application is being run from) needs to be supplied. Aborting");
                return;
            }
            var useLib = ParseArgument(args, "useLib").Contains("true");
            var libExceptions = ParseArgument(args, "libExceptions");

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

            if (useLib)
                EditAppConfig(exe);

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

            if (useLib) {
                Console.WriteLine("Moving libraries to Lib folder...");
                var lib = currDir.CreateSubdirectory("Lib");
                foreach (var file in currDir.EnumerateFiles()) {
                    if (file.Name.Contains(exeName))
                        continue;
                    // Skip files that mono needs in the main folder
                    if (file.Name == "netstandard.dll" || file.Name == "mscorlib.dll" || Regex.IsMatch(file.Name, "mono.*config")) {
                        Console.WriteLine($"Ignoring {file.Name} since mono needs it to start");
                        continue;
                    }
                    if (libExceptions.Contains(file.Name)) {
                        Console.WriteLine($"Ignoring {file.Name}");
                        continue;
                    }

                    var newFile = Path.Combine(lib.FullName, file.Name);
                    if (File.Exists(newFile))
                        File.Delete(newFile);
                    file.MoveTo(newFile);
                }
                foreach (var dir in currDir.EnumerateDirectories()) {
                    if (dir.Name == "Lib" || dir.Name == "Content")
                        continue;

                    var newDir = Path.Combine(lib.FullName, dir.Name);
                    if (Directory.Exists(newDir))
                        Directory.Delete(newDir, true);
                    dir.MoveTo(newDir);
                }
            }

            Console.WriteLine("Done");
        }

        private static void EditAppConfig(FileInfo exe) {
            var configLocation = exe.FullName + ".config";
            try {
                var config = new XmlDocument();
                config.Load(configLocation);

                var runtime = config.SelectSingleNode("/configuration/runtime");
                foreach (XmlNode binding in runtime.ChildNodes) {
                    foreach (XmlNode child in binding.ChildNodes) {
                        if (child.Name == "probing" && child.Attributes["privatePath"]?.Value == "Lib") {
                            Console.WriteLine("Didn't modify exe config since it already contains Lib redirect");
                            return;
                        }
                    }
                }

                var newChild = config.CreateElement("assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
                var probing = config.CreateElement("probing");
                probing.SetAttribute("privatePath", "Lib");
                newChild.AppendChild(probing);
                runtime.PrependChild(newChild);

                config.Save(configLocation);
                Console.WriteLine("Modified exe config to add Lib redirect");
            } catch (Exception e) {
                Console.WriteLine($"Couldn't load exe config file, expected {configLocation}: {e}");
            }
        }

        private static List<string> ParseArgument(string[] args, string name) {
            var results = new List<string>();
            var found = false;
            foreach (var arg in args) {
                if (found) {
                    if (arg.StartsWith("--"))
                        break;
                    results.Add(arg);
                } else {
                    if (arg != $"--{name}")
                        continue;
                    found = true;
                }
            }
            return results;
        }

    }
}