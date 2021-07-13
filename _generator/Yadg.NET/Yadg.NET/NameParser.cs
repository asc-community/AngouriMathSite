namespace YadgNet
{
    public static class NameParser
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

        private static (string name, string pars) SplitMethodInto(string methodSignature)
        {
            string name;
            string pars;

            if (methodSignature.Contains('`'))
                name = methodSignature.Substring(0, methodSignature.IndexOf('`'));
            else if (methodSignature.Contains('('))
                name = methodSignature.Substring(0, methodSignature.IndexOf('('));
            else
                name = methodSignature;

            if (methodSignature.Contains('('))
                pars = methodSignature.Substring(methodSignature.IndexOf('('));
            else
                pars = "";

            return (name, pars);
        }

        public static string GetMethodName(string methodSignature)
            => SplitMethodInto(methodSignature).name;

        public static string GetMethodParams(string methodSignature)
            => SplitMethodInto(methodSignature).pars;
    }
}
