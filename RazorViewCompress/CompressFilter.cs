using System;
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
                White(filterContext);
            }
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
        private void White(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            response.Filter = new WhiteSpaceFilter(response.Filter, s =>
            {
                s = Regex.Replace(s, @">\s+<", "><");
                s = Regex.Replace(s, @"\r\n\s{0,}", " ");
                // single-line doctype must be preserved 
                var firstEndBracketPosition = s.IndexOf(">");
                //if (firstEndBracketPosition >= 0)
                //{
                //    s = s.Remove(firstEndBracketPosition, 1);
                //    s = s.Insert(firstEndBracketPosition, ">");
                //}
                return s;
            });
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
