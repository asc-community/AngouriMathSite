using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OutputDocsParser
{
    public static class NamespaceParser
    {
        public static string OneFoldBack(string name)
        {
            if (name.Contains('('))
                name = name.Substring(0, name.IndexOf('('));
            return name.Substring(0, name.LastIndexOf('.'));
        }

        public static string LastFold(string name)
        {
            string withNoPss;
            if (name.Contains('('))
                withNoPss = name.Substring(0, name.IndexOf('('));
            else
                withNoPss = name;
            return name[(withNoPss.LastIndexOf('.') + 1)..];
        }
    }

    public sealed class DocAssemblyBuilder
    {
        public string Name { set; get; }

        public readonly SortedDictionary<string, DocNamespaceBuilder> Namespaces = new();

        public DocAssembly Build()
            => new DocAssembly(Name, 
                Namespaces.Select(c => c.Value.Build()));

        public DocNamespaceBuilder InsertNamespace(string name)
            => Namespaces.TryGetValue(name, out var res) ? res : Namespaces[name] = new() { Name=name };

        public DocAssemblyBuilder Parse(Dictionary<string, string> members)
        {
            foreach (var pair in members)
            {
                var techName = pair.Key;
                var text = pair.Value;
                if (techName.StartsWith("T:") && techName[2..] is var fullClassName)
                {
                    var namespaceName = NamespaceParser.OneFoldBack(fullClassName);
                    var className = NamespaceParser.LastFold(fullClassName);
                    InsertNamespace(namespaceName)
                        .InsertClass(className).Description = text;
                }
                // else
                // if (techName.StartsWith("P:") && techName[2..] is var fullPropertyName)
                // {
                //     var namespaceName = NamespaceParser.OneFoldBack(NamespaceParser.OneFoldBack(fullPropertyName));
                //     var className = NamespaceParser.LastFold(fullPropertyName);
                //     var propertyName = NamespaceParser.LastFold(fullPropertyName);
                //     InsertNamespace(namespaceName)
                //         .InsertClass(className)
                //             .InsertProperty(fullPropertyName, text);
                // }
                // else
                if (techName.StartsWith("M:") && techName[2..] is var fullMethodName)
                {
                    fullClassName = NamespaceParser.OneFoldBack(fullMethodName);
                    var namespaceName = NamespaceParser.OneFoldBack(fullClassName);
                    var className = NamespaceParser.LastFold(fullClassName);
                    var methodName = NamespaceParser.LastFold(fullMethodName);
                    InsertNamespace(namespaceName)
                        .InsertClass(className)
                            .InsertMethod(fullMethodName, text);
                }
            }

            return this;
        }
    }

    public sealed record DocAssembly(string Name, IEnumerable<DocNamespace> Namespaces);

    public sealed class DocNamespaceBuilder
    {
        public DocClassBuilder InsertClass(string name)
            => Classes.TryGetValue(name, out var res) ? res : Classes[name] = new() { Name=name };

        public string Name { get; set; }

        public SortedDictionary<string, DocClassBuilder> Classes { get; } = new();

        public DocNamespace Build() => new DocNamespace(Name, Classes.Select(c => c.Value.Build()));
    }

    public sealed record DocNamespace(string Name, IEnumerable<DocClass> Classes);

    public sealed class DocClassBuilder
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public SortedDictionary<string, DocMember> Members { get; } = new();

        public DocClass Build() => new DocClass(Name, Description, Members.Values);

        public DocProperty InsertProperty(string name, string description)
            => (DocProperty)(Members.TryGetValue(name, out var res) ? res : Members[name] = new DocProperty(name, description));

        public DocMethod InsertMethod(string name, string description)
            => (DocMethod)(Members.TryGetValue(name, out var res) ? res : Members[name] = new DocMethod(name, description));
    }

    public sealed record DocClass(string Name, string Description, IEnumerable<DocMember> Members);

    public abstract record DocMember;

    public sealed record DocMethod(string Name, string Description) : DocMember;

    public sealed record DocProperty(string Name, string Description) : DocMember;



    public static class DocsParser
    {
        public static DocAssemblyBuilder Parse(string path)
        {
            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true
            };
            using var fileStream = File.OpenText(path);
            using var reader = XmlReader.Create(fileStream, settings);

            while (reader.Read() && reader.Name != "members") { }

            var members = new Dictionary<string, string>();

            while (reader.Read() && reader.Name != "members")
            {
                var attrValue = reader.GetAttribute("name");
                var innerXml = reader.ReadInnerXml();
                members[attrValue] = innerXml;
                while (reader.Read() && reader.Name != "member");
            }

            return new DocAssemblyBuilder().Parse(members);
        }
    }
}
