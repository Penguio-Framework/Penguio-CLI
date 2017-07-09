using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PenguioCLI
{
    public class AssetGenerator
    {
        public static void Generate(string gamePath, string gameName)
        {

            var assetsFile = File.ReadAllText(Path.Combine(Extensions.ExeDirectory(), "Init", "Assets.cs"));
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
                if (image.Last() == "add-content") continue;
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
    }
}