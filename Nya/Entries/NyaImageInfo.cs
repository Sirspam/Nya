using System.IO;

namespace Nya.Entries
{
    public sealed class NyaImageInfo
    {
        public byte[] ImageBytes { get; private set; }
        public string ImageUrl { get; private set; }

        public NyaImageInfo(byte[] imageBytes, string imageUrl)
        {
            ImageBytes = imageBytes;
            ImageUrl = imageUrl;
        }

        public bool IsAnimated()
        {
            return Path.GetExtension(ImageUrl).ToLower() == ".gif";
        }
        
        public string GetFileName()
        {
            return Path.GetFileName(ImageUrl);
        }
    }
}