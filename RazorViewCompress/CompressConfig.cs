using System.Web.Mvc;

namespace RazorViewCompress
{
    public class CompressConfig
    {
        public static void Config(bool removeWhiteSpaceInRazor, bool removeWhiteSpaceInHtml, bool zipHtml)
        {
            if (removeWhiteSpaceInRazor)
            {
                ViewEngines.Engines.Clear();
                ViewEngines.Engines.Add(new ViewEngineAgent());
            }
            if (removeWhiteSpaceInHtml || zipHtml)
            {
                GlobalFilters.Filters.Add(new CompressFilter(zipHtml, removeWhiteSpaceInHtml));
            }
        }
    }
}