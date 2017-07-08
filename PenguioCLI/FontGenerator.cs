using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PenguioCLI
{
    public class FontGenerator
    {
        private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const int charsPerRow = 30;

        public static void Generate(string directory)
        {
            var fontPath = Path.Combine(directory, "assets", "fonts");
            var fontJson = Path.Combine(directory, "assets", "fonts", "fonts.json");
            var fontFile = JsonConvert.DeserializeObject<FontFile>(File.ReadAllText(fontJson));

            foreach (var fontContent in fontFile.Fonts)
            {
                var fontSize = fontContent.FontSize;


                if (!Directory.Exists($@"{fontPath}\{fontContent.FontName}\"))
                {
                    Directory.CreateDirectory($@"{fontPath}\{fontContent.FontName}\");
                }

                TextureBuilder tb = new TextureBuilder();
                Settings.BitmapFilename = string.Format(@"{0}\{1}\{1}-{2}pt.png", fontPath, fontContent.FontName, fontSize);
                Settings.CharactersInARow = charsPerRow;
                Settings.ExtraSpaceWidth = 0;
                Settings.Font = new FontSettings();



                Settings.Font.Family = new FontFamily(fontContent.FontName);
                Settings.Font.Style = FontStyles.Normal;
                Settings.Font.Weight = FontWeights.Normal;
                Settings.Font.Stretch = FontStretches.Normal;
                Settings.Font.Size = 30.0;
                Settings.Font.IsDecorationEnabled = false;
                Settings.Font.Decoration = TextDecorations.Baseline;

                Settings.IsSpaceEnabled = true;
                Settings.MetricsFilename = string.Format(@"{0}\{1}\{1}-{2}pt.xml", fontPath, fontContent.FontName, fontSize);
                Settings.SpaceWidth = 8;

                List<int> characters = new List<int>();

                if (fontContent.Characters == null)
                {
                    int lower;
                    int upper;

                    lower = 32;
                    upper = 127;
                    for (int from = lower; from <= upper; ++from)
                    {
                        characters.Add(from);
                    }
                }
                else if (fontContent.Characters == "$NUMBERS$")
                {
                    fontContent.Characters += Numbers + " ";
                    characters = fontContent.Characters.Select(a => (int)a).Distinct().ToList();
                }
                else if (fontContent.Characters == "$LETTERS$")
                {
                    fontContent.Characters += Letters + " ";
                    characters = fontContent.Characters.Select(a => (int)a).Distinct().ToList();
                }
                else if (fontContent.Characters == "$NUMBERS-LETTERS$" || fontContent.Characters == "$LETTERS-NUMBERS$")
                {
                    fontContent.Characters += Numbers + Letters + " ";
                    characters = fontContent.Characters.Select(a => (int)a).Distinct().ToList();
                }
                else
                {
                    fontContent.Characters += " ";
                    characters = fontContent.Characters.Select(a => (int)a).Distinct().ToList();
                }

                tb.CreateSpriteFont(fontSize, characters);


            }
        }
    }

    public class FontFile
    {
        public List<FontContent> Fonts { get; set; }
    }
    public class FontContent
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public string Characters { get; set; }
    }

    public class Rectangle
    {
        public Rectangle()
        {
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle(int x, int y, int width, int height)
        {

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }


        public bool Contains(int x, int y)
        {
            return X < x && Y < y && X + Width > x && Y + Height > y;
        }


    }




    public class TextureBuilder
    {
        private BitmapBuilder builder;

        public TextureBuilder()
        {
            this.builder = new BitmapBuilder();
        }


        public Typeface CreateTypeface()
        {
            return new Typeface(Settings.Font.Family, Settings.Font.Style, Settings.Font.Weight, Settings.Font.Stretch);
        }

        public BitmapSource CreateSpriteFont(double fontSize, List<int> characters)
        {
            BitmapSource bitmapSource1 = (BitmapSource)null;
            try
            {
                List<BitmapSource> list1 = new List<BitmapSource>();
                List<BitmapSource> bitmapList = new List<BitmapSource>();
                List<Int32Rect> list2 = new List<Int32Rect>();
                XDocument metricsDocument = this.CreateMetricsDocument();
                int num1 = int.MaxValue;
                int num2 = int.MinValue;

                var typeface = CreateTypeface();




                foreach (var character in characters)
                {

                    BitmapSource bitmapSource2 = (BitmapSource)null;
                    try
                    {

                        bitmapSource2 = (BitmapSource)this.builder.CreateBitmap(((char)character).ToString(), typeface, fontSize);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    Int32Rect bounds = this.builder.GetBounds(bitmapSource2);
                    if (!bounds.IsEmpty)
                    {
                        if (bounds.Y < num1)
                            num1 = bounds.Y;
                        if (bounds.Y + bounds.Height > num2)
                            num2 = bounds.Y + bounds.Height;
                    }
                    list1.Add(bitmapSource2);
                    list2.Add(bounds);
                    this.AddMetrics(metricsDocument, character);
                }


                for (int index = 0; index < list1.Count; ++index)
                {
                    BitmapSource bitmapSource2 = list1[index];
                    Int32Rect bounds = list2[index];
                    Size size = Size.Empty;
                    if (bounds.IsEmpty)
                    {
                        double val1 = 0.0;
                        size = new Size(val1 + fontSize / 3.0, (double)(num2 - num1));
                    }
                    else
                        size = new Size((double)bounds.Width, (double)(num2 - num1));
                    Point location = new Point(0.0, (double)(bounds.Y - num1));
                    BitmapSource bitmapSource3 = this.builder.CropBitmap(bitmapSource2, bounds, size, location);
                    bitmapList.Add(bitmapSource3);
                }
                bitmapSource1 = this.CreateBitmap(bitmapList, metricsDocument);
                this.SaveBitmap(bitmapSource1, Settings.BitmapFilename);
                this.SaveMetrics(metricsDocument);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bitmapSource1;
        }

        private void SaveBitmap(BitmapSource bitmapSource, string filename)
        {
            try
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapSource);
                FileStream fileStream = new FileStream(filename, FileMode.Create);
                BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(bitmapFrame);
                bitmapEncoder.Save((Stream)fileStream);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SaveMetrics(XDocument metrics)
        {
            if (metrics == null)
                return;
            try
            {
                metrics.Save(Settings.MetricsFilename);
                File.WriteAllText(Settings.MetricsFilename.Replace("xml", "js"), JsonConvert.SerializeXNode(metrics, Formatting.None, true).Replace("@", ""));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private BitmapSource CreateBitmap(List<BitmapSource> bitmapList, XDocument metrics)
        {
            RenderTargetBitmap renderTargetBitmap = (RenderTargetBitmap)null;
            SolidColorBrush solidColorBrush = new SolidColorBrush(new Color() { R = 255 });
            try
            {
                int val1 = 0;
                int num1 = Settings.SpaceWidth;
                if (!Settings.IsSpaceEnabled)
                    num1 = 0;
                int val2 = num1;
                int num2 = num1;
                if (bitmapList.Count == 0)
                    return (BitmapSource)null;
                int num3 = bitmapList[0].PixelHeight + 2 * Settings.ExtraSpaceWidth;
                for (int index = 0; index < bitmapList.Count; ++index)
                {
                    val2 += bitmapList[index].PixelWidth + 2 * Settings.ExtraSpaceWidth + num1;
                    if ((index + 1) % Settings.CharactersInARow == 0 && index + 1 < bitmapList.Count)
                    {
                        val1 = Math.Max(val1, val2);
                        val2 = num1;
                        num2 += num3 + num1;
                    }
                }
                int pixelWidth = Math.Max(val1, val2);
                int pixelHeight = num2 + num3 + num1;
                renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                int num4 = 0;
                int num5 = num1;
                Rect rect = new Rect(0.0, 0.0, (double)pixelWidth, (double)num1);
                drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                for (int index = 0; index < bitmapList.Count; ++index)
                {
                    int num6 = bitmapList[index].PixelWidth + 2 * Settings.ExtraSpaceWidth;
                    rect = new Rect((double)num4, (double)num5, (double)num1, (double)num3);
                    drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                    int num7 = num4 + num1;
                    rect = new Rect((double)(num7 + Settings.ExtraSpaceWidth), (double)(num5 + Settings.ExtraSpaceWidth), (double)(num6 - 2 * Settings.ExtraSpaceWidth), (double)(num3 - 2 * Settings.ExtraSpaceWidth));
                    drawingContext.DrawImage((ImageSource)bitmapList[index], rect);
                    rect = new Rect((double)num7, (double)num5, (double)num6, (double)num3);
                    this.AddMetricsRect(metrics, index, rect);
                    num4 = num7 + num6;
                    if ((index + 1) % Settings.CharactersInARow == 0 && index + 1 < bitmapList.Count)
                    {
                        if (Settings.IsSpaceEnabled)
                        {
                            rect = new Rect((double)num4, (double)num5, (double)(pixelWidth - num4), (double)num3);
                            drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                        }
                        int num8 = num5 + num3;
                        rect = new Rect(0.0, (double)num8, (double)pixelWidth, (double)num1);
                        drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                        num5 = num8 + num1;
                        num4 = 0;
                    }
                }
                if (Settings.IsSpaceEnabled)
                {
                    rect = new Rect((double)num4, (double)num5, (double)(pixelWidth - num4), (double)num3);
                    drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                }
                rect = new Rect(0.0, (double)(num5 + num3), (double)pixelWidth, (double)num1);
                drawingContext.DrawRectangle((Brush)solidColorBrush, (Pen)null, rect);
                drawingContext.Close();
                renderTargetBitmap.Render((Visual)drawingVisual);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return (BitmapSource)renderTargetBitmap;
        }

        private XDocument CreateMetricsDocument()
        {
            XDocument xdocument = (XDocument)null;
            try
            {
                xdocument = new XDocument();
                XElement xelement = new XElement((XName)"fontMetrics");
                XAttribute xattribute = new XAttribute((XName)"file", "a");
                xelement.Add((object)xattribute);
                xdocument.Add((object)xelement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return xdocument;
        }

        private void AddMetrics(XDocument document, int key)
        {
            try
            {
                XElement xelement = new XElement((XName)"character");
                XAttribute xattribute = new XAttribute("character", (object)key);
                xelement.Add((object)xattribute);
                document.Root.Add((object)xelement);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void AddMetricsRect(XDocument document, int index, Rect rect)
        {
            try
            {
                XElement xelement1 = new XElement((XName)"x", (object)rect.X);
                XElement xelement2 = new XElement((XName)"y", (object)rect.Y);
                XElement xelement3 = new XElement((XName)"width", (object)rect.Width);
                XElement xelement4 = new XElement((XName)"height", (object)rect.Height);
                Enumerable.ElementAt<XElement>(document.Root.Elements(), index).Add((object)xelement1);
                Enumerable.ElementAt<XElement>(document.Root.Elements(), index).Add((object)xelement2);
                Enumerable.ElementAt<XElement>(document.Root.Elements(), index).Add((object)xelement3);
                Enumerable.ElementAt<XElement>(document.Root.Elements(), index).Add((object)xelement4);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    internal class Settings
    {
        public static bool IsSpaceEnabled { get; set; }
        public static int SpaceWidth { get; set; }
        public static FontSettings Font { get; set; }
        public static int ExtraSpaceWidth { get; set; }
        public static string BitmapFilename { get; set; }
        public static string MetricsFilename { get; set; }
        public static int CharactersInARow { get; set; }
    }

    [Serializable]
    public class FontSettings
    {
        [XmlIgnore]
        public FontFamily Family { get; set; }

        public string FontFamily
        {
            get
            {
                return this.Family.Source;
            }
            set
            {
                this.Family = new FontFamily(value);
            }
        }

        [XmlIgnore]
        public FontStyle Style { get; set; }

        public string FontStyle
        {
            get
            {
                return this.Style.ToString();
            }
            set
            {
                if (value == FontStyles.Italic.ToString())
                    this.Style = FontStyles.Italic;
                else if (value == FontStyles.Oblique.ToString())
                    this.Style = FontStyles.Oblique;
                else
                    this.Style = FontStyles.Normal;
            }
        }

        [XmlIgnore]
        public FontWeight Weight { get; set; }

        public string FontWeight
        {
            get
            {
                return this.Weight.ToString();
            }
            set
            {
                if (value == FontWeights.Black.ToString())
                    this.Weight = FontWeights.Black;
                else if (value == FontWeights.Bold.ToString())
                    this.Weight = FontWeights.Bold;
                else if (value == FontWeights.DemiBold.ToString())
                    this.Weight = FontWeights.DemiBold;
                else if (value == FontWeights.ExtraBlack.ToString())
                    this.Weight = FontWeights.ExtraBlack;
                else if (value == FontWeights.ExtraBold.ToString())
                    this.Weight = FontWeights.ExtraBold;
                else if (value == FontWeights.ExtraLight.ToString())
                    this.Weight = FontWeights.ExtraLight;
                else if (value == FontWeights.Heavy.ToString())
                    this.Weight = FontWeights.Heavy;
                else if (value == FontWeights.Light.ToString())
                    this.Weight = FontWeights.Light;
                else if (value == FontWeights.Medium.ToString())
                    this.Weight = FontWeights.Medium;
                else if (value == FontWeights.Regular.ToString())
                    this.Weight = FontWeights.Regular;
                else if (value == FontWeights.SemiBold.ToString())
                    this.Weight = FontWeights.SemiBold;
                else if (value == FontWeights.Thin.ToString())
                    this.Weight = FontWeights.Thin;
                else if (value == FontWeights.UltraBlack.ToString())
                    this.Weight = FontWeights.UltraBlack;
                else if (value == FontWeights.UltraBold.ToString())
                    this.Weight = FontWeights.UltraBold;
                else if (value == FontWeights.UltraLight.ToString())
                    this.Weight = FontWeights.UltraLight;
                else
                    this.Weight = FontWeights.Normal;
            }
        }

        [XmlIgnore]
        public FontStretch Stretch { get; set; }

        public string FontStretch
        {
            get
            {
                return this.Stretch.ToString();
            }
            set
            {
                if (value == FontStretches.Condensed.ToString())
                    this.Stretch = FontStretches.Condensed;
                else if (value == FontStretches.Expanded.ToString())
                    this.Stretch = FontStretches.Expanded;
                else if (value == FontStretches.ExtraCondensed.ToString())
                    this.Stretch = FontStretches.ExtraCondensed;
                else if (value == FontStretches.ExtraExpanded.ToString())
                    this.Stretch = FontStretches.ExtraExpanded;
                else if (value == FontStretches.Medium.ToString())
                    this.Stretch = FontStretches.Medium;
                else if (value == FontStretches.SemiCondensed.ToString())
                    this.Stretch = FontStretches.SemiCondensed;
                else if (value == FontStretches.SemiExpanded.ToString())
                    this.Stretch = FontStretches.SemiExpanded;
                else if (value == FontStretches.UltraCondensed.ToString())
                    this.Stretch = FontStretches.UltraCondensed;
                else if (value == FontStretches.UltraExpanded.ToString())
                    this.Stretch = FontStretches.UltraExpanded;
                else
                    this.Stretch = FontStretches.Normal;
            }
        }

        public bool IsDecorationEnabled { get; set; }

        public TextDecorationCollection Decoration { get; set; }

        public double Size { get; set; }
    }


    public class BitmapBuilder
    {
        private const int BORDER = 50;

        public RenderTargetBitmap CreateBitmap(string text, Typeface typeface, double fontSize)
        {
            RenderTargetBitmap renderTargetBitmap = (RenderTargetBitmap)null;
            try
            {
                Pen pen = this.CreatePen();
                Brush brush = this.CreateBrush();
                FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, (Brush)Brushes.White);
                Geometry geometry = formattedText.BuildGeometry(new System.Windows.Point(50.0, 50.0));
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                BitmapEffectGroup bitmapEffectGroup = new BitmapEffectGroup();
                DropShadowBitmapEffect shadowBitmapEffect = new DropShadowBitmapEffect();
                OuterGlowBitmapEffect glowBitmapEffect = new OuterGlowBitmapEffect();
                BevelBitmapEffect bevelBitmapEffect = new BevelBitmapEffect();
                BlurBitmapEffect blurBitmapEffect = new BlurBitmapEffect();
                EmbossBitmapEffect embossBitmapEffect = new EmbossBitmapEffect();
                /*      foreach (Nubik.Tools.SpriteFont.Enums.Effect effect in (IEnumerable<Nubik.Tools.SpriteFont.Enums.Effect>)GlobalObject<Settings>.Instance.EffectOrder.Values)
                      {
                          if (effect == Nubik.Tools.SpriteFont.Enums.Effect.DropShadow && GlobalObject<Settings>.Instance.DropShadow.IsEnabled)
                              bitmapEffectGroup.Children.Add((BitmapEffect)shadowBitmapEffect);
                          else if (effect == Nubik.Tools.SpriteFont.Enums.Effect.OuterGlow && GlobalObject<Settings>.Instance.OuterGlow.IsEnabled)
                              bitmapEffectGroup.Children.Add((BitmapEffect)glowBitmapEffect);
                          else if (effect == Nubik.Tools.SpriteFont.Enums.Effect.Bevel && GlobalObject<Settings>.Instance.Bevel.IsEnabled)
                              bitmapEffectGroup.Children.Add((BitmapEffect)bevelBitmapEffect);
                          else if (effect == Nubik.Tools.SpriteFont.Enums.Effect.Blur && GlobalObject<Settings>.Instance.Blur.IsEnabled)
                              bitmapEffectGroup.Children.Add((BitmapEffect)blurBitmapEffect);
                          else if (effect == Nubik.Tools.SpriteFont.Enums.Effect.Emboss && GlobalObject<Settings>.Instance.Emboss.IsEnabled)
                              bitmapEffectGroup.Children.Add((BitmapEffect)embossBitmapEffect);
                      }*/
                drawingContext.PushEffect((BitmapEffect)bitmapEffectGroup, (BitmapEffectInput)null);
                drawingContext.DrawGeometry(brush, pen, geometry);
                drawingContext.Close();
                if (double.IsInfinity(geometry.Bounds.X) || double.IsInfinity(geometry.Bounds.Y))
                    return renderTargetBitmap;
                int pixelWidth = 0;
                int pixelHeight = 0;
                try
                {
                    pixelWidth = Convert.ToInt32(geometry.Bounds.X + geometry.Bounds.Width) + 50;
                    pixelHeight = Convert.ToInt32(geometry.Bounds.Y + geometry.Bounds.Height) + 50;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32);
                renderTargetBitmap.Render((Visual)drawingVisual);
                try
                {
                    renderTargetBitmap.Freeze();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return renderTargetBitmap;
        }

        public Int32Rect GetBounds(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                return Int32Rect.Empty;
            Int32Rect int32Rect = new Int32Rect();
            int32Rect.X = this.GetLeftBound(bitmapSource);
            int32Rect.Y = this.GetUpperBound(bitmapSource);
            int32Rect.Width = this.GetRightBound(bitmapSource) - int32Rect.X + 1;
            int32Rect.Height = this.GetLowerBound(bitmapSource) - int32Rect.Y + 1;
            return int32Rect;
        }

        public BitmapSource CropBitmap(BitmapSource bitmapSource, Int32Rect bounds, Size size, Point location)
        {
            RenderTargetBitmap renderTargetBitmap = (RenderTargetBitmap)null;
            try
            {
                renderTargetBitmap = new RenderTargetBitmap(Convert.ToInt32(size.Width), Convert.ToInt32(size.Height), 96.0, 96.0, PixelFormats.Pbgra32);
                if (bitmapSource == null)
                    return (BitmapSource)renderTargetBitmap;
                CroppedBitmap croppedBitmap = new CroppedBitmap(bitmapSource, bounds);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                Rect rectangle = new Rect(location.X, location.Y, croppedBitmap.Width, croppedBitmap.Height);
                drawingContext.DrawImage((ImageSource)croppedBitmap, rectangle);
                drawingContext.Close();
                renderTargetBitmap.Render((Visual)drawingVisual);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return (BitmapSource)renderTargetBitmap;
        }

        private Pen CreatePen()
        {
            Pen pen = (Pen)null;
            return pen;
        }

        private Brush CreateBrush()
        {
            Brush brush = (Brush)null;
            brush = (Brush)new SolidColorBrush(new System.Windows.Media.Color() { R = 255, G = 255, B = 255, A = 255 });
            brush.Opacity = 1;

            return brush;
        }

        private int GetLeftBound(BitmapSource bitmapSource)
        {
            int num1 = bitmapSource.Format.BitsPerPixel / 8;
            int stride = num1;
            for (int x = 0; x < bitmapSource.PixelWidth; ++x)
            {
                Int32Rect sourceRect = new Int32Rect(x, 0, 1, bitmapSource.PixelHeight);
                byte[] numArray = new byte[bitmapSource.PixelHeight * num1];
                bitmapSource.CopyPixels(sourceRect, (Array)numArray, stride, 0);
                foreach (int num2 in numArray)
                {
                    if (num2 > 0)
                        return x;
                }
            }
            return -1;
        }

        private int GetUpperBound(BitmapSource bitmapSource)
        {
            int num1 = bitmapSource.Format.BitsPerPixel / 8;
            int stride = bitmapSource.PixelWidth * num1;
            for (int y = 0; y < bitmapSource.PixelHeight; ++y)
            {
                Int32Rect sourceRect = new Int32Rect(0, y, bitmapSource.PixelWidth, 1);
                byte[] numArray = new byte[bitmapSource.PixelWidth * num1];
                bitmapSource.CopyPixels(sourceRect, (Array)numArray, stride, 0);
                foreach (int num2 in numArray)
                {
                    if (num2 > 0)
                        return y;
                }
            }
            return -1;
        }

        private int GetRightBound(BitmapSource bitmapSource)
        {
            int num1 = bitmapSource.Format.BitsPerPixel / 8;
            int stride = num1;
            for (int x = bitmapSource.PixelWidth - 1; x >= 0; --x)
            {
                Int32Rect sourceRect = new Int32Rect(x, 0, 1, bitmapSource.PixelHeight);
                byte[] numArray = new byte[bitmapSource.PixelHeight * num1];
                bitmapSource.CopyPixels(sourceRect, (Array)numArray, stride, 0);
                foreach (int num2 in numArray)
                {
                    if (num2 > 0)
                        return x;
                }
            }
            return -1;
        }

        private int GetLowerBound(BitmapSource bitmapSource)
        {
            int num1 = bitmapSource.Format.BitsPerPixel / 8;
            int stride = bitmapSource.PixelWidth * num1;
            for (int y = bitmapSource.PixelHeight - 1; y >= 0; --y)
            {
                Int32Rect sourceRect = new Int32Rect(0, y, bitmapSource.PixelWidth, 1);
                byte[] numArray = new byte[bitmapSource.PixelWidth * num1];
                bitmapSource.CopyPixels(sourceRect, (Array)numArray, stride, 0);
                foreach (int num2 in numArray)
                {
                    if (num2 > 0)
                        return y;
                }
            }
            return -1;
        }
    }

}