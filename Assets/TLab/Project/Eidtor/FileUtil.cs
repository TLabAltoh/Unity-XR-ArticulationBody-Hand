using System.IO;

namespace TLab.Project.Editor
{
    public static class FileUtil
    {
        public static bool FileExists(string assetPath)
        {
            if (assetPath.Length < "Assets".Length - 1)
                return false;

            return File.Exists(assetPath);
        }
    }
}