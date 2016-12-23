using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI.Platforms
{
    public class IOSSetup
    {
        public static void Add(string directory, string path, ProjectConfig project)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                Directory.CreateDirectory(Path.Combine(directory, "platforms"));
            }

            if (Directory.Exists(Path.Combine(directory, "platforms", "IOS")))
            {
                Console.WriteLine("IOS platform already exists");
                return;
            }
            var iosPlatform = Path.Combine(directory, "platforms", "IOS");
            Directory.CreateDirectory(iosPlatform);
            var clientPath = Path.Combine(path, "Client.IOS");

            File.Copy(Path.Combine(clientPath, "Program.cs"), Path.Combine(iosPlatform, "Program.cs"));
            File.Copy(Path.Combine(clientPath, "GameClient.cs"), Path.Combine(iosPlatform, "GameClient.cs"));
            File.Copy(Path.Combine(clientPath, "Info.plist"), Path.Combine(iosPlatform, "Info.plist"));
            File.Copy(Path.Combine(clientPath, "GameThumbnail.png"), Path.Combine(iosPlatform, "GameThumbnail.png"));
            File.Copy(Path.Combine(clientPath, "Entitlements.plist"), Path.Combine(iosPlatform, "Entitlements.plist"));
            File.Copy(Path.Combine(clientPath, "Default.png"), Path.Combine(iosPlatform, "Default.png"));

            File.Copy(Path.Combine(clientPath, "Client.IOSGame.csproj"), Path.Combine(iosPlatform, "Client.IOSGame.csproj"));
            File.Copy(Path.Combine(clientPath, "Client.IOSGame.sln"), Path.Combine(iosPlatform, "Client.IOSGame.sln"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Properties/"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Assets/"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Content/"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Resources/"));
            File.Copy(Path.Combine(clientPath, "Properties/AssemblyInfo.cs"), Path.Combine(iosPlatform, "Properties/AssemblyInfo.cs"));
            File.Copy(Path.Combine(clientPath, "Content/Content.mgcb"), Path.Combine(iosPlatform, "Content/Content.mgcb"));

            FileUtils.DirectoryCopy("none", Path.Combine(clientPath, "Resources"), Path.Combine(iosPlatform, "Resources"), true);

            Directory.CreateDirectory(Path.Combine(iosPlatform, "Engine"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Engine.Xna"));
            Directory.CreateDirectory(Path.Combine(iosPlatform, "Game"));

            var engineFiles = FileUtils.DirectoryCopy(Path.Combine(iosPlatform), Path.Combine(path, "Engine"), Path.Combine(iosPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(FileUtils.DirectoryCopy(Path.Combine(iosPlatform), Path.Combine(path, "Engine.Xna"), Path.Combine(iosPlatform, "Engine.Xna"), true, "*.cs"));
            engineFiles.Add(Path.Combine("Game", "Game.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(iosPlatform, "Game", "Game.cs"), contents.Replace("{{{projectName}}}", project.ProjectName));
            File.WriteAllText(Path.Combine(iosPlatform, "GameClient.cs"), File.ReadAllText(Path.Combine(iosPlatform, "GameClient.cs")).Replace("{{{projectName}}}", "new " + project.ProjectName + ".Game()"));

            engineFiles.Add("Program.cs");
            engineFiles.Add("GameClient.cs");
            engineFiles.Add(@"Properties\AssemblyInfo.cs");

            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(iosPlatform, "Client.IOSGame.csproj"));
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
            proj.Save(Path.Combine(iosPlatform, "Client.IOSGame.csproj"));
        }

        public static BuildResult Build(string directory, ProjectConfig config)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                throw new Exception("No Platforms");
            }

            if (!Directory.Exists(Path.Combine(directory, "platforms", "IOS")))
            {
                throw new Exception("IOS platform does not exist");
            }
            var platformFolder = Path.Combine(directory, "platforms", "IOS");
            var imagesFolder = Path.Combine(directory, "assets", "images");
            var fontsFolder = Path.Combine(directory, "assets", "fonts");
            var songsFolder = Path.Combine(directory, "assets", "songs");
            var soundsFolder = Path.Combine(directory, "assets", "sounds");
            var platformAssetsFolder = Path.Combine(platformFolder, "Content", "images");
            var platformFontsFolder = Path.Combine(platformFolder, "Content", "fonts");
            var platformFontsAssetsFolder = Path.Combine(platformFolder, "Assets", "fonts");
            var platformSongsFolder = Path.Combine(platformFolder, "Content", "songs");
            var platformSoundsFolder = Path.Combine(platformFolder, "Content", "sounds");
            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformContent = Path.Combine(platformFolder, "Content");
            var iosPlatform = Path.Combine(directory, "platforms", "IOS");
            var gameSrc = Path.Combine(directory, "src");

            if (Directory.Exists(platformAssetsFolder))
                Directory.Delete(platformAssetsFolder, true);

            if (Directory.Exists(platformFontsFolder))
                Directory.Delete(platformFontsFolder, true);

            if (Directory.Exists(platformFontsAssetsFolder))
                Directory.Delete(platformFontsAssetsFolder, true);

            if (Directory.Exists(platformSongsFolder))
                Directory.Delete(platformSongsFolder, true);

            if (Directory.Exists(platformSoundsFolder))
                Directory.Delete(platformSoundsFolder, true);

            if (Directory.Exists(platformGameFolder))
                Directory.Delete(platformGameFolder, true);

            //copy assets
            var names = FileUtils.DirectoryCopy(platformContent, imagesFolder, platformAssetsFolder, true);
            var fontFiles = FileUtils.DirectoryCopy(platformContent, fontsFolder, platformFontsFolder, true);
            FileUtils.DirectoryCopy(platformContent, fontsFolder, platformFontsAssetsFolder, true);
            var songFiles = FileUtils.DirectoryCopy(platformContent, songsFolder, platformSongsFolder, true);
            var soundsFiles = FileUtils.DirectoryCopy(platformContent, soundsFolder, platformSoundsFolder, true);

            var xmlFontFiles = fontFiles.Where(a => a.EndsWith(".xml"));

            names.AddRange(fontFiles.Where(a => a.EndsWith(".png")));
            var contentFile = new List<string>();
            contentFile.Add("/platform:iOS");
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
            proj.Load(Path.Combine(iosPlatform, "Client.IOSGame.csproj"));
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
                if (projItemGroup.ToArray().Any(a => a.Name == "BundleResource"))
                {
                    foreach (var buildItem in projItemGroup.ToArray())
                    {

                        if (buildItem.Include.IndexOf("Assets\\") == 0 || buildItem.Include.IndexOf("Content\\") == 0)
                        {
                            projItemGroup.RemoveItem(buildItem);
                        }

                    }
                    foreach (var engineFile in xmlFontFiles)
                    {
                        projItemGroup.AddNewItem("Content", "Assets\\" + engineFile);
                    }
                    foreach (var name in names)
                    {
                        projItemGroup.AddNewItem("BundleResource", "Content\\" + name);
                    }
                    foreach (var name in soundsFiles)
                    {
                        projItemGroup.AddNewItem("Content", "Content\\" + name);
                    }
                    foreach (var name in songFiles)
                    {
                        projItemGroup.AddNewItem("Content", "Content\\" + name);
                    }
                }
            }
            proj.Save(Path.Combine(iosPlatform, "Client.IOSGame.csproj"));


            return build(Path.Combine(iosPlatform, "Client.IOSGame.csproj"), config);
        }

        private static BuildResult build(string csproj, ProjectConfig config)
        {
            var pc = new ProjectCollection();
            var projectInstance = new ProjectInstance(csproj,
                new Dictionary<string, string>()
                {
                    {"Configuration", "Ad-Hoc"},
                    {"Platform", "iPhone"},
                    {"ServerAddress", config.Ios.ServerAddress},
                    {"ServerUser", config.Ios.ServerUser},
                    {"ServerPassword", config.Ios.ServerPassword}

                },
                null);

            var buildRequestData = new BuildRequestData(projectInstance, new[] { "Build" });
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
            return j;
        }

        public static void Run(string directory, ProjectConfig project, BuildResult build)
        {
            var iosPlatform = Path.Combine(directory, "platforms", "IOS");
            System.Diagnostics.Process.Start(Path.Combine(iosPlatform, "Client.IOSGame.sln"));
        }


        public static void Debug(string directory)
        {
            var iosPlatform = Path.Combine(directory, "platforms", "IOS");
            System.Diagnostics.Process.Start(Path.Combine(iosPlatform, "Client.IOSGame.sln"));
        }

    }
}
