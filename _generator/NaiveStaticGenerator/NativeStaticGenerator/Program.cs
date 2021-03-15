using NaiveStaticGenerator;
using System;
using System.IO;
using System.Linq;
using YadgNet;

/*
Console.WriteLine(
    new DescriptionFromXmlBuilder(@"
<summary>
Is a special case of logarithm where the base equals
<a href=""https://en.wikipedia.org/wiki/E_(mathematical_constant)"">e</a>:
<a href=""https://en.wikipedia.org/wiki/Natural_logarithm""/>
</summary>
<param name=""a"">Argument node of which natural logarithm will be taken</param>
<returns>Logarithm node with base equal to e</returns>
").Build());

return;
*/
GeneratePagesFromDocs();
GenerateFinalWebsite();


static void GeneratePagesFromDocs()
{
    new WebsiteBuilder(
        // new PageSaver(@"D:\main\vs_prj\AngouriMath\tests\MyDocsTestDest")
        new PageSaver(@"D:\main\vs_prj\AngouriMath\AngouriMathSite\_generator\content\docs")
    )
    {
        MainPageName = "AngouriMath Almanac",
        MainPageDescription = @"
Welcome to the storage of documentation for 
all public methods, properties and fields for AngouriMath!
Please, consider these pages as those made for reference for particular members,
 it is not a guideline, manual, or tutorial.
"
    }
    .Build(
        DocsParser.Parse(
            @"D:\main\vs_prj\AngouriMath\AngouriMath\Sources\AngouriMath\bin\Release\netstandard2.0\AngouriMath.xml"
        ).Build()
    );
}



static void GenerateFinalWebsite()
{
    var contentName =
        @"D:\main\vs_prj\AngouriMath\AngouriMathSite\_generator";

    var rootName = Path.GetDirectoryName(contentName);

    var dirName = Path.Combine(contentName, "content");

    Console.WriteLine(dirName);
    var files = Directory.GetFiles(dirName);
    
    var top = File.ReadAllText(Path.Combine(dirName, "_templates", "top.html"));
    var bottom = File.ReadAllText(Path.Combine(dirName, "_templates", "bottom.html"));

    var id = 0;
    var allFiles = Files.GatherFilesFromDir(dirName);
    foreach (var file in allFiles)
    {
        id++;
        if (file.Contains("_templates"))
            continue;
        if (!file.EndsWith(".html"))
            continue;
        var content = File.ReadAllText(file);
        var relativeName = file.Substring(dirName.Length + 1);
        var relativePathName = Path.GetDirectoryName(relativeName).Replace('\\', '_');

        var newContent = top + "\n" + content + "\n" + bottom;

        newContent = PathUp(newContent, relativeName, "themes.css");
        newContent = PathUp(newContent, relativeName, "styles.css");
        newContent = PathUp(newContent, relativeName, "img/icon_cropped.png");
        newContent = newContent.Replace($"li><!--active_{relativePathName}-->", "li class=\"active-link\">");
        newContent = newContent.Replace("<tbody>", "<pre>");

        var newFilePath = Path.Combine(rootName, relativeName);
        var n = $"[{id} / {allFiles.Count()}]";
        Console.WriteLine($"{n} Read from  {file}");
        Console.WriteLine($"{n} Writing to {newFilePath}");

        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
        File.WriteAllText(newFilePath, newContent);
    }


    static string PathUp(string content, string relativeName, string subPath)
        => content.Replace("/" + subPath,
            string.Join("",
                Enumerable.Range(0, relativeName.Count(c => c == '\\')
                ).Select(c => "../")) + subPath);
}