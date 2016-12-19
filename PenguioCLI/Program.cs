using System;
using System.IO;
using Microsoft.Build.Execution;
using Newtonsoft.Json;

namespace PenguioCLI
{
    class Program
    {
        private static ProjectConfig project;

        static void Main(string[] commands)
        {
//            commands = new[] { "platform", "rm", "web"};
//            commands = new[] { "platform", "add", "web", @"C:\code\penguio\Penguio-Framework" };
//            commands = new[] { "platform", "rm", "web" };
//                        commands = new[] { "platform", "run", "web" };
            var directory = Directory.GetCurrentDirectory();
//            directory = @"C:\code\penguio\PenguinShuffle";

            project = JsonConvert.DeserializeObject<ProjectConfig>(File.ReadAllText(Path.Combine(directory, "config.json")));


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
                                    WindowsSetup.AddWindowsPlatform(directory, path, project);
                                    break;
                                case "web":
                                case "w":
                                    WebSetup.AddWebPlatform(directory, path, project);
                                    break;
                                case "android":
                                case "a":
                                    AndroidSetup.AddAndroidPlatform(directory, path, project);
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
                                    WebSetup.BuildWebPlatform(directory,project);
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
                                    build = WebSetup.BuildWebPlatform(directory, project);
                                    WebSetup.RunWebPlatform(directory, project, build);
                                    break;
                                case "android":
                                case "a":
                                    build = AndroidSetup.BuildAndroidPlatform(directory);
                                    AndroidSetup.RunAndroidPlatform(directory, project, build);
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }
    }

    public class ProjectConfig
    {
        public string ProjectName { get; set; }
    }
}
