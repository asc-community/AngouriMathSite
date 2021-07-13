using System.Linq;
using static YadgNet.HtmlTags;

namespace YadgNet
{
    public sealed class NamespaceListBuilder
    {
        public string MainPageName { get; set; } = "";
        public string MainPageDescription { get; set; } = "";
        public string MainPageBottomText { get; set; } = "";

        private readonly DocAssembly assembly;
        public NamespaceListBuilder(DocAssembly assembly)
            => this.assembly = assembly;

        public string Build()
            =>
            h2_centered(MainPageName) +

            hr() +

            p(MainPageDescription) +

            p(
                ul(
                    assembly.Namespaces.Select(
                        nspace => a(nspace.Name + ".html", nspace.Name)
                        )
                )
            )
            
            + MainPageBottomText;
    }
}
