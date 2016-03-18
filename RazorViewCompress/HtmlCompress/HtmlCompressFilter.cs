using System;
using System.IO.Compression;
using System.Web.Mvc;

namespace RazorViewCompress
{
    internal class HtmlCompressFilter : ActionFilterAttribute
    {
        private bool _zipHtml;
        private bool _removeWhiteSpace;

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.Filter = null;
            }
        }

        public HtmlCompressFilter(bool zipHtml, bool removeWhiteSpace)
        {
            _zipHtml = zipHtml;
            _removeWhiteSpace = removeWhiteSpace;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_zipHtml)
            {
                GZip(filterContext);
            }
            if (_removeWhiteSpace)
            {
                RemoveWhiteSpaces(filterContext);
            }
            EventHandler errorHandler = null;
            errorHandler = delegate
            {
                filterContext.HttpContext.Response.Filter = null;
                if (filterContext.HttpContext.ApplicationInstance != null)
                {
                    filterContext.HttpContext.ApplicationInstance.Error -= errorHandler;
                }
            };
            filterContext.HttpContext.ApplicationInstance.Error += errorHandler;
        }

        private void GZip(ActionExecutingContext filterContext)
        {
            var encodingsAccepted = filterContext.HttpContext.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(encodingsAccepted)) return;

            encodingsAccepted = encodingsAccepted.ToLowerInvariant();
            var response = filterContext.HttpContext.Response;

            if (encodingsAccepted.Contains("deflate"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            }
            else if (encodingsAccepted.Contains("gzip"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
        }

        private void RemoveWhiteSpaces(ActionExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            response.Filter = new WhiteSpaceFilter(response.Filter, s =>
            {
                ICompressorFactory compressorFactory = new HtmlCompressorFactory();
                BaseCompressor compressor = compressorFactory.CreateCompressor();
                return compressor.Compress(s);
            });
        }
    }
}