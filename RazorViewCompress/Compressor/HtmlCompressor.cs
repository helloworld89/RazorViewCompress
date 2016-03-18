using System.Text.RegularExpressions;

namespace RazorViewCompress
{
    internal class HtmlCompressor : BaseCompressor
    {
        public override string RemoveWhiteSpaces(string s)
        {
            s = Regex.Replace(s, @">\s+<", "><");
            s = Regex.Replace(s, @"\r\n\s{0,}", " ");
            return s;
        }
    }
}