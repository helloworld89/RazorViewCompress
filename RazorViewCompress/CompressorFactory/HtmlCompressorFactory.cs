namespace RazorViewCompress
{
    internal class HtmlCompressorFactory : ICompressorFactory
    {
        public BaseCompressor CreateCompressor()
        {
            return new HtmlCompressor();
        }
    }
}