using NaiveStaticGenerator;
using System;
using System.IO;
using System.Linq;

var contentName = 
    Path.GetDirectoryName(
        Path.GetDirectoryName(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Directory.GetCurrentDirectory()))));

var rootName = Path.GetDirectoryName(contentName);

var dirName = Path.Combine(contentName, "content");

Console.WriteLine(dirName);
var files = Directory.GetFiles(dirName);

var top = File.ReadAllText(Path.Combine(dirName, "_templates", "top.html"));
var bottom = File.ReadAllText(Path.Combine(dirName, "_templates", "bottom.html"));

foreach (var file in Files.GatherFilesFromDir(dirName))
{
    if (file.Contains("_templates"))
        continue;
    var content = File.ReadAllText(file);
    var relativeName = file.Substring(dirName.Length + 1);
    var relativePathName = Path.GetDirectoryName(relativeName).Replace('\\', '_');

    var newContent = top + "\n" + content + "\n" + bottom;    

    newContent = PathUp(newContent, relativeName, "themes.css");
    newContent = PathUp(newContent, relativeName, "styles.css");
    newContent = PathUp(newContent, relativeName, "img/icon_cropped.png");
    newContent = newContent.Replace($"li><!--active_{relativePathName}-->", "li class=\"active-link\">");

    var newFilePath = Path.Combine(rootName, relativeName);
    Console.WriteLine($"Read from  {file}");
    Console.WriteLine($"Writing to {newFilePath}");
    File.WriteAllText(newFilePath, newContent);
}


static string PathUp(string content, string relativeName, string subPath)
    => content.Replace("/" + subPath,
        string.Join("",
            Enumerable.Range(0, relativeName.Count(c => c == '\\')
            ).Select(c => "../")) + subPath);
