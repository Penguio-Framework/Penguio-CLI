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
    public class AndroidSetup
    {
        public static void Add(string directory, string path, ProjectConfig project)
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
                    var item = projItemGroup.AddNewItem("Compile", "..\\..\\src\\**\\*.cs");
                    item.SetMetadata("Link", "Game\\%(RecursiveDir)%(Filename)%(Extension)");
                    item.SetMetadata("CopyToOutputDirectory", "PreserveNewest");
                    break;
                }


            }
            proj.Save(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
        }

        public static BuildResult Build(string directory)
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
            var androidPlatform = Path.Combine(directory, "platforms", "Android");

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


            Engine eng = new Engine();
            Project proj = new Project(eng);
            proj.Load(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));
            foreach (BuildItemGroup projItemGroup in proj.ItemGroups)
            {
                if (projItemGroup.ToArray().Any(a => a.Name == "AndroidAsset"))
                {
                    foreach (var buildItem in projItemGroup.ToArray())
                    {
                    
                        if (buildItem.Include.IndexOf("Assets\\") == 0)
                        {
                            projItemGroup.RemoveItem(buildItem);
                        }

                    }
                    foreach (var engineFile in xmlFontFiles)
                    {
                        var fontContent = projItemGroup.AddNewItem("AndroidAsset", "Assets\\" + engineFile); 
                    }
                }
            }
            proj.Save(Path.Combine(androidPlatform, "Client.AndroidGame.csproj"));

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.SetGlobalProperty("Platform", "Any CPU"); 
            var buildRequestData = new BuildRequestData(new ProjectInstance(Path.Combine(androidPlatform, "Client.AndroidGame.csproj")), new[] { "SignAndroidPackage", "Install" });

            var j = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc)
            {
                Loggers = new ILogger[]
                {
                    new ConsoleLogger(LoggerVerbosity.Detailed)
                },
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
            //            var apk = Path.Combine(directory, @"platforms\Android\bin\Android\AnyCPU\Debug\com."+projectName+".game-Signed.apk");
            System.Diagnostics.Process.Start("adb", "shell am start -n com." + project.ProjectName + ".game/md5fe4548818b426bee4361f0bedb3504dc.MainActivity").WaitForExit();
        }
        public static void Debug(string directory)
        {
            var androidPlatform = Path.Combine(directory, "platforms", "Android");
            System.Diagnostics.Process.Start(Path.Combine(androidPlatform, "Client.AndroidGame.sln"));
        }

    }
}