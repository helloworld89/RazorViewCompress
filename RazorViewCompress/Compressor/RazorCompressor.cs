using System.Text.RegularExpressions;

namespace RazorViewCompress
{
    internal class RazorCompressor : BaseCompressor
    {
        public override string RemoveWhiteSpaces(string fileContent)
        {
            Regex ra = new Regex(@"@:.*\r\n");
            var matcha = ra.Match(fileContent);
            if (matcha.Success)
            {
                fileContent = fileContent.Replace(matcha.Value, matcha.Value + "@/r/n@");
            }

            fileContent = Regex.Replace(fileContent, @">\s+<", "><");
            fileContent = Regex.Replace(fileContent, @"\r\n\s{0,}", " ");
            fileContent = Regex.Replace(fileContent, @"(@model[\t ]{1,}\S{1,})\s", "$1\r\n");

            //Regex r = new Regex(@"@model\s{1,}[0-9A-Za-z._]{1,}");
            //var match = r.Match(fileContent);
            //if (match.Success)
            //{
            //    fileContent = fileContent.Replace(match.Value, match.Value + "\r\n");
            //}

            //fileContent = fileContent.Replace("@/r/n@", "\r\n");
            return fileContent;
        }
    }
}