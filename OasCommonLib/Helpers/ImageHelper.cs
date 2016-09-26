namespace OasCommonLib.Helpers
{
    using Config;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class ImageHelper
    {
        private static readonly OasConfig _cfg = OasConfig.Instance;

        public static string ImageFolder()
        {
            var folder = _cfg.ImagePath;
            if (!Directory.Exists(folder))
            {
                FileHelper.CreateDirectoryRecursively(folder);
            }
            return folder;
        }

        public static string ImagePath(string imageName)
        {
            return Path.Combine(ImageFolder(), imageName);
        }

        public static bool IsImageMissing(string imageName)
        {
            var imagePath = ImagePath(imageName);
            bool fileExists = File.Exists(imagePath);
            bool fileHasCorrectSize = FileHelper.Length(imagePath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }

        public static string CaseImageFolder(long envelopeId)
        {
            var folder = Path.Combine(_cfg.CaseImagePath, envelopeId.ToString());
            if (!Directory.Exists(folder))
            {
                FileHelper.CreateDirectoryRecursively(folder);
            }
            return folder;
        }
        public static string CaseImagePath(long envelopeId, string imageName)
        {
            Debug.Assert(envelopeId > 0L);
            return Path.Combine(CaseImageFolder(Math.Abs(envelopeId)), imageName);
        }


        public static bool IsCaseImageMissing(long envelopeId, string imageName)
        {
            var imagePath = CaseImagePath(envelopeId, imageName);
            bool fileExists = File.Exists(imagePath);
            bool fileHasCorrectSize = FileHelper.Length(imagePath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }

        public static string CaseAudioFolder(long envelopeId)
        {
            var folder = Path.Combine(_cfg.CaseAudioPath, envelopeId.ToString());
            if (!Directory.Exists(folder))
            {
                FileHelper.CreateDirectoryRecursively(folder);
            }

            return folder;
        }

        public static string AudioPath(long envelopeId, string audioName)
        {
            var folder = CaseAudioFolder(envelopeId);
            return Path.Combine(folder, audioName);                
        }

        public static bool IsAudioMissing(long envelopeId, string audioName)
        {
            string audioPath = AudioPath(envelopeId, audioName);
            bool fileExists = File.Exists(audioPath);
            bool fileHasCorrectSize = FileHelper.Length(audioPath) > FileHelper.MinimalLength;
            return !(fileExists && fileHasCorrectSize);
        }
    }
}
