﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI
{
    public class AndroidSetup
    {
        public static void AddAndroidPlatform(string directory, string path, ProjectConfig project)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                Directory.CreateDirectory(Path.Combine(directory, "platforms"));
            }

            if (Directory.Exists(Path.Combine(directory, "platforms", "Android")))
            {
                Console.WriteLine("Android platform already exists");
                return;
            }
            var androidPlatform = Path.Combine(directory, "platforms", "Android");
            Directory.CreateDirectory(androidPlatform);
            var clientPath = Path.Combine(path, "Client.Android");

            File.Copy(Path.Combine(clientPath, "Activity.cs"), Path.Combine(androidPlatform, "Activity.cs"));
            File.Copy(Path.Combine(clientPath, "GameClient.cs"), Path.Combine(androidPlatform, "GameClient.cs"));
            File.Copy(Path.Combine(clientPath, "AndroidUserPreferences.cs"), Path.Combine(androidPlatform, "AndroidUserPreferences.cs"));
            
            File.Copy(Path.Combine(clientPath, "Client.AndroidGame.csproj"), Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
            File.Copy(Path.Combine(clientPath, "Client.AndroidGame.sln"), Path.Combine(androidPlatform, "Client.AndroidGame.sln"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Properties/"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Assets/"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Content/"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Resources/"));
            FileUtils.DirectoryCopy("none", Path.Combine(clientPath, "Resources"), Path.Combine(androidPlatform, "Resources"), true);
            File.Copy(Path.Combine(clientPath, "Properties/AssemblyInfo.cs"), Path.Combine(androidPlatform, "Properties/AssemblyInfo.cs"));
            File.Copy(Path.Combine(clientPath, "Properties/AndroidManifest.xml"), Path.Combine(androidPlatform, "Properties/AndroidManifest.xml"));
            File.Copy(Path.Combine(clientPath, "Content/Content.mgcb"), Path.Combine(androidPlatform, "Content/Content.mgcb"));


            Directory.CreateDirectory(Path.Combine(androidPlatform, "Engine"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Engine.Xna"));
            Directory.CreateDirectory(Path.Combine(androidPlatform, "Game"));
            var engineFiles = FileUtils.DirectoryCopy(Path.Combine(androidPlatform), Path.Combine(path, "Engine"), Path.Combine(androidPlatform, "Engine"), true, "*.cs");
            engineFiles.AddRange(FileUtils.DirectoryCopy(Path.Combine(androidPlatform), Path.Combine(path, "Engine.Xna"), Path.Combine(androidPlatform, "Engine.Xna"), true, "*.cs"));
            engineFiles.Add(Path.Combine("Game", "Game.cs"));

            var contents = "using System;using Engine.Interfaces;namespace {{{projectName}}}{public class Game : IGame{public void InitScreens(IRenderer renderer, IScreenManager screenManager){throw new NotImplementedException();}public void LoadAssets(IRenderer renderer){throw new NotImplementedException();}public void BeforeTick(){throw new NotImplementedException();}public void AfterTick(){throw new NotImplementedException();}public void BeforeDraw(){throw new NotImplementedException();}public void AfterDraw(){throw new NotImplementedException();}public IClient Client { get; set; }public AssetManager AssetManager { get; set; }}}";
            File.WriteAllText(Path.Combine(androidPlatform, "Game", "Game.cs"), contents.Replace("{{{projectName}}}", project.ProjectName));
            File.WriteAllText(Path.Combine(androidPlatform, "GameClient.cs"), File.ReadAllText(Path.Combine(androidPlatform, "GameClient.cs")).Replace("{{{projectName}}}", "new " + project.ProjectName + ".Game()"));

            File.WriteAllText(Path.Combine(androidPlatform, "Properties/AndroidManifest.xml"), File.ReadAllText(Path.Combine(androidPlatform, "Properties/AndroidManifest.xml")).Replace("{{{projectName}}}", project.ProjectName));

            engineFiles.Add("Activity.cs");
            engineFiles.Add(@"Resources\Resource.Designer.cs");
            engineFiles.Add("GameClient.cs");
            engineFiles.Add("AndroidUserPreferences.cs");
            engineFiles.Add(@"Properties\AssemblyInfo.cs");

            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
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
            proj.Save(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
        }

        public static BuildResult BuildAndroidPlatform(string directory)
        {

            if (!Directory.Exists(Path.Combine(directory, "platforms")))
            {
                throw new Exception("No Platforms");
            }

            if (!Directory.Exists(Path.Combine(directory, "platforms", "Android")))
            {
                throw new Exception("Android platform does not exist");
            }
            var platformFolder = Path.Combine(directory, "platforms", "Android");
            var assetsFolder = Path.Combine(directory, "assets", "images");
            var platformAssetsFolder = Path.Combine(platformFolder, "Content", "images");
            var platformGameFolder = Path.Combine(platformFolder, "Game");

            var platformContent = Path.Combine(platformFolder, "Content");
            var androidPlatform = Path.Combine(directory, "platforms", "Android");
            var gameSrc = Path.Combine(directory, "src");

            if (Directory.Exists(platformAssetsFolder))
                Directory.Delete(platformAssetsFolder, true);

            if (Directory.Exists(platformGameFolder))
                Directory.Delete(platformGameFolder, true);
            
            //copy assets
            var names = FileUtils.DirectoryCopy(platformContent, assetsFolder, platformAssetsFolder, true);

            var contentFile = new List<string>();
            contentFile.Add("/platform:Android");
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
            proj.Load(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
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
            proj.Save(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU");
            var buildRequestData = new BuildRequestData(new ProjectInstance(Path.Combine(androidPlatform, "Client.AndroidGame.csproj")), new [] { "SignAndroidPackage" ,"Install"});
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

        public static void RunAndroidPlatform(string directory, ProjectConfig project,BuildResult build)
        {
//            var apk = Path.Combine(directory, @"platforms\Android\bin\Android\AnyCPU\Debug\com."+projectName+".game-Signed.apk");
            System.Diagnostics.Process.Start("adb", "shell am start -n com." + project.ProjectName + ".game/md5fe4548818b426bee4361f0bedb3504dc.MainActivity").WaitForExit();
        }
    }
}