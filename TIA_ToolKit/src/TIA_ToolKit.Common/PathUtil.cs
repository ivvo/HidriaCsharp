using System;
using System.IO;

namespace TIA_ToolKit
{
    public static class PathUtil
    {
        /// <summary>Pot fullPath relativno na baseDir; če ni pod baseDir, vrne polno pot.</summary>
        public static string MakeRelative(string baseDir, string fullPath)
        {
            string b = Path.GetFullPath(baseDir).TrimEnd('\\', '/') + "\\";
            string f = Path.GetFullPath(fullPath);
            return f.StartsWith(b, StringComparison.OrdinalIgnoreCase) ? f.Substring(b.Length) : f;
        }
    }
}
