using NaiveStaticGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YadgNet;

const string GENERATOR_PATH = @"D:\main\vs_prj\AngouriMath\AngouriMathSite\_generator";

/*
Console.WriteLine(
    new DescriptionFromXmlBuilder(@"
            <summary>
            Attempt to find analytical roots of a custom equation.
            It solves the given expression assuming that it is
            equal to zero. No need to make it equal to 0 yourself;
            however, if you prefer so, consider using the .Solve()
            method instead
            </summary>
            <param name=""x"">
            The variable over which to solve the equation
            </param>
            <example><code>
            Entity expr = ""x + 8 - 4"";
            Console.WriteLine(expr.SolveEquation(""x""));
            </code>
            Will print ""{ -4 }""
            </example>
            <returns>
            Returns <see cref=""T:AngouriMath.Entity.Set""/>
            </returns>
", "").Build());

return;*/

GenerateWikiToPages();
GeneratePagesFromDocs();
GenerateFinalWebsite();


static void GenerateWikiToPages()
{
    var contentFolder = Path.Combine(GENERATOR_PATH, "content");
    var allFiles = Directory.GetFiles(Path.Combine(contentFolder, "_wiki"));
    var destFolder = Path.Combine(contentFolder, "wiki");
    Directory.CreateDirectory(destFolder);
    var links = new List<(string url, string name)>();

    foreach (var file in allFiles)
    {
        var fileName = Path.GetFileName(file);
        var destPath = Path.Combine(destFolder, fileName);
        if (file.ToLower().EndsWith(".png"))
        {
            Console.WriteLine($"W: Image {file} copied");
            var newDest = Path.Combine(Path.GetDirectoryName(GENERATOR_PATH), "wiki", Path.GetFileName(file));
            if (File.Exists(newDest))
                File.Delete(newDest);
            File.Copy(file, newDest);
        }
        else if (file.EndsWith(".md"))
        {
            var nameOnly = fileName[..(fileName.Length - 3)];
            if (nameOnly.StartsWith("_"))
                continue;

            var rawTitle = nameOnly[(nameOnly.IndexOf('-') + 1)..];
            var title = rawTitle.Replace('-', ' ');

            var altName = destPath[..(destPath.Length - 2)] + "html";
            links.Add((url: nameOnly + ".html", name: title));
            Console.WriteLine($"W: {file} processing");
            File.WriteAllText(
                altName,
                MdToHtml(File.ReadAllText(file), title)
            );
            Console.WriteLine($"W: Written to {altName}");
        }
    }



    var sb = new StringBuilder(File.ReadAllText(Path.Combine(contentFolder, "_templates", "wiki.html")));
    sb.Append("<ul>");
    foreach (var (url, name) in links)
        sb.Append($"<li><a href='{url}'>{name}</a></li>");
    sb.Append("</ul>");
    sb.Append("<hr>");
    sb.Append($"Last update: [{DateTime.Now.ToUniversalTime()} UTC]");
    File.WriteAllText(Path.Combine(contentFolder, "wiki", "index.html"), sb.ToString());


    static string MdToHtml(string md, string title)
    {
        // 
        var sb = new StringBuilder();
        sb.Append($"<h2 class='centered'>{title}</h2><hr>");
        sb.Append("<p><a href='index.html'>&#8592; Back to the main page</a></p>");
        sb.Append($"{md}");
        sb.Replace("\r", "");
        sb.Replace("\n\n", "<br><br>");
        sb.Replace("<img ", "<img style='width: 100%'");
        return 
            Wrap("`", "`", "<text class='cw'>", "</text>",
                Wrap("```", "```", "<pre><code>", "</code></pre>",
                    Wrap("```cs", "```", "<pre><code>", "</code></pre>",
                        Wrap("```fs", "```", "<pre><code>", "</code></pre>",
                            sb.ToString()
                        )
                    )
                )
            );

        static string Wrap(string before, string after, string newBefore, string newAfter, string src)
        {
            var unjailed = DescriptionFromXmlBuilder.Unjail(before, after, src);
            if (unjailed is not { } notNull)
                return src;
            var (inside, first, last) = notNull;
            return src[..first] + newBefore + inside.Trim() + newAfter + Wrap(before, after, newBefore, newAfter, src[last..]);
        }
    }
}


static void GeneratePagesFromDocs()
{
    new WebsiteBuilder(
        // new PageSaver(@"D:\main\vs_prj\AngouriMath\tests\MyDocsTestDest")
        new PageSaver(@"D:\main\vs_prj\AngouriMath\AngouriMathSite\_generator\content\docs")
    )
    {
        MainPageName = "AngouriMath Almanac",
        MainPageDescription = @"<p>
Welcome to the storage of documentation for 
all public methods, properties and fields for AngouriMath!</p><p>
Please, consider these pages as those made for reference for particular members,
 it is not a guideline, manual, or tutorial.</p>
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
    var contentName = GENERATOR_PATH;

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
        if (file.StartsWith("_"))
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
        if (relativeName.Contains("docs"))
            newContent = newContent.Replace($"li><!--active_docs-->", "li class=\"active-link\">");
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