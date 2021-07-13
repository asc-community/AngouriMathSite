using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace YadgNet
{
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

            reader.Read();

            while (reader.Name != "members")
            {
                if (reader.EOF)
                    break;
                var attrValue = reader.GetAttribute("name");
                var innerXml = reader.ReadInnerXml();
                members[attrValue ?? throw new NullReferenceException()] = innerXml;
                if (reader.Name != "member")
                {
                    while (reader.Read() && reader.Name != "member") ;
                    reader.Read();
                }
            }

            return new DocAssemblyBuilder().Parse(members);
        }
    }
}
