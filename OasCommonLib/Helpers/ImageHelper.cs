namespace OasCommonLib.Helpers
{
    using Config;
    using System.IO;

    public class ImageHelper
    {
        private static readonly OasConfig _cfg = OasConfig.Instance;

        public static string CaseImageFolder()
        {
            return _cfg.CaseImagePath;
        }

        public static string ImageFolder()
        {
            return _cfg.ImagePath;
        }

        public static string CaseImagePath(string imageName)
        {
            return Path.Combine(CaseImageFolder(), imageName);
        }

        public static string ImagePath(string imageName)
        {
            return Path.Combine(ImageFolder(), imageName);
        }

        public static bool IsCaseImageMissing(string imageName)
        {
            var imagePath = CaseImagePath(imageName);
            bool fileExists = File.Exists(imagePath);
            bool fileHasCorrectSize = FileHelper.Length(imagePath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }

        public static bool IsImageMissing(string imageName)
        {
            var imagePath = ImagePath(imageName);
            bool fileExists = File.Exists(imagePath);
            bool fileHasCorrectSize = FileHelper.Length(imagePath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }

        public static bool IsAudioMissing(string audioName)
        {
            string audioPath = Path.Combine(_cfg.CaseAudioPath, audioName);
            bool fileExists = File.Exists(audioPath);
            bool fileHasCorrectSize = FileHelper.Length(audioPath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }
    }
}
