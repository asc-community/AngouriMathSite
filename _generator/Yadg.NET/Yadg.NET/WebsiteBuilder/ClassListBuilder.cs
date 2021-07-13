using System.Linq;
using static YadgNet.HtmlTags;

namespace YadgNet
{
    public sealed class ClassListBuilder
    {
        public string BackToNamespacesButtonText { get; set; } = "&#8592;";
        private DocNamespace nspace;
        public ClassListBuilder(DocNamespace nspace)
            => this.nspace = nspace;

        public string Build()
            =>
            h2_centered(nspace.Name) +

            hr() +

            p(a("namespaces.html", "&#8592; Back to list of namespaces")) +

            p($"Classes within the {nspace.Name} namespace") +

            p(
                ul("yadg-class yadg-list",
                    nspace.Classes.Select(
                        cls
                            =>
                                (cls.Members.Count() > 0
                                ?
                                h3(a($"{nspace.Name}/{cls.Name}.html", cls.Name))
                                :
                                h3(cls.Name)
                                ) +
                                new DescriptionFromXmlBuilder(cls.Description, "").Build()
                    )
                )
            );
    }
}
