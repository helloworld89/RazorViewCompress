using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorViewCompress
{
    public abstract class BaseCompressor
    {
        public string Compress(string s)
        {
            return HanldePre(s, RemoveWhiteSpaces);
        }

        public abstract string RemoveWhiteSpaces(string fileContent);

        public string HanldePre(string s, Func<string, string> action)
        {
            List<string> pres = new List<string>();
            var preStartIndex = 0;
            var preEndIndex = 0;
            var preStartMark = "<pre>";
            var preEndMark = "</pre>";
            while (s.IndexOf(preStartMark, preEndIndex) > -1)
            {
                preStartIndex = s.IndexOf(preStartMark, preEndIndex);
                preEndIndex = s.IndexOf(preEndMark, preStartIndex) + 6;
                pres.Add(s.Substring(preStartIndex, preEndIndex - preStartIndex));
            }

            s = action(s);

            preStartIndex = 0;
            preEndIndex = 0;
            int commentIndex = 0;
            while (s.IndexOf(preStartMark, preEndIndex) > -1)
            {
                preStartIndex = s.IndexOf(preStartMark, preEndIndex);
                preEndIndex = s.IndexOf(preEndMark, preStartIndex) + 6;
                s = s.Replace(s.Substring(preStartIndex, preEndIndex - preStartIndex), string.Format("\r\n{0}\r\n", pres[commentIndex]));
                commentIndex++;
            }
            return s;
        }
    }
}
