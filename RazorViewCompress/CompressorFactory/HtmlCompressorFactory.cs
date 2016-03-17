using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorViewCompress
{
    public class HtmlCompressorFactory : ICompressorFactory
    {
        public BaseCompressor CreateCompressor()
        {
            return new HtmlCompressor();
        }
    }
}
