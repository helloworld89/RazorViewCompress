using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorViewCompress
{
    public interface ICompressorFactory
    {
        BaseCompressor CreateCompressor();
    }
}
