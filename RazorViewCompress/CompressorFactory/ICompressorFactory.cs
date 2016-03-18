namespace RazorViewCompress
{
    internal interface ICompressorFactory
    {
        BaseCompressor CreateCompressor();
    }
}