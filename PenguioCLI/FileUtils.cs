using System.Collections.Generic;
using System.IO;

namespace PenguioCLI
{
    public static class FileUtils
    {
        public static List<string> DirectoryCopy(string root, string sourceDirName, string destDirName, bool copySubDirs, string filter = null)
        {
            List<string> fileNames = new List<string>();
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = filter == null ? dir.GetFiles() : dir.GetFiles(filter);
            foreach (FileInfo file in files)
            {
                if (file.Name .StartsWith(".") || file.Name=="add-content") continue;
                string temppath = Path.Combine(destDirName, file.Name);
                fileNames.Add(temppath.Replace(root + "\\", ""));
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if (subdir.Name == "obj" || subdir.Name == "bin") continue;
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    fileNames.AddRange(DirectoryCopy(root, subdir.FullName, temppath, copySubDirs, filter));
                }
            }
            return fileNames;
        }

    }
}