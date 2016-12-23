using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI.Platforms
{
    public class WindowsSetup
    {
        public static void Add(string directory, string path, ProjectConfig project)
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
            var engineFiles = FileUtils.DirectoryCopy(Path.Combine(winDeskopPlatform), Path.Combine(path, "Engine"), Path.Combine(winDeskopPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(FileUtils.DirectoryCopy(Path.Combine(winDeskopPlatform), Path.Combine(path, "Engine.Xna"), Path.Combine(winDeskopPlatform, "Engine.Xna"), true, "*.cs"));
            engineFiles.Add(Path.Combine("Game", "Game.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(winDeskopPlatform, "Game", "Game.cs"), contents.Replace("{{{projectName}}}", project.ProjectName));
            File.WriteAllText(Path.Combine(winDeskopPlatform, "GameClient.cs"), File.ReadAllText(Path.Combine(winDeskopPlatform, "GameClient.cs")).Replace("{{{projectName}}}", "new " + project.ProjectName + ".Game()"));

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
                    break;
                }


            }
            proj.Save(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));
        }

        public static BuildResult Build(string directory)
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
            var imagesFolder = Path.Combine(directory, "assets", "images");
            var fontsFolder = Path.Combine(directory, "assets", "fonts");
            var songsFolder = Path.Combine(directory, "assets", "songs");
            var soundsFolder = Path.Combine(directory, "assets", "sounds");
            var platformAssetsFolder = Path.Combine(platformFolder, "Content", "images");
            var platformFontsFolder = Path.Combine(platformFolder, "Content", "fonts");
            var platformSongsFolder = Path.Combine(platformFolder, "Content", "songs");
            var platformSoundsFolder = Path.Combine(platformFolder, "Content", "sounds");

            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformContent = Path.Combine(platformFolder, "Content");
            var winDeskopPlatform = Path.Combine(directory, "platforms", "WindowsDesktop");
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
            var names = FileUtils.DirectoryCopy(platformContent, imagesFolder, platformAssetsFolder, true);
            var fontFiles = FileUtils.DirectoryCopy(platformContent, fontsFolder, platformFontsFolder, true);
            var songFiles = FileUtils.DirectoryCopy(platformContent, songsFolder, platformSongsFolder, true);
            var soundsFiles = FileUtils.DirectoryCopy(platformContent, soundsFolder, platformSoundsFolder, true);

            var xmlFontFiles = fontFiles.Where(a => a.EndsWith(".xml"));

            names.AddRange(fontFiles.Where(a => a.EndsWith(".png")));

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


            contentFile.Add("/importer:Mp3Importer");
            contentFile.Add("/processor:SongProcessor");
            contentFile.Add("/processorParam:Quality=Best");
            foreach (var name in songFiles)
            {
                contentFile.Add("/build:" + name);
            }

            contentFile.Add("/importer:WavImporter");
            contentFile.Add("/processor:SoundEffectProcessor");
            contentFile.Add("/processorParam:Quality=Best");
            foreach (var name in soundsFiles)
            {
                contentFile.Add("/build:" + name);
            }


            File.WriteAllLines(Path.Combine(platformContent, "Content.mgcb"), contentFile);


            var gameFiles = FileUtils.DirectoryCopy(platformGameFolder, gameSrc, platformGameFolder, true, "*.cs");




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
                if (projItemGroup.ToArray().Any(a => a.Name == "Content"))
                {
                    foreach (var buildItem in projItemGroup.ToArray())
                    {
                        if (buildItem.Include.IndexOf("Content\\") == 0)
                        {
                            projItemGroup.RemoveItem(buildItem);
                        }

                    }
                    foreach (var engineFile in xmlFontFiles)
                    {
                        var fontContent = projItemGroup.AddNewItem("Content", "Content\\" + engineFile);
                        fontContent.SetMetadata("CopyToOutputDirectory", "Always");
                    }
                }


            }
            proj.Save(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj"));

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU");

            var j = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc)
            {
                Loggers = new ILogger[]
                {
                    new ConsoleLogger(LoggerVerbosity.Normal)
                }
            }, new BuildRequestData(new ProjectInstance(Path.Combine(winDeskopPlatform, "Client.WindowsGame.csproj")), new string[] { "Build" }));
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

        public static void Run(BuildResult build)
        {
            var exe = build.ResultsByTarget["Build"].Items.First().ItemSpec;
            Directory.SetCurrentDirectory(exe.Replace("Client.WindowsGame.exe", ""));
            System.Diagnostics.Process.Start(exe);
        }

        public static void Debug(string directory)
        {
            var winDeskopPlatform = Path.Combine(directory, "platforms", "WindowsDesktop");
            System.Diagnostics.Process.Start(Path.Combine(winDeskopPlatform, "Client.WindowsGame.sln"));
        }
    }
}