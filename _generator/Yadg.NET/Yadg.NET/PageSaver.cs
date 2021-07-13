using System;
using System.IO;

namespace YadgNet
{
    public sealed class PageSaver : IPageSave
    {
        private readonly string rootPath;
        public PageSaver(string path)
            => this.rootPath = path;
        public void Save(string path, string text)
        {
            var finalPath = Path.Combine(rootPath, path) ?? throw new NullReferenceException();
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath) ?? throw new NullReferenceException());
            File.WriteAllText(finalPath, text);
            Console.WriteLine($"Writing to {finalPath}");
        }
    }
}
