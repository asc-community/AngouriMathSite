using System;
using System.Linq;
using static YadgNet.HtmlTags;


namespace YadgNet
{
    public sealed class MemberListBuilder
    {
        public string BackToClassesButtonText { get; set; } = "&#8592; Back to list of classes";
        private readonly DocClass cls;
        public MemberListBuilder(DocClass cls)
            => this.cls = cls;

        private static string SplitWrapWordsMethod(string method)
            => method.Replace(",", ", ").Replace("(", " (").Replace(".", ". ");

        public string Build(string back)
            =>
            h2_centered(cls.Name) +

            hr() +

            p(a($"../{back}", "&#8592; Back to list of classes")) +

            h2("Description") +

            new DescriptionFromXmlBuilder(cls.Description, "../").Build() +

            h2("Members") +

            p(
                ul("yadg-member yadg-list",
                    cls.Members.Select(
                        member
                            => member switch
                            {
                                DocMethod method =>
                                    $"<a name='{method.Name}'></a>" +
                                    (
                                    method.Overloads.Count() == 1

                                    ?

                                    p(
                                        $"{h3(SplitWrapWordsMethod(method.Name + method.Overloads.First().Parameters))} Method"
                                    ) +
                                    p(new DescriptionFromXmlBuilder(method.Overloads.First().Description, "../").Build())

                                    :

                                    p(
                                        $"{h3(method.Name)} Method and its overloads"
                                    ) +

                                    ul("yadg-list-2",
                                        method.Overloads.Select(
                                            overload =>
                                                h3(SplitWrapWordsMethod(method.Name + overload.Parameters)) +
                                                p(new DescriptionFromXmlBuilder(overload.Description, "../").Build())
                                        )
                                    )
                                    )
                                ,
                                DocProperty property =>
                                    $"<a name='{property.Name}'></a>" +
                                    p(
                                        $"{h3(property.Name)} Property"
                                    ) +
                                    p(new DescriptionFromXmlBuilder(property.Description, "../").Build()),
                                DocField field =>
                                    $"<a name='{field.Name}'></a>" +
                                    p(
                                        $"{h3(field.Name)} Field"
                                    ) +
                                    p(new DescriptionFromXmlBuilder(field.Description, "../").Build()),
                                _ => throw new Exception()
                            }
                    )
                )
            );
    }
}
