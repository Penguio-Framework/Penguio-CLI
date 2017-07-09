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
            /*
                        commands = new[] { "generate", "font" };
                        commands = new[] { "init", "OrbitalCrash" };
                        commands = new[] { "run", "android", "local" };
            */
//            commands = new[] { "generate", "assets" };

            //                                                commands = new[] {   "add", "web" };
            //            commands = new[] {   "rm", "web" };
            //                                                commands = new[] {  "run", "wd" };
            var directory = Directory.GetCurrentDirectory();
            /*
                        directory = @"C:\code\penguio\";
            */
//            directory = @"C:\code\penguio\PenguinShuffle\";




            if (commands.Length == 0 || commands[0].ToLower() == "/h")
            {
                Console.WriteLine("Penguio CLI");
                Console.WriteLine("Usage:");
                Console.WriteLine("peng init GameName");
                Console.WriteLine("peng generate font");
                Console.WriteLine("peng add WindowsDesktop");
                Console.WriteLine("peng rm Android");
                Console.WriteLine("peng build Web");
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
                    InitProject(directory, commands[1]);
                    return;
                case "add":
                case "a":
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

                    break;
                case "generate":
                case "g":

                    if (commands[1].ToLower() == "font")
                    {
                        FontGenerator.Generate(directory);
                    }
                    if (commands[1].ToLower() == "assets")
                    {
                        GenerateAssets(directory, getProject(directory).ProjectName);
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
                    switch (commands[1].ToLower())
                    {
                        case "windowsdesktop":
                        case "windows":
                        case "wd":
                            WindowsSetup.Build(directory);
                            return;
                        case "web":
                        case "w":
                            WebSetup.Build(directory, getProject(directory));
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
                        case "ios":
                        case "i":
                            IOSSetup.Debug(directory);
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
                            build = WebSetup.Build(directory, getProject(directory));
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

        private static void InitProject(string directory, string gameName)
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

            File.WriteAllText(Path.Combine(gamePath, "src", gameName + ".csproj"), File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Init", "GameName.csproj")).Replace("{{{GameName}}}", gameName));
            File.WriteAllText(Path.Combine(gamePath, "src", gameName + ".sln"), File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Init", "GameName.sln")).Replace("{{{GameName}}}", gameName));
            File.WriteAllText(Path.Combine(gamePath, "src", "Game.cs"), File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Init", "Game.cs")).Replace("{{{GameName}}}", gameName));
            GenerateAssets(gamePath, gameName);

        }

        private static void GenerateAssets(string gamePath, string gameName)
        {

            var assetsFile = File.ReadAllText(Path.Combine(System.Windows.Forms.Application.ExecutablePath.RemoveLastSplit('\\'), "Init", "Assets.cs"));
            assetsFile = assetsFile.Replace("{{{GameName}}}", gameName);

            Tree<string> tree = new Tree<string>("Assets");
            BuildAssetContent(tree.AddChild("images"), Path.Combine(gamePath, "assets", "images"));
            BuildAssetContent(tree.AddChild("sounds"), Path.Combine(gamePath, "assets", "sounds"));
            BuildAssetContent(tree.AddChild("songs"), Path.Combine(gamePath, "assets", "songs"));

            var fontFile = JsonConvert.DeserializeObject<FontFile>(File.ReadAllText(Path.Combine(gamePath, "assets", "fonts", "fonts.json")));


            var assetBuiler = new StringBuilder();

            if (tree.GetChild("sounds").IsLeaf())
            {
                assetsFile = assetsFile.Replace("{{{SoundEffects}}}", "");
            }
            else
            {
                var result = BuildClasses(tree.GetChild("sounds"), "ISoundEffect", "CreateSoundEffect", false);
                assetsFile = assetsFile.Replace("{{{SoundEffects}}}", result.Item1);
                assetBuiler.AppendLine(result.Item2);
            }

            if (tree.GetChild("images").IsLeaf())
            {
                assetsFile = assetsFile.Replace("{{{Images}}}", "");
            }
            else
            {
                var result = BuildClasses(tree.GetChild("images"), "IImage", "CreateImage", false);
                assetsFile = assetsFile.Replace("{{{Images}}}", result.Item1);
                assetBuiler.AppendLine(result.Item2);
            }

            if (tree.GetChild("songs").IsLeaf())
            {
                assetsFile = assetsFile.Replace("{{{Songs}}}", "");
            }
            else
            {
                var result = BuildClasses(tree.GetChild("songs"), "ISong", "CreateSong", false);
                assetsFile = assetsFile.Replace("{{{Songs}}}", result.Item1);
                assetBuiler.AppendLine(result.Item2);
            }

            if (fontFile.Fonts.Count == 0)
            {
                assetsFile = assetsFile.Replace("{{{Fonts}}}", "");
            }
            else
            {

                var fontTree = tree.AddChild("fonts");

                foreach (var fontFileFont in fontFile.Fonts)
                {
                    var font = fontTree.GetChild(fontFileFont.FontName) ?? fontTree.AddChild(fontFileFont.FontName);
                    font.AddChild("_" + fontFileFont.FontSize);
                }


                var result = BuildClasses(tree.GetChild("fonts"), "IFont", "CreateFont", true);
                assetsFile = assetsFile.Replace("{{{Fonts}}}", result.Item1);
                assetBuiler.AppendLine(result.Item2);
            }

            assetsFile = assetsFile.Replace("{{{Fonts}}}", "");
            assetsFile = assetsFile.Replace("{{{LoadAssets}}}", assetBuiler.ToString());

            File.WriteAllText(Path.Combine(gamePath, "src", "Assets.cs"), assetsFile);
        }

        private static void BuildAssetContent(Tree<string> tree, string gamePath)
        {
            var imagesDir = DirSearch(gamePath);
            var assets = imagesDir.Select(img => img.Replace(gamePath, "").Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            foreach (var image in assets)
            {
                image.Aggregate(tree, (current, im) => current.GetChild(im) ?? current.AddChild(im));
            }
        }

        public static Tuple<string, string> BuildClasses(Tree<string> tree, string type, string createMethod, bool isFont)
        {
            StringBuilder sbClasses = new StringBuilder();
            StringBuilder sbBuilder = new StringBuilder();
            if (tree.IsLeaf())
            {
                /*regular asset*/
                sbClasses.AppendLine($"public static {type} {SanatizeFieldName(tree.Key)} {{get;set;}}");

                var path = tree.GetPath(SanatizeFieldName, ".");
                var fileName = tree.GetPath(a => a, "/").Replace("Assets/", "").RemoveLastSplit('.');
                if (isFont)
                {
                    fileName = Regex.Replace(fileName, "_([0-9]+)", a =>
                    {
                        var fontName = fileName.Split('/')[1];
                        var size = a.Groups[1].Value;
                        return fontName + "-" + size + "pt";
                    });
                }
                sbBuilder.AppendLine($"{path} = assetManager.{createMethod}(\"{fileName}\");");
            }
            else
            {
                int m;
                if (!isFont &&
                    tree.Children.All(a => a.Key.Any(b => int.TryParse(b.ToString(), out m))) &&
                    tree.Children.All(a => Regex.Replace(a.Key, @"[\d]", string.Empty) == Regex.Replace(tree.Children.First().Key, @"[\d]", string.Empty))
                    )
                {
                    if (tree.Children.All(a => a.IsLeaf()))
                    {
                        /*numbered asset*/

                        sbClasses.AppendLine($"public static class {SanatizeFieldName(tree.Key)} {{");

                        var keys = string.Join(",", tree.Children.Select(a => int.Parse(Regex.Match(a.Key, @"[\d]").Value)));

                        var key = SanatizeFieldName(Regex.Replace(tree.Children.First().Key, @"[\d]", string.Empty));
                        sbClasses.AppendLine($"public static Dictionary<int,{type}> {key} {{get;set;}}");
                        var path = tree.GetPath(SanatizeFieldName, ".") + $".{key}";
                        sbClasses.AppendLine("}");
                        sbBuilder.AppendLine($"{path} = assetManager.{createMethod}(\"{tree.GetPath(a => a, "/").Replace("Assets/", "")}/{Regex.Replace(tree.Children.First().Key, @"[\d]", "{0}").RemoveLastSplit('.')}\",new []{{{keys}}});");
                    }
                    else
                    {
                        /*numbered asset folder*/
                        var nextChildren = tree.Children.SelectMany(a => a.Children).ToArray();
                        if (
                            nextChildren.All(a => a.Key.Any(b => int.TryParse(b.ToString(), out m))) &&
                            nextChildren.All(a => Regex.Replace(a.Key, @"[\d]", string.Empty) == Regex.Replace(nextChildren.First().Key, @"[\d]", string.Empty))
                        )
                        {
                            sbClasses.AppendLine($"public static class {SanatizeFieldName(tree.Key)} {{");
                            var key = Regex.Replace(nextChildren.First().Key, @"[\d]", string.Empty);
                            sbClasses.AppendLine($"public static Dictionary<int,Dictionary<int,{type}>> {SanatizeFieldName(key)} {{get;set;}}");
                            sbClasses.AppendLine("}");

                            var keysOuter = string.Join(",", tree.Children.Select(a => int.Parse(Regex.Match(a.Key, @"[\d]").Value)));
                            var keysInner = string.Join(",", nextChildren.Select(a => int.Parse(Regex.Match(a.Key, @"[\d]").Value)).Distinct());
                            var path = tree.GetPath(SanatizeFieldName, ".") + $".{SanatizeFieldName(key)}";
                            var replacedOuterPath = Regex.Replace(tree.Children.First().Key, @"[\d]", "{0}").RemoveLastSplit('.');
                            var replacedInnerPath = Regex.Replace(nextChildren.First().Key, @"[\d]", "{1}").RemoveLastSplit('.');
                            sbBuilder.AppendLine($"{path} = assetManager.{createMethod}(\"{tree.GetPath(a => a, "/").Replace("Assets/", "")}/{replacedOuterPath}/{replacedInnerPath}\",new []{{{keysOuter}}},new []{{{keysInner}}});");

                        }

                    }

                }
                else
                {
                    /*regular folder*/
                    sbClasses.AppendLine($"public static class {SanatizeFieldName(tree.Key)} {{");
                    foreach (var child in tree.Children)
                    {
                        var result = BuildClasses(child, type, createMethod, isFont);
                        sbClasses.Append(result.Item1);
                        sbBuilder.Append(result.Item2);
                    }
                    sbClasses.AppendLine("}");
                }

            }
            return Tuple.Create(sbClasses.ToString(), sbBuilder.ToString());
        }

        public static string SanatizeFieldName(string name)
        {

            name = name[0].ToString().ToUpper() + name.Substring(1);
            name = name.Replace("." + name.Split('.').Last(), "");
            name = name.Replace(".", "-");
            var gex = new Regex("(\\-\\w)");
            name = gex.Replace(name, (a) => a.Value[1].ToString().ToUpper());
            name = name.Replace("-", "");
            name = name.Replace(" ", "");
            return name;
        }
        static List<string> DirSearch(string sDir)
        {
            List<string> strs = new List<string>();
            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    strs.Add(f);
                }
                strs.AddRange(DirSearch(d));
            }
            foreach (string f in Directory.GetFiles(sDir))
            {
                strs.Add(f);
            }
            return strs;
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

    [DebuggerStepThrough]
    public class Tree<T>
    {
        public override string ToString()
        {
            return $"{Key}";
        }
        public Tree<T> Parent { get; set; }
        public T Key { get; set; }
        public List<Tree<T>> Children { get; set; }

        public Tree(T key)
        {
            Key = key;
            Children = new List<Tree<T>>();
        }

        public Tree<T> GetChild(T key)
        {
            return Children.FirstOrDefault(a => Equals(a.Key, key));
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public Tree<T> AddChild(T child)
        {
            var item = new Tree<T>(child);
            item.Parent = this;
            Children.Add(item);
            return item;
        }

        public string GetPath(Func<T, string> func, string concat)
        {
            var m = func(this.Key);
            if (Parent != null)
                return Parent.GetPath(func, concat) + concat + m;
            else return m;
        }
    }


    public static class Extensions
    {
        public static string RemoveLastSplit(this string s, char sp)
        {
            return s.Replace(sp + s.Split(sp).Last(), "");
        }
    }
}


