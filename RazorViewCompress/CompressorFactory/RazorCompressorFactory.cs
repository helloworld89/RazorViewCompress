namespace RazorViewCompress
{
    internal class RazorCompressorFactory : ICompressorFactory
    {
        public BaseCompressor CreateCompressor()
        {
            return new RazorCompressor();
        }
    }
}