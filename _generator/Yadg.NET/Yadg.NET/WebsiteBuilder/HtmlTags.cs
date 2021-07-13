using System.Collections.Generic;
using System.Linq;

namespace YadgNet
{
    internal static class HtmlTags
    {
        internal static string tag(string text, string tag)
           => HtmlTags.tag(text, tag, "");

        internal static string tag(string text, string tag, string @class)
            => $"<{tag} class=\"yadg-{tag} {@class}\">{text}</{tag}>";

        internal static string p(string text)
            => tag(text, "p");
        internal static string ul(IEnumerable<string> points)
            => tag(string.Join("", points.Select(c => li(c))), "ul");

        internal static string ul(string liClass, IEnumerable<string> points)
            => tag(string.Join("", points.Select(c => li(liClass, c))), "ul");

        internal static string li(string @class, string text)
            => tag(text, "li", @class);

        internal static string li(string text)
            => tag(text, "li");

        internal static string h1(string text)
            => tag(text, "h1");
        internal static string h2(string text)
            => tag(text, "h2");
        internal static string h2_centered(string text)
            => tag(text, "h2", "centered");
        internal static string h3(string text)
            => tag(text, "h3");
        internal static string h4(string text)
            => tag(text, "h4");
        internal static string div(string text, string @class)
            => tag(text, "div", @class);
        internal static string hr() => "<hr/>";
        internal static string a(string href, string text)
            => $"<a href=\"{href}\">{text}</a>";
        internal static string b(string text)
            => tag(text, "strong");
        internal static string cw(string text)
            => tag(text, "text", "cw");
    }
}
