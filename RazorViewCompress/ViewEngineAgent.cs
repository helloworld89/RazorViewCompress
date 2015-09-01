using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RazorViewCompress
{
    internal class ViewEngineAgent : IViewEngine
    {
        private readonly IViewEngine _innerViewEngine;
        public ViewEngineAgent()
        {
            _innerViewEngine = new RazorViewEngine();
        }
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            partialViewName = RazorCompressHelper.GetCompressedViewName(controllerContext, partialViewName);

            var result = _innerViewEngine.FindPartialView(controllerContext, partialViewName, useCache);
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

            var result = _innerViewEngine.FindView(controllerContext, viewName, masterName, useCache);
            return result;
        }
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            _innerViewEngine.ReleaseView(controllerContext, view);
        }
    }
}