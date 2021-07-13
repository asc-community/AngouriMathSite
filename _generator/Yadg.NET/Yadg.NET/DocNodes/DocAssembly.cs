using System.Collections.Generic;
using System.Linq;

namespace YadgNet
{
    public sealed class DocAssemblyBuilder
    {
        public readonly SortedDictionary<string, DocNamespaceBuilder> Namespaces = new();

        public DocAssembly Build()
            => new DocAssembly(Namespaces.Select(c => c.Value.Build()).ToArray());

        public DocNamespaceBuilder InsertNamespace(string name)
            => Namespaces.TryGetValue(name, out var res) ? res : Namespaces[name] = new() { Name = name };

        public DocAssemblyBuilder Parse(Dictionary<string, string> members)
        {
            foreach (var pair in members)
            {
                var techName = pair.Key;
                var text = pair.Value;
                if (techName.StartsWith("T:") && techName[2..] is var fullClassName)
                {
                    var namespaceName = NameParser.OneFoldBack(fullClassName);
                    var className = NameParser.LastFold(fullClassName);
                    InsertNamespace(namespaceName)
                        .InsertClass(className).Description = text;
                }
                else
                if (techName.StartsWith("P:") && techName[2..] is var fullPropertyName)
                {
                    fullClassName = NameParser.OneFoldBack(fullPropertyName);
                    var namespaceName = NameParser.OneFoldBack(fullClassName);
                    var className = NameParser.LastFold(fullClassName);
                    var propertyName = NameParser.LastFold(fullPropertyName);
                    InsertNamespace(namespaceName)
                        .InsertClass(className)
                            .InsertProperty(propertyName)
                                .Description = text;
                }
                else
                if (techName.StartsWith("F:") && techName[2..] is var fullFieldName)
                {
                    fullClassName = NameParser.OneFoldBack(fullFieldName);
                    var namespaceName = NameParser.OneFoldBack(fullClassName);
                    var className = NameParser.LastFold(fullClassName);
                    var fieldName = NameParser.LastFold(fullFieldName);
                    InsertNamespace(namespaceName)
                        .InsertClass(className)
                            .InsertField(fieldName)
                                .Description = text;
                }
                else
                if (techName.StartsWith("M:") && techName[2..] is var fullMethodName)
                {
                    fullClassName = NameParser.OneFoldBack(fullMethodName);
                    var namespaceName = NameParser.OneFoldBack(fullClassName);
                    var className = NameParser.LastFold(fullClassName);
                    var methodSignature = NameParser.LastFold(fullMethodName);
                    var methodName = NameParser.GetMethodName(methodSignature);
                    var methodParams = NameParser.GetMethodParams(methodSignature);
                    InsertNamespace(namespaceName)
                        .InsertClass(className)
                            .InsertMethod(methodName)
                                .InsertOverload(methodParams, text);
                }
            }

            return this;
        }
    }

    public sealed record DocAssembly(IEnumerable<DocNamespace> Namespaces);

}
