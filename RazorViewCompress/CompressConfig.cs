using System.Web.Mvc;

namespace RazorViewCompress
{
    public class CompressConfig
    {
        public CompressConfig(bool removeWhiteSpaceInRazor, bool removeWhiteSpaceWhenReturn, bool zipResult)
        {
            if (removeWhiteSpaceInRazor)
            {
                ViewEngines.Engines.Clear();
                ViewEngines.Engines.Add(new ViewEngineAgent());
            }
            if (removeWhiteSpaceWhenReturn || zipResult)
            {
                GlobalFilters.Filters.Add(new CompressFilter(zipResult, removeWhiteSpaceWhenReturn));
            }
        }
    }
}
