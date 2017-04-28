namespace OasCommonLib.Helpers
{
    using Config;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class AddInfoHelper
    {
        private static readonly OasConfig _cfg = OasConfig.Instance;

        public static string AddInfoFolder()
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
            return Path.Combine(AddInfoFolder(), imageName);
        }

        public static bool IsAddInfoMissing(string imageName)
        {
            var imagePath = ImagePath(imageName);
            return !FileHelper.Exists(imagePath);
        }

        public static string CaseAddInfoFolder(long envelopeId)
        {
            var folder = Path.Combine(_cfg.CaseImagePath, envelopeId.ToString());
            if (!Directory.Exists(folder))
            {
                FileHelper.CreateDirectoryRecursively(folder);
            }
            return folder;
        }
        public static string CaseAddInfoPath(long envelopeId, string imageName)
        {
            Debug.Assert(envelopeId > 0L);
            return Path.Combine(CaseAddInfoFolder(Math.Abs(envelopeId)), imageName);
        }


        public static bool IsCaseAddInfoMissing(long envelopeId, string imageName)
        {
            var imagePath = CaseAddInfoPath(envelopeId, imageName);
            return !FileHelper.Exists(imagePath);
        }
    }
}
