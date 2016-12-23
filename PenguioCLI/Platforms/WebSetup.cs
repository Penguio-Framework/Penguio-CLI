using System;
using System.IO;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI.Platforms
{
    public class WebSetup
    {
        public static void Add(string directory, string path, ProjectConfig project)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                Directory.CreateDirectory(Path.Combine(directory, "platforms"));
            }

            if (Directory.Exists(Path.Combine(directory, "platforms", "Web")))
            {
                Console.WriteLine("Web platform already exists");
                return;
            }
            var webPlatform = Path.Combine(directory, "platforms", "Web");
            Directory.CreateDirectory(webPlatform);
            Directory.CreateDirectory(webPlatform + "/Bridge");
            var clientPath = Path.Combine(path, "Client.Web");

            File.Copy(Path.Combine(clientPath, "Program.cs"), Path.Combine(webPlatform, "Program.cs"));
            File.Copy(Path.Combine(clientPath, "Bridge/bridge.json"), Path.Combine(webPlatform, "Bridge/bridge.json"));
            File.Copy(Path.Combine(clientPath, "WebUserPreferences.cs"), Path.Combine(webPlatform, "WebUserPreferences.cs"));

            File.Copy(Path.Combine(clientPath, "Client.WebGame.csproj"), Path.Combine(webPlatform, "Client.WebGame.csproj"));
            File.Copy(Path.Combine(clientPath, "Client.WebGame.sln"), Path.Combine(webPlatform, "Client.WebGame.sln"));

            Directory.CreateDirectory(Path.Combine(webPlatform, "Game"));
            var engineFiles = FileUtils.DirectoryCopy(Path.Combine(webPlatform), Path.Combine(path, "Engine"), Path.Combine(webPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(FileUtils.DirectoryCopy(Path.Combine(webPlatform), Path.Combine(path, "Engine.Web"), Path.Combine(webPlatform, "Engine.Web"), true, "*.cs"));

            FileUtils.DirectoryCopy(Path.Combine(clientPath), Path.Combine(clientPath, "dlls"), Path.Combine(webPlatform, "dlls"), true, "*.dll");
            FileUtils.DirectoryCopy(Path.Combine(clientPath), Path.Combine(clientPath, "packages"), Path.Combine(webPlatform, "packages"), true);

            engineFiles.Add(Path.Combine("Game", "Program.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(webPlatform, "Game", "Program.cs"), contents.Replace("{{{projectName}}}", project.ProjectName));
            File.WriteAllText(Path.Combine(webPlatform, "Program.cs"), File.ReadAllText(Path.Combine(webPlatform, "Program.cs")).Replace("{{{projectName}}}", "new " + project.ProjectName + ".Game()"));

            engineFiles.Add("WebUserPreferences.cs");
            engineFiles.Add(@"Program.cs");

            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(webPlatform, "Client.WebGame.csproj"));
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
                    break;
                }


            }
            proj.Save(Path.Combine(webPlatform, "Client.WebGame.csproj"));
        }

        public static BuildResult Build(string directory, ProjectConfig project)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                throw new Exception("No Platforms");
            }

            if (!Directory.Exists(Path.Combine(directory, "platforms", "Web")))
            {
                throw new Exception("Web platform does not exist");
            }
            var platformFolder = Path.Combine(directory, "platforms", "Web");
            var imagesFolder = Path.Combine(directory, "assets", "images");
            var fontsFolder = Path.Combine(directory, "assets", "fonts");
            var songsFolder = Path.Combine(directory, "assets", "songs");
            var soundsFolder = Path.Combine(directory, "assets", "sounds");

            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformOutput = Path.Combine(platformFolder, "bin", "output");
            var platformAssetsFolder = Path.Combine(platformOutput, "assets", "images");
            var platformFontsFolder = Path.Combine(platformOutput, "assets", "fonts");
            var platformSongsFolder = Path.Combine(platformOutput, "assets", "songs");
            var platformSoundsFolder = Path.Combine(platformOutput, "assets", "sounds");
            var webPlatform = Path.Combine(directory, "platforms", "Web");
            var gameSrc = Path.Combine(directory, "src");

            if (Directory.Exists(platformAssetsFolder))
                Directory.Delete(platformAssetsFolder, true);

            if (Directory.Exists(platformFontsFolder))
                Directory.Delete(platformFontsFolder, true);

            if (Directory.Exists(platformSongsFolder))
                Directory.Delete(platformSongsFolder, true);

            if (Directory.Exists(platformSoundsFolder))
                Directory.Delete(platformSoundsFolder, true);

            if (Directory.Exists(platformGameFolder))
                Directory.Delete(platformGameFolder, true);
            //copy assets

            FileUtils.DirectoryCopy(platformAssetsFolder, imagesFolder, platformAssetsFolder, true);
            FileUtils.DirectoryCopy(platformFontsFolder, fontsFolder, platformFontsFolder, true);
            FileUtils.DirectoryCopy(platformSongsFolder, songsFolder, platformSongsFolder, true);
            FileUtils.DirectoryCopy(platformSoundsFolder, soundsFolder, platformSoundsFolder, true);



            var gameFiles = FileUtils.DirectoryCopy(platformGameFolder, gameSrc, platformGameFolder, true, "*.cs");

            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(webPlatform, "Client.WebGame.csproj"));
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
                    break;
                }


            }
            proj.Save(Path.Combine(webPlatform, "Client.WebGame.csproj"));

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU");
            var buildRequestData = new BuildRequestData(new ProjectInstance(Path.Combine(webPlatform, "Client.WebGame.csproj")), new[] { "Build" });
            var j = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc)
            {
                Loggers = new ILogger[]
                {
                    new ConsoleLogger(LoggerVerbosity.Normal)
                }
            }, buildRequestData);
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

            var jsFiles = "";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/bridge.min.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/WebGame.min.js\" ></script>";

            File.WriteAllText(Path.Combine(platformOutput, "index.html"), "<!DOCTYPE html><html><head><style>body {padding: 50px;font: 14px \"Lucida Grande\", Helvetica, Arial, sans-serif;}* {margin: 0;padding: 0;}html, body {width: 100%;height: 100%;}canvas {display: block;margin: 0;position: absolute;top: 0;left: 0;z-index: 0;}.clickManager {display: block;margin: 0;position: absolute;top: 0;left: 0;z-index: 0;}</style></head><body>{{{js}}}</body></html>".Replace("{{{js}}}", jsFiles));

            return j;
        }

        public static void Run(string directory, ProjectConfig project, BuildResult build)
        {
            Console.WriteLine("Server running on 8018");
            new SimpleHttpServer(Path.Combine(directory, "platforms/Web/bin/output"), 8018);
        }

        public static void Debug(string directory)
        {
            var webPlatform = Path.Combine(directory, "platforms", "Web");
            System.Diagnostics.Process.Start(Path.Combine(webPlatform, "Client.WebGame.sln"));
        }
    }
}