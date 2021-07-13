using System;

namespace YadgNet
{
    public interface IPageSave
    {
        public void Save(string path, string text);
    }

    public sealed class WebsiteBuilder
    {
        public string MainPageName { get; set; } = "My Documentation";
        public string MainPageDescription { get; set; } = "Welcome to manual!";
        public string MainPageBottomText { get; set; } = $"<hr>Last update: [{DateTime.Now.ToUniversalTime()} UTC]. <i>Via <a href='https://github.com/WhiteBlackGoose/Yadg.NET'>Yadg.NET</a></i>.";
        public string BackToNamespacesButtonText { get; set; } = "&#8592; Back to list of namespaces";
        public string BackToClassesButtonText { get; set; } = "&#8592; Back to list of classes";

        private readonly IPageSave saver;
        public WebsiteBuilder(IPageSave saver)
            => this.saver = saver;

        internal static string? GetLinkWithinAsmByName(string cref)
            => (cref[..2], cref[2..]) switch
            {
                (_, var containsSystem) when NameParser.GetMethodName(cref).Contains("System")
                    => null,
                ("T:", var full)
                    => $"{NameParser.OneFoldBack(full)}/{NameParser.LastFold(full)}.html",
                ("P:", var full) when NameParser.OneFoldBack(full) is var classFullName
                    => $"{NameParser.OneFoldBack(classFullName)}/{NameParser.LastFold(classFullName)}.html#{NameParser.LastFold(full)}",
                ("F:", var full) when NameParser.OneFoldBack(full) is var classFullName
                    => $"{NameParser.OneFoldBack(classFullName)}/{NameParser.LastFold(classFullName)}.html#{NameParser.LastFold(full)}",
                ("M:", var full) when NameParser.OneFoldBack(full) is var classFullName
                    => $"{NameParser.OneFoldBack(classFullName)}/{NameParser.LastFold(classFullName)}.html#{NameParser.LastFold(NameParser.GetMethodName(full))}",
                _ => throw new InvalidOperationException()
            };

        public void Build(DocAssembly assembly)
        {
            saver.Save("namespaces.html", 
                new NamespaceListBuilder(assembly)
                { 
                    MainPageName = MainPageName,
                    MainPageDescription = MainPageDescription,
                    MainPageBottomText = MainPageBottomText
                }
                .Build()
                );

            foreach (var nspace in assembly.Namespaces)
                saver.Save(nspace.Name + ".html", 
                    new ClassListBuilder(nspace)
                    {
                        BackToNamespacesButtonText = BackToNamespacesButtonText
                    }.Build()
                    );

            foreach (var nspace in assembly.Namespaces)
                foreach (var cls in nspace.Classes)
                    saver.Save($"{nspace.Name}/{cls.Name}.html", 
                        new MemberListBuilder(cls)
                        {
                            BackToClassesButtonText = BackToClassesButtonText
                        }.Build(nspace.Name + ".html")
                        );
        }
    }
}
