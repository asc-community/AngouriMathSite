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

var RootP = Path.GetDirectoryName(GetNearestRoot("src", Directory.GetCurrentDirectory()));
var GeneratorP = RootP._("src");
var OutputP = RootP._(".output");
var FinalOutputP = OutputP._("final");
var PreOutputP = OutputP._("pre");
var ContentP = GeneratorP._("content");
// const string 


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
    var allFiles = Directory.GetFiles(ContentP._("_wiki"));
    var destFolder = PreOutputP._("wiki");
    Directory.CreateDirectory(destFolder);
    
    var links = new List<(string url, string name)>();

    foreach (var file in allFiles)
    {
        var fileName = Path.GetFileName(file);
        var destPath = Path.Combine(destFolder, fileName);
        if (file.ToLower().EndsWith(".png"))
        {
            Console.WriteLine($"W: Image {file} copied");
            var newDest = Path.GetDirectoryName(GeneratorP)._("wiki")._(file);
            if (File.Exists(newDest))
                File.Delete(newDest);
            File.Copy(file, newDest);
        }
        else if (file.EndsWith(".md"))
        {
            var nameOnly = fileName[..^3];
            if (nameOnly.StartsWith("_"))
                continue;

            var rawTitle = nameOnly[(nameOnly.IndexOf('-') + 1)..];
            var title = rawTitle.Replace('-', ' ');

            var altName = destPath[..^2] + "html";
            links.Add((url: nameOnly.Replace("#", "%23") + ".html", name: title));
            Console.WriteLine($"W: {file} processing");
            File.WriteAllText(
                altName,
                MdToHtml(File.ReadAllText(file), title)
            );
            Console.WriteLine($"W: Written to {altName}");
        }
    }

    var sb = new StringBuilder(File.ReadAllText(ContentP._("_templates")._("wiki.html")));
    sb.Append("<ul class='wiki-ul-main'>");
    foreach (var (url, name) in links.OrderBy(l => l.url))
        sb.Append($"<li><a href='{url}'>{name}</a></li>");
    sb.Append("</ul>");
    sb.Append("<hr>");
    sb.Append($"Last update: [{DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")} UTC]");
    sb.Append("</div>"); // this tag is opened in the template
    File.WriteAllText(destFolder._("index.html"), sb.ToString());


    static string MdToHtml(string md, string title)
    {
        // 
        var sb = new StringBuilder();
        sb.Append($"<h2 class='centered'>{title}</h2><hr>");
        sb.Append("<p><a href='index.html'>&#8592; Back to the main page</a></p>");
        sb.Append($"{md}");
        sb.Replace("\r", "");
        // sb.Replace("\n\n", "<br><br>");
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
    var saver = new PageSaverAndCounter(PreOutputP._("docs"));
    new WebsiteBuilder(saver)
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
            Path.Combine(GeneratorP, "AngouriMath", "Sources", "AngouriMath", "AngouriMath", "bin", "release", "netstandard2.0", "AngouriMath.xml")
        ).Build()
    );
    Console.WriteLine($"The number of generated doc pages: {saver.PageSavedCount}");
}



void GenerateFinalWebsite()
{
    DirectoryCopy(ContentP, PreOutputP, copySubDirs: true, _ => true);

    var files = Directory.GetFiles(PreOutputP);
    
    var top = File.ReadAllText(ContentP._("_templates")._("top.html"));
    var bottom = File.ReadAllText(ContentP._("_templates")._("bottom.html"));

    var id = 0;
    var allFiles = Files.GatherFilesFromDir(PreOutputP);
    var count = allFiles.Count();

    bottom = bottom.Replace("<!--PAGE_COUNT-->", $"{count} pages online");
    
    foreach (var file in allFiles)
    {
        id++;
        if (file.Contains("_templates") || file.Contains("_wiki"))
            continue;
        if (!file.EndsWith(".html"))
            continue;
        var content = File.ReadAllText(file);
        var relativeName = file.Substring(PreOutputP.Length + 1);
        var relativePathName = Path.GetDirectoryName(relativeName)
            .Replace('\\', '_')
            .Replace('/', '_');

        var relativeNameSimple = relativeName.Replace(".html", "");

        var localTop = top.Replace("##page_title##", $"AngouriMath | {relativeNameSimple}");
        var newContent = localTop + "\n" + content + "\n" + bottom;

       //  #if DEBUG
       //  newContent = PathUp(newContent, relativeName, "themes.css");
       //  newContent = PathUp(newContent, relativeName, "styles.css");
       //  newContent = PathUp(newContent, relativeName, "img/icon_cropped.png");
       //  #endif

        newContent = newContent.Replace($"li><!--active_{relativePathName}-->", "li class=\"active-link\">");
        if (relativeName.Contains("docs"))
            newContent = newContent.Replace($"li><!--active_docs-->", "li class=\"active-link\">");
        newContent = newContent.Replace("<tbody>", "<pre>");

        var newFilePath = Path.Combine(FinalOutputP, relativeName);
        var n = $"[{id} / {count}]";
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
    var root = Path.GetDirectoryName(GeneratorP);
    var wikiImgPath = Path.Combine(root, "wiki");
    var wikiDestination = Path.Combine(root, OutputP, "wiki");
    DirectoryCopy(wikiImgPath, wikiDestination, copySubDirs: false, _ => true);    
}

void CopyCssFilesToFinalWebsite()
{
    var root = Path.GetDirectoryName(GeneratorP);
    var destination = Path.Combine(root, OutputP);
    DirectoryCopy(root, destination, copySubDirs: false, f => Path.GetExtension(f) is ".css");
}

void CopyImgFolderToFinalWebsite()
{
    var root = Path.GetDirectoryName(GeneratorP);
    var destination = Path.Combine(root, OutputP, "img");
    DirectoryCopy(Path.Combine(root, "img"), destination, copySubDirs: false, _ => true);
}

void CopyCName()
{
    var root = Path.GetDirectoryName(GeneratorP);
    File.Copy(Path.Combine(root, "CNAME"), Path.Combine(root, OutputP, "CNAME"), true);
}


public sealed class PageSaverAndCounter : IPageSave
{
    private readonly string rootPath;
    
    public int PageSavedCount { get; private set; } = 0;
    
    public PageSaverAndCounter(string path)
        => this.rootPath = path;
        
    public void Save(string path, string text)
    {
        var finalPath = Path.Combine(rootPath, path) ?? throw new NullReferenceException();
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath) ?? throw new NullReferenceException());
        File.WriteAllText(finalPath, text);
        Console.WriteLine($"Writing to {finalPath}");
        PageSavedCount++;
    }
}

public static class Extensions
{
    public static string _(this string a, string b) => Path.Combine(a, b);
}