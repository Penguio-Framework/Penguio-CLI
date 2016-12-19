using System;
using System.IO;
using Microsoft.Build.Execution;

namespace PenguioCLI
{
    class Program
    {
        private static string projectName;

        static void Main(string[] commands)
        {
            projectName = "DemolitionRobots";

            //                        commands = new[] { "platform", "add", "a", @"C:\code\Multi\MonoSpanEngine" };
            //            commands = new[] { "platform", "run", "android" };
            var directory = Directory.GetCurrentDirectory();
            //            directory = @"C:\code\Multi\NewDemolitionRobots";
            if (commands.Length == 0 || commands[0].ToLower() == " / h")
            {
                Console.WriteLine("help");
                return;
            }
            switch (commands[0].ToLower())
            {
                case "platform":
                case "p":
                case "platforms":
                    switch (commands[1].ToLower())
                    {
                        case "add":
                        case "a":
                            var path = commands[3];
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    WindowsSetup.AddWindowsPlatform(directory, path, projectName);
                                    break;
                                case "web":
                                case "w":
                                    WebSetup.AddWebPlatform(directory, path, projectName);
                                    break;
                                case "android":
                                case "a":
                                    AndroidSetup.AddAndroidPlatform(directory, path, projectName);
                                    break;
                            }

                            break;
                        case "remove":
                        case "rm":
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    Directory.Delete(Path.Combine(directory, "platforms", "WindowsDesktop"), true);
                                    break;
                                case "web":
                                case "w":
                                    Directory.Delete(Path.Combine(directory, "platforms", "Web"), true);
                                    break;
                                case "android":
                                case "a":
                                    Directory.Delete(Path.Combine(directory, "platforms", "Android"), true);
                                    break;
                            }

                            break;

                        case "build":
                        case "b":
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    WindowsSetup.BuildWindowsPlatform(directory);
                                    break;
                                case "web":
                                case "w":
                                    WebSetup.BuildWebPlatform(directory);
                                    break;
                                case "android":
                                case "a":
                                    AndroidSetup.BuildAndroidPlatform(directory);
                                    break;
                            }

                            break;

                        case "run":
                        case "r":
                            BuildResult build;
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    build = WindowsSetup.BuildWindowsPlatform(directory);
                                    WindowsSetup.RunWindowsPlatform(build);
                                    break;

                                case "web":
                                case "w":
                                    build = WebSetup.BuildWebPlatform(directory);
                                    WebSetup.RunWebPlatform(directory, projectName, build);
                                    break;
                                case "android":
                                case "a":
                                    build = AndroidSetup.BuildAndroidPlatform(directory);
                                    AndroidSetup.RunAndroidPlatform(directory, projectName, build);
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }
    }
}
