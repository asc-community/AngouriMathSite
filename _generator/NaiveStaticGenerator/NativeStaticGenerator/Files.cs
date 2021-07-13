using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveStaticGenerator
{
    public static class Files
    {
        public static IEnumerable<string> GatherFilesFromDir(string path)
        {
            var files = Directory.GetFiles(path).ToList();
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
                files.AddRange(GatherFilesFromDir(dir));
            return files;
        }
    }
}
