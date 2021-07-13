using System;
using System.Text;
using static YadgNet.HtmlTags;

namespace YadgNet
{
    public sealed class DescriptionFromXmlBuilder
    {
        private readonly string xml;
        private readonly string upPath;
        public DescriptionFromXmlBuilder(string xml, string upPath)
            => (this.xml, this.upPath) = (xml, upPath);        

        private static string ReplaceTag(string before, string tag, string src)
            => src
                .Replace($"<{tag}", $"{before}<div class='yadg-{tag}'")
                .Replace($"</{tag}>", $"</div>");

        public static (string inside, int firstId, int lastId)? Unjail(string before, string after, string src)
        {
            var id1 = src.IndexOf(before);
            if (id1 == -1)
                return null;
            var id2 = src.IndexOf(after, id1 + before.Length);
            if (id2 == -1)
                return null;
            var inside = src[(id1 + before.Length)..id2];
            return (inside, id1, id2 + after.Length);
        }

        private static string ReplaceAttributedOpenCloseTag(
            Func<string, string> attrDelegate,
            string tag, string attr, string src)
        {
            if (Unjail($"<{tag} {attr}=\"", "\"", src) is not { } pair)
                return src;
            var (inside, firstId, _) = pair;
            if (inside.Contains('>') || inside.Contains('<'))
                return src;
            var before = attrDelegate(inside);
            if (Unjail($"<{tag} {attr}=\"{inside}\">", $"</{tag}>", src) is not { } insideTextPair)
                return src;
            var (insideText, _, finalId) = insideTextPair;
            return src[..firstId] +
                before + div(insideText, $"yadg-{tag}") +
                ReplaceAttributedOpenCloseTag(attrDelegate,
                tag, attr, src[finalId..]);
        }

        private static string ReplaceAttributedCloseTag(
            Func<string, string> attrDelegate,
            string tag, string attr, string src)
        {
            if (Unjail($"<{tag} {attr}=\"", "\"/>", src) is not { } pair)
                return src;
            var (inside, firstId, lastId) = pair;
            if (inside.Contains('>') || inside.Contains('<'))
                return src[..(firstId + 2)] + ReplaceAttributedCloseTag(attrDelegate, tag, attr, src[(firstId + 2)..]); ;
            var replacement = attrDelegate(inside);
            return src[..firstId] + replacement +
                ReplaceAttributedCloseTag(attrDelegate, tag, attr, src[lastId..]);
        }

        private static string FindWhiteSpace(string src, int since)
        {
            var sb = new StringBuilder();
            while (since < src.Length && src[since] == ' ')
            {
                sb.Append(" ");
                since++;
            }
            return sb.ToString();
        }

        private static string N2Br(string src)
            => src.Replace("\n", "<br>").Replace("><br>", ">");

        private static string CodeToPreCodeAndN2Br(string src)
        {
            var unjailed = Unjail("<code>", "</code>", src);
            if (unjailed is not { } unjailedNotNull)
                return N2Br(src);
            var (inside, first, last) = unjailedNotNull;
            
            inside = inside.Replace("\n" + FindWhiteSpace(src, first + 7), "\n");
            if (inside.StartsWith('\n'))
                inside = inside[1..];
            if (inside.EndsWith('\n'))
                inside = inside.Substring(0, inside.Length - 1);
            return N2Br(src.Substring(0, first)) + $"<pre><code>{inside.Replace("<br>", "\n")}</code></pre>" + CodeToPreCodeAndN2Br(src.Substring(last));
        }

        public string Build()
            => 
            ReplaceTag(
                p(b("Example")),
                "example",
                ReplaceTag(
                    p(b("Returns")),
                    "returns",
                    ReplaceTag(
                        p(b("Summary")),
                        "summary",
                        ReplaceTag(
                            p(b("Remarks")),
                            "remarks",
                            ReplaceAttributedOpenCloseTag(
                                eName => p(b(
                                            "Exception \"" + 
                                         (
                                            WebsiteBuilder.GetLinkWithinAsmByName(eName) is { } link
                                            ?
                                            a(upPath + link, NameParser.LastFold(eName))
                                            :
                                            NameParser.LastFold(eName)
                                         ) + "\""
                                        )
                                    )
                                    ,
                                "exception",
                                "cref",
                                ReplaceAttributedOpenCloseTag(
                                    tName => p(b($"Type parameter \"{tName}\"")),
                                    "typeparam",
                                    "name",
                                    ReplaceAttributedOpenCloseTag(
                                        pName => p(b($"Parameter \"{pName}\"")),
                                        "param",
                                        "name",
                                        ReplaceAttributedCloseTag(
                                            langword => cw(langword),
                                            "see",
                                            "langword",
                                            ReplaceAttributedCloseTag(
                                                name => cw(name),
                                                "paramref",
                                                "name",
                                                ReplaceAttributedCloseTag(
                                                    cref =>
                                                        (
                                                            WebsiteBuilder.GetLinkWithinAsmByName(cref) is { } link
                                                            ?
                                                            a(upPath + link, NameParser.LastFold(cref))
                                                            :
                                                            NameParser.LastFold(cref)
                                                        ),
                                                    "see",
                                                    "cref",
                                                    ReplaceAttributedCloseTag(
                                                        href => a(href, href),
                                                        "a",
                                                        "href",
                                                        CodeToPreCodeAndN2Br(
                                                            xml
                                                            .Replace("\r", "")
                                                            .Replace("\" />", "\"/>")
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
    }
}
