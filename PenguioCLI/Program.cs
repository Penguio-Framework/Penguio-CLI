using System;
using System.IO;
using LibGit2Sharp;
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
//                                                commands = new[] { "platform", "add", "web" };
            //            commands = new[] { "platform", "rm", "web" };
//                                                commands = new[] {  "run", "web" };
            var directory = Directory.GetCurrentDirectory();
//                                                directory = @"C:\code\penguio\bingoblockparty";

            project = JsonConvert.DeserializeObject<ProjectConfig>(File.ReadAllText(Path.Combine(directory, "config.json")));


            if (commands.Length == 0 || commands[0].ToLower() == "/h")
            {
                Console.WriteLine("Penguio CLI");
                Console.WriteLine("Usage:");
                Console.WriteLine("peng platform add WindowsDesktop");
                Console.WriteLine("peng platform rm Android");
                Console.WriteLine("peng build Web");
                Console.WriteLine("peng debug Android");
                Console.WriteLine("peng run iOS");
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
                            string path;
                            if (commands.Length == 3)
                            {
                                var workdirPath = Path.GetTempPath();
                                var framework = Path.Combine(workdirPath, "penguio-framework", Guid.NewGuid().ToString());
                                if (Directory.Exists(framework))
                                    Directory.Delete(framework, true);
                                Directory.CreateDirectory(framework);
                                Repository.Clone("https://github.com/Penguio-Framework/Penguio-Framework.git", framework);
                                path = framework;
                            }
                            else
                            {
                                path = commands[3];

                            }
                            switch (commands[2].ToLower())
                            {

                                case "windowsdesktop":
                                case "windows":
                                case "wd":
                                    WindowsSetup.Add(directory, path, project);
                                    return;
                                case "web":
                                case "w":
                                    WebSetup.Add(directory, path, project);
                                    return;
                                case "android":
                                case "a":
                                    AndroidSetup.Add(directory, path, project);
                                    return;
                            }

                            break;
                        case "remove":
                        case "rm":
                            switch (commands[2].ToLower())
                            {

                                case "windowsdesktop":
                                case "windows":
                                case "wd":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "WindowsDesktop")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "WindowsDesktop"), true);
                                    return;
                                case "web":
                                case "w":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "Web")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "Web"), true);
                                    return;
                                case "android":
                                case "a":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "Android")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "Android"), true);
                                    return;
                            }

                            break;

                    }
                    break;

                case "build":
                case "b":
                    switch (commands[1].ToLower())
                    {
                        case "windowsdesktop":
                        case "windows":
                        case "wd":
                            WindowsSetup.Build(directory);
                            return;
                        case "web":
                        case "w":
                            WebSetup.Build(directory, project);
                            return;
                        case "android":
                        case "a":
                            AndroidSetup.Build(directory);
                            return;
                    }
                    break;
                case "debug":
                case "d":
                    switch (commands[1].ToLower())
                    {
                        case "windowsdesktop":
                        case "windows":
                        case "wd":
                            WindowsSetup.Debug(directory);
                            return;
                        case "web":
                        case "w":
                            WebSetup.Debug(directory);
                            return;
                        case "android":
                        case "a":
                            AndroidSetup.Debug(directory);
                            return;
                    }

                    break;

                case "run":
                case "r":
                    BuildResult build;
                    switch (commands[1].ToLower())
                    {
                        case "windowsdesktop":
                        case "windows":
                        case "wd":
                            build = WindowsSetup.Build(directory);
                            WindowsSetup.Run(build);
                            return;

                        case "web":
                        case "w":
                            build = WebSetup.Build(directory, project);
                            WebSetup.Run(directory, project, build);
                            return;
                        case "android":
                        case "a":
                            build = AndroidSetup.Build(directory);
                            AndroidSetup.Run(directory, project, build);
                            return;
                    }
                    break;
            }

            Console.WriteLine($"Command not understood: {string.Join(" ", commands)}");

        }
    }

    public class ProjectConfig
    {
        public string ProjectName { get; set; }
    }
}
