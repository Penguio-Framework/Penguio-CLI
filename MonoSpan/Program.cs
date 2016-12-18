using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Project = Microsoft.Build.BuildEngine.Project;

namespace MonoSpan
{
    class Program
    {
        private static string projectName;

        static void Main(string[] commands)
        {
            projectName = "DemolitionRobots";

            //commands = new[] { "platform", "add", "WindowsDesktop", @"C:\code\Multi\MonoSpanEngine" };
            //commands = new[] { "platform", "run", "WindowsDesktop" };
            var directory = Directory.GetCurrentDirectory();
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
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    var path = commands[3];
                                    addWindowsPlatform(directory, path);
                                    break;
                            }

                            break;
                        case "remove":
                        case "rm":
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    Directory.Delete(Path.Combine(directory, "platforms", "WindowsDesktop"),true);
                                    break;
                            }

                            break;

                        case "build":
                        case "b":
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    buildWindowsPlatform(directory);
                                    break;
                            }

                            break;

                        case "run":
                        case "r":
                            switch (commands[2].ToLower())
                            {
                                case "windowsdesktop":
                                case "wd":
                                    var build = buildWindowsPlatform(directory);
                                    runWindowsPlatform(build);
                                    break;
                            }

                            break;

                    }
                    break;
            }
        }


        private static void addWindowsPlatform(string directory, string path)
        {


            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                Directory.CreateDirectory(Path.Combine(directory, "platforms"));
            }

            if (Directory.Exists(Path.Combine(directory, "platforms", "WindowsDesktop")))
            {
                Console.WriteLine("Windows Desktop platform already exists");
                return;
            }
            var winDeskopPlatform = Path.Combine(directory, "platforms", "WindowsDesktop");
            Directory.CreateDirectory(winDeskopPlatform);
            var clientPath = Path.Combine(path, "Client.WindowsDesktop");

            File.Copy(Path.Combine(clientPath, "Program.cs"), Path.Combine(winDeskopPlatform, "Program.cs"));
            File.Copy(Path.Combine(clientPath, "WindowsUserPreferences.cs"), Path.Combine(winDeskopPlatform, "WindowsUserPreferences.cs"));
            File.Copy(Path.Combine(clientPath, "GameClient.cs"), Path.Combine(winDeskopPlatform, "GameClient.cs"));
            File.Copy(Path.Combine(clientPath, "Client.WindowsGame.csproj"), Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));
            File.Copy(Path.Combine(clientPath, "Client.WindowsGame.sln"), Path.Combine(winDeskopPlatform, "Client.WindowsGame.sln"));
            Directory.CreateDirectory(Path.Combine(winDeskopPlatform, "Properties/"));
            Directory.CreateDirectory(Path.Combine(winDeskopPlatform, "Content/"));
            File.Copy(Path.Combine(clientPath, "Properties/AssemblyInfo.cs"), Path.Combine(winDeskopPlatform, "Properties/AssemblyInfo.cs"));
            File.Copy(Path.Combine(clientPath, "Content/Content.mgcb"), Path.Combine(winDeskopPlatform, "Content/Content.mgcb"));
            File.Copy(Path.Combine(clientPath, "Icon.ico"), Path.Combine(winDeskopPlatform, "Icon.ico"));


            Directory.CreateDirectory(Path.Combine(winDeskopPlatform, "Engine"));
            Directory.CreateDirectory(Path.Combine(winDeskopPlatform, "Engine.Xna"));
            Directory.CreateDirectory(Path.Combine(winDeskopPlatform, "Game"));
            var engineFiles = DirectoryCopy(Path.Combine(winDeskopPlatform), Path.Combine(path, "Engine"), Path.Combine(winDeskopPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(DirectoryCopy(Path.Combine(winDeskopPlatform), Path.Combine(path, "Engine.Xna"), Path.Combine(winDeskopPlatform, "Engine.Xna"), true, "*.cs"));
            engineFiles.Add(Path.Combine("Game", "Game.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(winDeskopPlatform, "Game", "Game.cs"), contents.Replace("{{{projectName}}}", projectName));
            File.WriteAllText(Path.Combine(clientPath, "GameClient.cs"), File.ReadAllText(Path.Combine(clientPath, "GameClient.cs")).Replace("{{{projectName}}}", "new " + projectName + ".Game()"));

            engineFiles.Add("GameClient.cs");
            engineFiles.Add("Program.cs");
            engineFiles.Add(@"Properties\AssemblyInfo.cs");
            engineFiles.Add("WindowsUserPreferences.cs");

            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));
            foreach (BuildItemGroup projItemGroup in proj.ItemGroups)
            {
                if (projItemGroup.ToArray().Any(a => a.Name == "Compile"))
                {
                    foreach (var buildItem in projItemGroup.ToArray())
                    {
                        projItemGroup.RemoveItem(buildItem);
                    }
                    foreach (var engineFile in engineFiles)
                    {
                        projItemGroup.AddNewItem("Compile", engineFile);
                    }
                }


            }
            proj.Save(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));
        }
        private static BuildResult buildWindowsPlatform(string directory)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                throw new Exception("No Platforms");
            }

            if (!Directory.Exists(Path.Combine(directory, "platforms", "WindowsDesktop")))
            {
                throw new Exception("Windows Desktop platform does not exist");
            }
            var platformFolder = Path.Combine(directory, "platforms", "WindowsDesktop");
            var assetsFolder = Path.Combine(directory, "assets", "images");
            var platformAssetsFolder = Path.Combine(platformFolder, "Content", "images");
            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformContent = Path.Combine(platformFolder, "Content");
            var winDeskopPlatform = Path.Combine(directory, "platforms", "WindowsDesktop");
            var gameSrc = Path.Combine(directory, "src");

            //copy assets
            var names = DirectoryCopy(platformContent, assetsFolder, platformAssetsFolder, true);

            var contentFile = new List<string>();
            contentFile.Add("/platform:Windows");
            contentFile.Add("/profile:Reach");
            contentFile.Add("/compress:False");
            contentFile.Add("/importer:TextureImporter");
            contentFile.Add("/processor:TextureProcessor");
            contentFile.Add("/processorParam:ColorKeyColor=255,0,255,255");
            contentFile.Add("/processorParam:ColorKeyEnabled=True");
            contentFile.Add("/processorParam:GenerateMipmaps=False");
            contentFile.Add("/processorParam:PremultiplyAlpha=True");
            contentFile.Add("/processorParam:ResizeToPowerOfTwo=False");
            contentFile.Add("/processorParam:MakeSquare=False");
            contentFile.Add("/processorParam:TextureFormat=Color");
            foreach (var name in names)
            {
                contentFile.Add("/build:" + name);
            }
            File.WriteAllLines(Path.Combine(platformContent, "Content.mgcb"), contentFile);


            var gameFiles = DirectoryCopy(platformGameFolder, gameSrc, platformGameFolder, true, "*.cs");




            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));
            foreach (BuildItemGroup projItemGroup in proj.ItemGroups)
            {
                if (projItemGroup.ToArray().Any(a => a.Name == "Compile"))
                {
                    foreach (var buildItem in projItemGroup.ToArray())
                    {
                        if (buildItem.Include.IndexOf("Game\\") == 0)
                        {
                            projItemGroup.RemoveItem(buildItem);
                        }
                    }
                    foreach (var engineFile in gameFiles)
                    {
                        projItemGroup.AddNewItem("Compile", "Game\\" + engineFile);
                    }
                }


            }
            proj.Save(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU");
            var j = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), new BuildRequestData(new ProjectInstance(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj")), new string[] { "Rebuild" }));
            switch (j.OverallResult)
            {
                case BuildResultCode.Success:
                    Console.WriteLine("Build Succeeded");
                    break;
                case BuildResultCode.Failure:
                    Console.WriteLine(j);
                    throw new Exception("Build failed");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return j;
        }

        private static void runWindowsPlatform(BuildResult build)
        {
            var exe = build.ResultsByTarget["Rebuild"].Items.First().ItemSpec;
            System.Diagnostics.Process.Start(exe);
        }
        private static List<string> DirectoryCopy(string root, string sourceDirName, string destDirName, bool copySubDirs, string filter = null)
        {
            List<string> fileNames = new List<string>();
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = filter == null ? dir.GetFiles() : dir.GetFiles(filter);
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                fileNames.Add(temppath.Replace(root + "\\", ""));
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if (subdir.Name == "obj" || subdir.Name == "bin") continue;
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    fileNames.AddRange(DirectoryCopy(root, subdir.FullName, temppath, copySubDirs, filter));
                }
            }
            return fileNames;
        }
    }
}
