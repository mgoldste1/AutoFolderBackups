using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace hammerwatch_backupCore
{
    internal static class Config
    {
        public static bool TakeScreenshotFlag() => ConfigurationManager.AppSettings["AttemptScreenshots"].ToBool();
        public static SearchOption BackupSubdirectories()
        {
            if (ConfigurationManager.AppSettings["BackupSubdirectories"].ToBool())
                return SearchOption.AllDirectories;
            else
                return SearchOption.TopDirectoryOnly;
        }
        public static CompressionLevel CompressionLevel()
        {
            int cLevel = int.Parse(ConfigurationManager.AppSettings["CompressionLevel"]!);
            if (cLevel <= 1)
                return System.IO.Compression.CompressionLevel.SmallestSize;
            else if (cLevel == 2)
                return System.IO.Compression.CompressionLevel.Optimal;
            else if (cLevel == 3)
                return System.IO.Compression.CompressionLevel.Fastest;
            else
                return System.IO.Compression.CompressionLevel.NoCompression;
        }
        public static string SourceFolder() => ConfigurationManager.AppSettings["SourceDirectory"]!;
        public static string DestinationFolder() => ConfigurationManager.AppSettings["SaveDirectory"]!;
        public static string FolderDateTimeFormat() => ConfigurationManager.AppSettings["FolderDateTimeFormat"]!;

        public static double DelayBetweenChecks() => double.Parse(ConfigurationManager.AppSettings["CheckEveryXSeconds"]!);
        public static bool CompressOldFolders() => ConfigurationManager.AppSettings["CompressOldFolders"].ToBool();
        public static int MaxFoldersBeforeCompressing() => int.Parse(ConfigurationManager.AppSettings["MaxFoldersBeforeCompressing"]!);
        public static string ArchiveFolder() => ConfigurationManager.AppSettings["ArchiveFolder"]!;
    
    }
}
