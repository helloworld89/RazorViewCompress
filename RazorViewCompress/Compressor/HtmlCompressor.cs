using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RazorViewCompress
{
    public class HtmlCompressor : BaseCompressor
    {
        public override string RemoveWhiteSpaces(string s)
        {
            s = Regex.Replace(s, @">\s+<", "><");
            s = Regex.Replace(s, @"\r\n\s{0,}", " ");
            return s;
        }
    }
}
