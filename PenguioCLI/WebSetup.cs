using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Threading;
using System.Web;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Project = Microsoft.Build.BuildEngine.Project;

namespace PenguioCLI
{
    public class WebSetup
    {
        public static void AddWebPlatform(string directory, string path, ProjectConfig project)
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

        public static BuildResult BuildWebPlatform(string directory, ProjectConfig project)
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
            var buildRequestData = new BuildRequestData(new ProjectInstance(Path.Combine(webPlatform, "Client.WebGame.csproj")), new[] { "Rebuild" });
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
            jsFiles += $"<script type=\"text/javascript\" src=\"js/bridge.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/engine.interfaces.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/engine.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/engine.web.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/engine.animation.js\" ></script>";

            jsFiles += $"<script type=\"text/javascript\" src=\"js/client.js\" ></script>";
            jsFiles += $"<script type=\"text/javascript\" src=\"js/client.web.js\" ></script>";


            foreach (var file in Directory.GetFiles(Path.Combine(platformOutput, "js"), project.ProjectName + "*.js").Where(a => !a.Contains(".min.")))
            {
                jsFiles += $"<script type=\"text/javascript\" src=\"{file.Replace(platformOutput + "\\", "")}\" ></script>";
            }

            File.WriteAllText(Path.Combine(platformOutput, "index.html"), "<!DOCTYPE html><html><head><style>body {padding: 50px;font: 14px \"Lucida Grande\", Helvetica, Arial, sans-serif;}* {margin: 0;padding: 0;}html, body {width: 100%;height: 100%;}canvas {display: block;margin: 0;position: absolute;top: 0;left: 0;z-index: 0;}.clickManager {display: block;margin: 0;position: absolute;top: 0;left: 0;z-index: 0;}</style></head><body>{{{js}}}</body></html>".Replace("{{{js}}}", jsFiles));



            return j;
        }

        public static void RunWebPlatform(string directory, ProjectConfig project, BuildResult build)
        {
            Console.WriteLine("Server running on 8018");
            new SimpleHTTPServer(Path.Combine(directory, "platforms/Web/bin/output"), 8018);
        }
    }
}



class SimpleHTTPServer
{
    private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

    private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;

    public int Port
    {
        get { return _port; }
        private set { }
    }

    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port)
    {
        this.Initialize(path, port);
    }

    /// <summary>
    /// Construct server with suitable port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _serverThread.Abort();
        _listener.Stop();
    }

    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (true)
        {
            try
            {
                HttpListenerContext context = _listener.GetContext();
                Process(context);
            }
            catch (Exception ex)
            {

            }
        }
    }

    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        Console.WriteLine(filename);
        filename = WebUtility.UrlDecode(filename);
        filename = filename.Substring(1);

        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        filename = Path.Combine(_rootDirectory, filename);

        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);

                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        context.Response.OutputStream.Close();
    }

    private void Initialize(string path, int port)
    {
        this._rootDirectory = path;
        this._port = port;
        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }


}