using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Microsoft.Build.Execution;
using Newtonsoft.Json;
using PenguioCLI.Platforms;

namespace PenguioCLI
{
    class Program
    {
        private static ProjectConfig getProject(string directory)
        {
            return JsonConvert.DeserializeObject<ProjectConfig>(File.ReadAllText(Path.Combine(directory, "config.json")));
        }

        [STAThread]
        static void Main(string[] commands)
        {

            try
            {
                /*
            commands = new[] { "generate", "font" };
*/
//                commands = new[] { "init", "OrbitalCrash" };

                //            commands = new[] { "generate", "assets" };

                //                                                commands = new[] {   "add", "web" };
                //            commands = new[] {   "rm", "web" };
                //                                                commands = new[] {  "run", "wd" };
                var directory = Directory.GetCurrentDirectory();
                /*
                            directory = @"C:\code\penguio\orbitalcrash\";
                */
//                directory = @"C:\code\penguio\";




                if (commands.Length == 0 || commands[0].ToLower() == "/h")
                {
                    Console.WriteLine("Penguio CLI");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("peng init GameName");
                    Console.WriteLine("peng generate font");
                    Console.WriteLine("peng add WindowsDesktop");
                    Console.WriteLine("peng rm Android");
                    Console.WriteLine("peng build Web");
                    Console.WriteLine("peng clean WindowsDesktop");
                    Console.WriteLine("peng debug Android");
                    Console.WriteLine("peng run iOS");
                    return;
                }
                switch (commands[0].ToLower())
                {
                    case "init":
                    case "i":
                        if (commands.Length == 1)
                        {
                            Console.WriteLine("Missing Game Name");
                            return;
                        }
                        InitProject.Build(directory, commands[1]);
                        return;
                    case "add":
                    case "a":
                        {
                            string path;

                            if (commands.Length == 2)
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
                                path = commands[2] == "local" ? @"C:\code\penguio\Penguio-Framework" : commands[2];

                            }
                            switch (commands[1].ToLower())
                            {

                                case "windowsdesktop":
                                case "windows":
                                case "wd":
                                    WindowsSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "web":
                                case "w":
                                    WebSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "android":
                                case "a":
                                    AndroidSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "ios":
                                case "i":
                                    IOSSetup.Add(directory, path, getProject(directory));
                                    return;
                            }

                        }

                        break;
                    case "generate":
                    case "g":

                        if (commands[1].ToLower() == "font")
                        {
                            FontGenerator.Generate(directory);
                        }
                        if (commands[1].ToLower() == "assets")
                        {
                            AssetGenerator.Generate(directory, getProject(directory).ProjectName);
                        }
                        return;
                    case "clean":
                    case "c":
                        {
                            string path;

                            if (commands.Length == 2)
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
                                path = commands[2] == "local" ? @"C:\code\penguio\Penguio-Framework" : commands[2];

                            }
                            switch (commands[1].ToLower())
                            {

                                case "windowsdesktop":
                                case "windows":
                                case "wd":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "WindowsDesktop")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "WindowsDesktop"), true);
                                    WindowsSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "web":
                                case "w":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "Web")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "Web"), true);
                                    WebSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "android":
                                case "a":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "Android")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "Android"), true);
                                    AndroidSetup.Add(directory, path, getProject(directory));
                                    return;
                                case "ios":
                                case "i":
                                    if (Directory.Exists(Path.Combine(directory, "platforms", "IOS")))
                                        Directory.Delete(Path.Combine(directory, "platforms", "IOS"), true);
                                    IOSSetup.Add(directory, path, getProject(directory));
                                    return;
                            }

                        }
                        return;
                    case "remove":
                    case "rm":
                        switch (commands[1].ToLower())
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
                            case "ios":
                            case "i":
                                if (Directory.Exists(Path.Combine(directory, "platforms", "IOS")))
                                    Directory.Delete(Path.Combine(directory, "platforms", "IOS"), true);
                                return;
                        }

                        break;


                    case "build":
                    case "b":
                        AssetGenerator.Generate(directory, getProject(directory).ProjectName);
                        switch (commands[1].ToLower())
                        {
                            case "windowsdesktop":
                            case "windows":
                            case "wd":
                                WindowsSetup.Build(directory);
                                return;
                            case "web":
                            case "w":
                                WebSetup.Build(directory);
                                return;
                            case "android":
                            case "a":
                                AndroidSetup.Build(directory);
                                return;
                            case "ios":
                            case "i":
                                IOSSetup.Build(directory, getProject(directory));
                                return;
                        }
                        break;
                    case "debug":
                    case "d":
                        AssetGenerator.Generate(directory, getProject(directory).ProjectName);
                        switch (commands[1].ToLower())
                        {
                            case "windowsdesktop":
                            case "windows":
                            case "wd":
                                WindowsSetup.Build(directory);
                                WindowsSetup.Debug(directory);
                                return;
                            case "web":
                            case "w":
                                WebSetup.Build(directory);
                                WebSetup.Debug(directory);
                                return;
                            case "android":
                            case "a":
                                AndroidSetup.Build(directory);
                                AndroidSetup.Debug(directory);
                                return;
                            case "ios":
                            case "i":
                                IOSSetup.Build(directory, getProject(directory));
                                IOSSetup.Debug(directory);
                                return;
                        }

                        break;

                    case "run":
                    case "r":
                        AssetGenerator.Generate(directory, getProject(directory).ProjectName);
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
                                build = WebSetup.Build(directory);
                                WebSetup.Run(directory, getProject(directory), build);
                                return;
                            case "android":
                            case "a":
                                build = AndroidSetup.Build(directory);
                                AndroidSetup.Run(directory, getProject(directory), build);
                                return;
                            case "ios":
                            case "i":
                                build = IOSSetup.Build(directory, getProject(directory));
                                IOSSetup.Run(directory, getProject(directory), build);
                                return;
                        }
                        break;
                }

                Console.WriteLine($"Command not understood: {string.Join(" ", commands)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


    }

    public class ProjectConfig
    {
        public string ProjectName { get; set; }
        public IosConfig Ios { get; set; }
    }

    public class IosConfig
    {
        public string ServerAddress { get; set; }
        public string ServerUser { get; set; }
        public string ServerPassword { get; set; }
    }
}


