namespace OasCommonLib.Helpers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using Logger;
    using Config;

    public class OASImage
    {
        public string Folder { get; private set; }
        public string FileName { get; private set; }
        public bool IsDirectory { get; private set; }

        public string PathToFile
        {
            get
            {
                return Path.Combine(Folder, FileName);
            }
        }

        public OASImage(string folder, string fileName, bool isDir = false)
        {
            if (isDir)
            {
                Folder = folder;
            }
            else
            {
                Folder = Path.GetDirectoryName(folder) + "\\";
            }
            FileName = fileName;
            IsDirectory = isDir;
        }

        public OASImage(string fullFileName, bool isDir = false)
        {
            if (isDir)
            {
                Folder = fullFileName;
                FileName = string.Empty;
            }
            else
            {
                Folder = Path.GetDirectoryName(fullFileName) + "\\";
                FileName = Path.GetFileName(fullFileName);
            }
            IsDirectory = isDir;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is OASImage)
            {
                if (((OASImage)obj).IsDirectory == this.IsDirectory &&
                    ((OASImage)obj).FileName.Equals(this.FileName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void FixFileName(char p1, char p2)
        {
            FileName = FileName.Replace(p1, p2);
        }
    }

    public class CaseImage
    {
        public string Image { get; set; }
        public bool Found { get; set; }

        public CaseImage(string imageName)
        {
            Image = imageName;
            Found = false;
        }
    }

    public static class Images
    {
        public readonly static string TAG = "Images";

        private readonly static LogQueue _log = LogQueue.Instance;
        private readonly static OasConfig _cfg = OasConfig.Instance;

        public static List<OASImage> GetImageList(string path, string exts)
        {
            List<OASImage> imageList = GetImageListFromDirectory(path, exts.ToLower());

            return imageList;
        }

        private static List<OASImage> GetImageListFromDirectory(string path, string exts)
        {
            List<OASImage> imageList = new List<OASImage>();
            string[] extension = exts.ToLower().Split(';');

            var list = Directory.GetFiles(path);
            foreach (var file in list)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (extension.Contains(ext))
                {
                    imageList.Add(new OASImage(path, Path.GetFileName(file)));
                }
            }

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                imageList.AddRange(GetImageListFromDirectory(dir, exts));
            }

            return imageList;
        }

        public static int RemoveUnusedImages(long envelopeId, string caseName)
        {
            int removedImages = 0;

            var caseFolder = ImageHelper.CaseImageFolder(envelopeId);
            var imageList = GetImageList(caseFolder, _cfg.ImageExts).Select(x => new CaseImage(ImageHelper.CaseImagePath(envelopeId, x.FileName))).ToList();
            foreach (var il in imageList)
            {
                if (il.Image.StartsWith(caseName))
                {
                    il.Found = true;
                }
            }

            foreach (var il in imageList)
            {
                if (!il.Found)
                {
                    if (FileHelper.DeleteFile(ImageHelper.CaseImagePath(envelopeId, il.Image)))
                    {
                        ++removedImages;
                    }
                    else
                    {
                        _log.Add(TAG, FileHelper.Error);
                    }
                }
            }

            return removedImages;
        }
    }
}
