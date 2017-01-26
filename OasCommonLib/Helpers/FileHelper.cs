namespace OasCommonLib.Helpers
{
    using Config;
    using Logger;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    //
    // last chance weapon to shadow file copy - https://github.com/alphaleonis/AlphaVSS
    //

    public class FileHelper
    {
        public static readonly string TAG = "FileHelper";
        public static readonly long MinimalLength = 500L;

        public static string LastError { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, int lpData, ref int pbCancel, uint dwCopyFlags);
        private delegate uint CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, uint dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

        public static bool CopyFileEx(string source, string destination, bool replace)
        {
//            if (!_cfg.IsAdmin)
            {
                return Copy(source, destination, replace);
            }

            //bool res = false;
            //try
            //{
            //    var oVSSImpl = VssUtils.LoadImplementation();

            //    using (var vss = oVSSImpl.CreateVssBackupComponents())
            //    {
            //        vss.InitializeForBackup(null);

            //        vss.SetBackupState(false, true, VssBackupType.Full, false);

            //        vss.GatherWriterMetadata();
            //        //using (var async = )
            //        //{
            //        //    async.Wait();
            //        //}

            //        vss.StartSnapshotSet();
            //        string volume = new FileInfo(source).Directory.Root.Name;
            //        var snapshot = vss.AddToSnapshotSet(volume, Guid.Empty);

            //        vss.PrepareForBackup();
            //        //using (var async = vss.PrepareForBackup())
            //        //    async.Wait();

            //        vss.DoSnapshotSet();
            //        //using (var async = vss.DoSnapshotSet())
            //        //    async.Wait();

            //        var props = vss.GetSnapshotProperties(snapshot);
            //        string vssFile = source.Replace(volume, props.SnapshotDeviceObject + @"");

            //        int cancel = 0;
            //        res = CopyFileEx(vssFile, destination, null, 0, ref cancel, 0);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _log.AddError(TAG, ex);
            //}

            //return res;
        }

        public static bool Copy(string from, string to, bool replace = false)
        {
            bool result = false;

            LastError = string.Empty;

            try
            {
                Thread.Sleep(100);
                using (var inputFile = new FileStream(from, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {

                    Thread.Sleep(50);
                    if (File.Exists(to))
                    {
                        if (!replace)
                        {
                            LastError = string.Format("file '{0}' already exists", to);
                            return result;
                        }
                        else
                        {
                            File.Delete(to);
                        }
                    }

                    Thread.Sleep(50);
                    using (var outputFile = new FileStream(to, FileMode.Create))
                    {
                        Thread.Sleep(50);
                        inputFile.CopyTo(outputFile, 0x10000);
                    }

                }
                result = true;
            }
            catch (Exception ex)
            {
                LastError = string.Format("file copy failed : {0}", ex.Message);
            }

            return result;
        }

        public static bool ParseName(string imageName, out int dbReference, out DateTime? dt)
        {
            string[] d = imageName.Split('-');

            dbReference = 0;
            dt = null;

            if (!int.TryParse(d[1], out dbReference))
            {
                return false;
            }

            int[] data = new int[6];
            int i;

            if (!int.TryParse(d[2].Substring(0, 4), out i))
            {
                return false;
            }
            data[0] = i;
            if (!int.TryParse(d[2].Substring(4, 2), out i))
            {
                return false;
            }
            data[1] = i;
            if (!int.TryParse(d[2].Substring(6, 2), out i))
            {
                return false;
            }
            data[2] = i;
            if (!int.TryParse(d[2].Substring(9, 2), out i))
            {
                return false;
            }
            data[3] = i;
            if (!int.TryParse(d[2].Substring(11, 2), out i))
            {
                return false;
            }
            data[4] = i;
            if (!int.TryParse(d[2].Substring(13, 2), out i))
            {
                return false;
            }
            data[5] = i;

            try
            {
                DateTime tmp = new DateTime(data[0], data[1], data[2], data[3], data[4], data[5]);
                dt = tmp;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool Move(string from, string to, bool replace = false)
        {
            bool result = false;

            LastError = string.Empty;

            try
            {
                using (var inputFile = new FileStream(
                     from,
                     FileMode.Open,
                     FileAccess.Read,
                     FileShare.ReadWrite))
                {
                    if (File.Exists(to))
                    {
                        if (!replace)
                        {
                            LastError = string.Format("file '{0}' already exists", to);
                            return result;
                        }
                        else
                        {
                            File.Delete(to);
                        }
                    }

                    using (var outputFile = new FileStream(to, FileMode.Create))
                    {
                        inputFile.CopyTo(outputFile, 0x10000);
                    }
                }

                File.Delete(from);
                result = true;
            }
            catch (Exception ex)
            {
                LastError = string.Format("file move failed : {0}", ex.Message);
                result = false;
            }

            return result;
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath) && Length(filePath) > MinimalLength;
        }

        public static bool CreateDirectoryRecursively(string path)
        {
            bool res = false;

            string[] pathParts = path.Split('\\');

            try
            {
                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (i > 0)
                    {
                        pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);
                    }
                    else if (i == 0 && pathParts[0][1] == ':' && pathParts[0].Length == 2)
                    {
                        pathParts[i] += "\\";
                    }

                    if (!Directory.Exists(pathParts[i]))
                    {
                        Directory.CreateDirectory(pathParts[i]);
                    }
                }

                res = true;
            }
            catch (Exception ex)
            {
                LastError = string.Format("error during creating apth '{0}' {1}", path, ex.Message);
            }

            return res;
        }

        public static bool FileLocked(string fileName)
        {
            try
            {
                using (Stream stream = new FileStream(fileName, FileMode.Open))
                {
                }
            }
            catch
            {
                return true;
            }

            return false;
        }

        public static long Length(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            long length = fi.Exists ? fi.Length : 0;
            fi = null;

            return length;
        }

        public static bool DeleteFile(string fullPath)
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                LastError = String.Format("error during file '{0}' deletion", ex.Message);
                return false;
            }

            return true;
        }

        public static bool MoveEmsToBackup(string emsFullPath)
        {
            bool res = false;
            string srcPath;
            string name;
            string trgPath;

            try
            {
                srcPath = Path.GetDirectoryName(emsFullPath);
                name = Path.GetFileNameWithoutExtension(emsFullPath);

                trgPath = Path.Combine(srcPath, "saved");
                OasConfig.CheckPath(trgPath);

                string[] caseFiles = Directory.GetFiles(srcPath, name + "*.*");
                foreach (var f in caseFiles)
                {
                    if (File.Exists(Path.Combine(trgPath, Path.GetFileName(f))))
                    {
                        File.Delete(Path.Combine(trgPath, Path.GetFileName(f)));
                    }
                    File.Move(f, Path.Combine(trgPath, Path.GetFileName(f)));
                }

                res = true;
            }
            catch (Exception ex)
            {
                LastError = String.Format("error during moving file '{0}' to backup", ex.Message);
            }

            return res;
        }

        public static bool MoveMcfToBackup(string mcfFullPath)
        {
            bool res = false;
            string srcPath;
            string name;
            string trgPath;

            try
            {
                srcPath = Path.GetDirectoryName(mcfFullPath);
                name = Path.GetFileNameWithoutExtension(mcfFullPath);

                trgPath = Path.Combine(srcPath, "saved");
                OasConfig.CheckPath(trgPath);

                if (File.Exists(Path.Combine(trgPath, name)))
                {
                    File.Delete(Path.Combine(trgPath, name));
                }
                Move(mcfFullPath, Path.Combine(trgPath, name), true);

                res = true;
            }
            catch (Exception ex)
            {
                LastError = String.Format("error during moving mcf '{0}' to backup", ex.Message);
            }

            return res;
        }
    }
}
