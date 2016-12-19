using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI
{
    public class WebSetup
    {
        public static void AddWebPlatform(string directory, string path,string projectName)
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
            var clientPath = Path.Combine(path, "Client.Web");

            File.Copy(Path.Combine(clientPath, "Activity.cs"), Path.Combine(webPlatform, "Activity.cs"));
            File.Copy(Path.Combine(clientPath, "GameClient.cs"), Path.Combine(webPlatform, "GameClient.cs"));
            File.Copy(Path.Combine(clientPath, "WebUserPreferences.cs"), Path.Combine(webPlatform, "WebUserPreferences.cs"));
            
            File.Copy(Path.Combine(clientPath, "Client.WebGame.csproj"), Path.Combine(webPlatform, "Client.WebGame.csproj"));
            File.Copy(Path.Combine(clientPath, "Client.WebGame.sln"), Path.Combine(webPlatform, "Client.WebGame.sln"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Properties/"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Assets/"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Content/"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Resources/"));
            FileUtils.DirectoryCopy("none", Path.Combine(clientPath, "Resources"), Path.Combine(webPlatform, "Resources"), true);
            File.Copy(Path.Combine(clientPath, "Properties/AssemblyInfo.cs"), Path.Combine(webPlatform, "Properties/AssemblyInfo.cs"));
            File.Copy(Path.Combine(clientPath, "Properties/WebManifest.xml"), Path.Combine(webPlatform, "Properties/WebManifest.xml"));
            File.Copy(Path.Combine(clientPath, "Content/Content.mgcb"), Path.Combine(webPlatform, "Content/Content.mgcb"));


            Directory.CreateDirectory(Path.Combine(webPlatform, "Engine"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Engine.Xna"));
            Directory.CreateDirectory(Path.Combine(webPlatform, "Game"));
            var engineFiles = FileUtils.DirectoryCopy(Path.Combine(webPlatform), Path.Combine(path, "Engine"), Path.Combine(webPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(FileUtils.DirectoryCopy(Path.Combine(webPlatform), Path.Combine(path, "Engine.Xna"), Path.Combine(webPlatform, "Engine.Xna"), true, "*.cs"));
            engineFiles.Add(Path.Combine("Game", "Game.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(webPlatform, "Game", "Game.cs"), contents.Replace("{{{projectName}}}", projectName));
            File.WriteAllText(Path.Combine(webPlatform, "GameClient.cs"), File.ReadAllText(Path.Combine(webPlatform, "GameClient.cs")).Replace("{{{projectName}}}", "new " + projectName + ".Game()"));

            File.WriteAllText(Path.Combine(webPlatform, "Properties/WebManifest.xml"), File.ReadAllText(Path.Combine(webPlatform, "Properties/WebManifest.xml")).Replace("{{{projectName}}}", projectName));

            engineFiles.Add("Activity.cs");
            engineFiles.Add(@"Resources\Resource.Designer.cs");
            engineFiles.Add("GameClient.cs");
            engineFiles.Add("WebUserPreferences.cs");
            engineFiles.Add(@"Properties\AssemblyInfo.cs");

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

        public static BuildResult BuildWebPlatform(string directory)
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
            var assetsFolder = Path.Combine(directory, "assets", "images");
            var platformAssetsFolder = Path.Combine(platformFolder, "Content", "images");
            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformContent = Path.Combine(platformFolder, "Content");
            var webPlatform = Path.Combine(directory, "platforms", "Web");
            var gameSrc = Path.Combine(directory, "src");

            Directory.Delete(platformAssetsFolder,true);
            //copy assets
            var names = FileUtils.DirectoryCopy(platformContent, assetsFolder, platformAssetsFolder, true);

            var contentFile = new List<string>();
            contentFile.Add("/platform:Web");
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
            var buildRequestData = new BuildRequestData(new ProjectInstance(Path.Combine(webPlatform, "Client.WebGame.csproj")), new [] { "Rebuild"});
            var j = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequestData);
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

        public static void RunWebPlatform(string directory, string projectName,BuildResult build)
        {
//            var apk = Path.Combine(directory, @"platforms\Web\bin\Web\AnyCPU\Debug\com."+projectName+".game-Signed.apk");
            System.Diagnostics.Process.Start("adb", "shell am start -n com." + projectName + ".game/md5fe4548818b426bee4361f0bedb3504dc.MainActivity").WaitForExit();
        }
    }
}