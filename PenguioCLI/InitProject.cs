using System;
using System.IO;

namespace PenguioCLI
{
    public class InitProject
    {
        public static void Build(string directory, string gameName)
        {
            var gamePath = Path.Combine(directory, gameName);
            if (Directory.Exists(gamePath))
            {
                Console.WriteLine("Game directory already exists.");
                return;
            }
            Directory.CreateDirectory(gamePath);

            Directory.CreateDirectory(Path.Combine(gamePath, "assets"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "fonts"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "icons"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "images"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "songs"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "sounds"));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "splashScreens"));

            File.WriteAllText(Path.Combine(gamePath, "assets", "fonts", "fonts.json"), "{'fonts': []}");
            File.WriteAllText(Path.Combine(gamePath, "assets", "icons", "add-content"), "");
            File.WriteAllText(Path.Combine(gamePath, "assets", "images", "add-content"), "");
            File.WriteAllText(Path.Combine(gamePath, "assets", "songs", "add-content"), "");
            File.WriteAllText(Path.Combine(gamePath, "assets", "sounds", "add-content"), "");
            File.WriteAllText(Path.Combine(gamePath, "assets", "splashScreens", "add-content"), "");
            File.WriteAllText(Path.Combine(gamePath, ".gitignore"), GitIngoreContent);
            File.WriteAllText(Path.Combine(gamePath, "readme.md"), gameName + "\r\n======");
            File.WriteAllText(Path.Combine(gamePath, "config.json"), @"{'projectName': '" + gameName + @"','ios': {'serverAddress': '','serverUser': '','serverPassword': ''}}");

            Directory.CreateDirectory(Path.Combine(gamePath, "src"));

            File.WriteAllText(Path.Combine(gamePath, "src", gameName + ".csproj"), File.ReadAllText(Path.Combine(Extensions.ExeDirectory(), "Init", "GameName.csproj")).Replace("{{{GameName}}}", gameName));
            File.WriteAllText(Path.Combine(gamePath, "src", gameName + ".sln"), File.ReadAllText(Path.Combine(Extensions.ExeDirectory(), "Init", "GameName.sln")).Replace("{{{GameName}}}", gameName));
            File.WriteAllText(Path.Combine(gamePath, "src", "Game.cs"), File.ReadAllText(Path.Combine(Extensions.ExeDirectory(), "Init", "Game.cs")).Replace("{{{GameName}}}", gameName));
            File.WriteAllText(Path.Combine(gamePath, "src", "LandingAreaLayout.cs"), File.ReadAllText(Path.Combine(Extensions.ExeDirectory(), "Init", "LandingAreaLayout.cs")).Replace("{{{GameName}}}", gameName));
            Directory.CreateDirectory(Path.Combine(gamePath, "assets", "images", "Landing"));

            File.Copy(Path.Combine(Extensions.ExeDirectory(), "Init", "assets", "Landing", "hello-world.png"), Path.Combine(gamePath, "assets", "images", "Landing", "hello-world.png"));
            File.Copy(Path.Combine(Extensions.ExeDirectory(), "Init", "assets", "Landing", "welcome.png"), Path.Combine(gamePath, "assets", "images", "Landing", "welcome.png"));
            AssetGenerator.Generate(gamePath, gameName);
        }




        public const string GitIngoreContent = @"
#OS junk files
[Tt]humbs.db
*.DS_Store
#Visual Studio files
*.[Oo]bj
*.user
*.aps
*.pch
*.vspscc
*.vssscc
*_i.c
*_p.c
*.ncb
*.tlb
*.tlh
*.bak
*.[Cc]ache
*.ilk
*.log
*.lib
*.sbr
*.sdf
*.suo
ipch/
obj/
[Bb]in
[Dd]ebug/
[Rr]elease*/
Ankh.NoLoad
_ReSharper*/
*.resharper
[Tt]est[Rr]esult*
.svn
~$*
platforms/";
    }
}