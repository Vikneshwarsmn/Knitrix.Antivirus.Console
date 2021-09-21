using System;
using System.IO;

namespace Knitrix.Antivirus.Console.Utilities
{
    public class Utilities
    {
        private static readonly string QUARANTINE_PATH = "Quarantine";
        public static string FileShortDescription(string fileName)
        {
            string fileShortDescription = fileName;
            if (fileName.Length > 50)
            {
                string firstTenCharacters = fileName.Substring(0, 24);
                string lastTenCharacters = fileName.Substring(fileName.Length - 24, 24);
                fileShortDescription = firstTenCharacters + "......" + lastTenCharacters;
            }
            return fileShortDescription;
        }

        public static string GetFileSize(double bytes)
        {
            string size = "0 Bytes";
            if (bytes >= 1073741824.0)
                size = string.Format("{0:##.##}", bytes / 1073741824.0) + " GB";
            else if (bytes >= 1048576.0)
                size = string.Format("{0:##.##}", bytes / 1048576.0) + " MB";
            else if (bytes >= 1024.0)
                size = string.Format("{0:##.##}", bytes / 1024.0) + " KB";
            else if (bytes > 0 && bytes < 1024.0)
                size = bytes.ToString() + " Bytes";

            return size;
        }

        public static string GetTimeElapsed(double milliSeconds)
        {
            string timeConsumed = "0 Seconds";
            if (milliSeconds >= (24 * 60 * 60 * 1000))
                timeConsumed = string.Format("{0:##.##}", milliSeconds / (24 * 60 * 60 * 1000)) + " Days";
            else if (milliSeconds >= (60 * 60 * 1000))
                timeConsumed = string.Format("{0:##.##}", milliSeconds / (60 * 60 * 1000)) + " Hours";
            else if (milliSeconds >= (60 * 1000))
                timeConsumed = string.Format("{0:##.##}", milliSeconds / (60 * 1000)) + " Minutes";
            else if (milliSeconds > 0 && milliSeconds < (60 * 1000))
                timeConsumed = string.Format("{0:##.##}", milliSeconds / 1000) + " Seconds";

            return timeConsumed;
        }

        public static void QuarantineContainer(string scannedFile)
        {
            try
            {
                string quarantineFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, QUARANTINE_PATH);

                if (!Directory.Exists(quarantineFolder))
                    Directory.CreateDirectory(quarantineFolder);

                string quarantinedFile = Path.Combine(quarantineFolder, Path.GetFileName(scannedFile));

                if (!File.Exists(quarantinedFile))
                    File.Copy(scannedFile, quarantinedFile);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
