using System.Web.Mvc;

namespace RazorViewCompress
{
    internal class ViewEngineAgent : IViewEngine
    {
        private readonly IViewEngine _razorViewEngine;

        public ViewEngineAgent()
        {
            _razorViewEngine = new RazorViewEngine();
        }

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            partialViewName = RazorCompressHelper.GetCompressedViewName(controllerContext, partialViewName);

            var result = _razorViewEngine.FindPartialView(controllerContext, partialViewName, useCache);
            return result;
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (masterName != null)
            {
                masterName = masterName == string.Empty ? "_Layout" : masterName;
            }
            masterName = RazorCompressHelper.GetCompressedViewName(controllerContext, masterName);
            viewName = RazorCompressHelper.GetCompressedViewName(controllerContext, viewName);

            var result = _razorViewEngine.FindView(controllerContext, viewName, masterName, useCache);
            return result;
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            _razorViewEngine.ReleaseView(controllerContext, view);
        }
    }
}