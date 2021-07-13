using System;
using System.Collections.Generic;
using System.Linq;

namespace YadgNet
{
    public sealed class DocNamespaceBuilder
    {
        public DocClassBuilder InsertClass(string name)
            => Classes.TryGetValue(name, out var res) ? res : Classes[name] = new() { Name = name };

        public string? Name { get; set; }

        public SortedDictionary<string, DocClassBuilder> Classes { get; } = new();

        public DocNamespace Build() => new DocNamespace(Name ?? throw new NullReferenceException(), Classes.Select(c => c.Value.Build()).ToArray());
    }

    public sealed record DocNamespace(string Name, IEnumerable<DocClass> Classes);
}
