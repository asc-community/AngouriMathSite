using OutputDocsParser;
using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;
using System.Xml;

// 
// 
// var i = 0;
// 
// var members = new Dictionary<string, string>();
// 
// 
// 
// foreach (var s in members)
// {
//     Console.WriteLine(s.Key + "\n" + s.Value + "\n\n");
// }
// 
// static void ReadMembers(XmlReader reader, Dictionary<string, string> members)
// {
//     while (reader.Read() && reader.Name != "members")
//     {
//         if (reader.NodeType is XmlNodeType.Element && reader.Name == "member")
//         {
//             members[reader.GetAttribute("name")] = reader.ReadInnerXml();
//         }
//     }
// }
// 


var parsed = DocsParser.Parse(@"D:\main\vs_prj\AngouriMath\AngouriMath\Sources\AngouriMath\bin\Release\netstandard2.0\AngouriMath.xml");

parsed.Name = "AngouriMath Almanac";

new WebsiteBuilder(new PageSaver()).Build(parsed.Build());

public sealed class PageSaver : IPageSave
{
    public void Save(string path, string text)
    {
        // var finalPath = Path.Combine(@"D:\main\vs_prj\AngouriMath\tests\MyDocsTestDest", path);
        var finalPath = Path.Combine(@"D:\main\vs_prj\AngouriMath\AngouriMathSite\_generator\content\docs", path);
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
        File.WriteAllText(finalPath, text);
    }
}