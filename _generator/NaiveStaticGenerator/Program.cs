using NaiveStaticGenerator;
using System.Text;
using YadgNet;

// Those are in global usings in .NET 6 preview 7+, but by the moment
// of writing this code, the GitHub Actions CI only supported .NET 6
// preview 5, which yet didn't have global usings for system namespaces.
#pragma warning disable CS0105 // Using directive appeared previously in this namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#pragma warning restore CS0105 // Using directive appeared previously in this namespace

var GeneratorPath = GetNearestRoot("_generator", Directory.GetCurrentDirectory());
const string GeneratedPath = "generated";


GenerateWikiToPages();
GeneratePagesFromDocs();

GenerateFinalWebsite();

CopyWikiImagesToFinalWebsite();
CopyCssFilesToFinalWebsite();
CopyImgFolderToFinalWebsite();
CopyCName();

static string GetNearestRoot(string name, string current)
    => Path.GetFileName(current) == name ? current : GetNearestRoot(name, Path.GetDirectoryName(current));

void GenerateWikiToPages()
{
    var contentFolder = Path.Combine(GeneratorPath, "content");
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
            var newDest = Path.Combine(Path.GetDirectoryName(GeneratorPath), "wiki", Path.GetFileName(file));
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
            links.Add((url: nameOnly.Replace("#", "%23") + ".html", name: title));
            Console.WriteLine($"W: {file} processing");
            File.WriteAllText(
                altName,
                MdToHtml(File.ReadAllText(file), title)
            );
            Console.WriteLine($"W: Written to {altName}");
        }
    }



    var sb = new StringBuilder(File.ReadAllText(Path.Combine(contentFolder, "_templates", "wiki.html")));
    sb.Append("<ul class='wiki-ul-main'>");
    foreach (var (url, name) in links)
        sb.Append($"<li><a href='{url}'>{name}</a></li>");
    sb.Append("</ul>");
    sb.Append("<hr>");
    sb.Append($"Last update: [{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd mm:hh")} UTC]");
    sb.Append("</div>"); // this tag is opened in the template
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


void GeneratePagesFromDocs()
{
    new WebsiteBuilder(
        new PageSaver(Path.Combine(GeneratorPath, "content", "docs"))
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
            Path.Combine(GeneratorPath, "AngouriMath", "Sources", "AngouriMath", "bin", "release", "netstandard2.0", "AngouriMath.xml")
        ).Build()
    );
}



void GenerateFinalWebsite()
{
    var contentName = GeneratorPath;

    var rootName = Path.Combine(Path.GetDirectoryName(contentName), GeneratedPath);

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
        if (file.Contains("_templates") || file.Contains("_wiki"))
            continue;
        if (!file.EndsWith(".html"))
            continue;
        var content = File.ReadAllText(file);
        var relativeName = file.Substring(dirName.Length + 1);
        var relativePathName = Path.GetDirectoryName(relativeName).Replace('\\', '_');

        var newContent = top + "\n" + content + "\n" + bottom;

        #if DEBUG
        newContent = PathUp(newContent, relativeName, "themes.css");
        newContent = PathUp(newContent, relativeName, "styles.css");
        newContent = PathUp(newContent, relativeName, "img/icon_cropped.png");
        #endif

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

// Source:
// https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, Func<string, bool> fileToCopy)
{
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
    Directory.CreateDirectory(destDirName);

    // Get the files in the directory and copy them to the new location.
    FileInfo[] files = dir.GetFiles();
    foreach (FileInfo file in files)
    {
        if (!fileToCopy(file.Name))
            continue;
        string tempPath = Path.Combine(destDirName, file.Name);
        file.CopyTo(tempPath, true);
    }

    // If copying subdirectories, copy them and their contents to new location.
    if (copySubDirs)
    {
        foreach (DirectoryInfo subdir in dirs)
        {
            string tempPath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopy(subdir.FullName, tempPath, copySubDirs, fileToCopy);
        }
    }
}

void CopyWikiImagesToFinalWebsite()
{
    var root = Path.GetDirectoryName(GeneratorPath);
    var wikiImgPath = Path.Combine(root, "wiki");
    var wikiDestination = Path.Combine(root, GeneratedPath, "wiki");
    DirectoryCopy(wikiImgPath, wikiDestination, copySubDirs: false, _ => true);    
}

void CopyCssFilesToFinalWebsite()
{
    var root = Path.GetDirectoryName(GeneratorPath);
    var destination = Path.Combine(root, GeneratedPath);
    DirectoryCopy(root, destination, copySubDirs: false, f => Path.GetExtension(f) is ".css");
}

void CopyImgFolderToFinalWebsite()
{
    var root = Path.GetDirectoryName(GeneratorPath);
    var destination = Path.Combine(root, GeneratedPath, "img");
    DirectoryCopy(Path.Combine(root, "img"), destination, copySubDirs: false, _ => true);
}

void CopyCName()
{
    var root = Path.GetDirectoryName(GeneratorPath);
    File.Copy(Path.Combine(root, "CNAME"), Path.Combine(root, GeneratedPath, "CNAME"), true);
}
