using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace RazorViewCompress
{
    internal class CompressFilter : ActionFilterAttribute
    {
        bool _zipHtml;
        bool _removeWhiteSpace;
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.Filter = null;
            }
        }
        public CompressFilter(bool zipHtml, bool removeWhiteSpace)
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
                return HanldePre(s, RemoveWhiteSpaces);
            });
        }
        private string RemoveWhiteSpaces(string s)
        {
            s = Regex.Replace(s, @">\s+<", "><");
            s = Regex.Replace(s, @"\r\n\s{0,}", " ");
            return s;
        }
        private string HanldePre(string s, Func<string, string> action)
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



        internal class WhiteSpaceFilter : Stream
        {
            private Stream _shrink;
            private Func<string, string> _filter;

            public WhiteSpaceFilter(Stream shrink, Func<string, string> filter)
            {
                _shrink = shrink;
                _filter = filter;
            }


            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return true; } }
            public override bool CanWrite { get { return true; } }
            public override void Flush() { _shrink.Flush(); }
            public override long Length { get { return 0; } }
            public override long Position { get; set; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                return _shrink.Read(buffer, offset, count);
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                return _shrink.Seek(offset, origin);
            }
            public override void SetLength(long value)
            {
                _shrink.SetLength(value);
            }
            public override void Close()
            {
                var s = sb.ToString();
                s = _filter(s);
                // write the data to stream 
                byte[] outdata = Encoding.UTF8.GetBytes(s);
                _shrink.Write(outdata, 0, outdata.GetLength(0));
                _shrink.Close();
            }
            private StringBuilder sb = new StringBuilder();
            public override void Write(byte[] buffer, int offset, int count)
            {
                // capture the data and convert to string 
                byte[] data = new byte[count];
                Buffer.BlockCopy(buffer, offset, data, 0, count);
                string s = Encoding.UTF8.GetString(data);
                sb = sb.Append(s);
                //var a = s.IndexOf((@"</html>"));
                //if (a > 0)
                //{
                //    s = s.Substring(0, s.IndexOf((@"</html>")) + 7);
                //    end = true;
                //}
                //// filter the string
                //s = _filter(s);
                //// write the data to stream 
                //byte[] outdata = Encoding.Default.GetBytes(s);
                //_shrink.Write(outdata, 0, outdata.GetLength(0));
            }
        }

    }
}
