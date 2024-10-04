using hammerwatch_backupCore;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hammerwatch_backup
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }
        Program()
        {
            string sourceFolder = Config.SourceFolder();
            string destinationFolder = Config.DestinationFolder();
            string archiveFolder = Config.ArchiveFolder();

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
           
            if(!Directory.Exists(sourceFolder))
            {
                Console.WriteLine("Source folder doesnt exist.");
                Environment.Exit(0);
            }
            Console.WriteLine("If you are using this to backup a game save folder, you should probably disable cloud saves first.");

            while (true)
            {
                DirectoryInfo sourceFolderDI = new DirectoryInfo(sourceFolder);
                if (!sourceFolderDI.Exists)
                {
                    Thread.Sleep(Convert.ToInt32(Config.DelayBetweenChecks() * 1000.0));
                    continue;
                }
                var currentSourceFiles = sourceFolderDI.GetFiles("*", Config.BackupSubdirectories());
                
                DirectoryInfo destinationFolderDI = new DirectoryInfo(destinationFolder);
                var destinationFolderDirs = destinationFolderDI.GetDirectories("*", SearchOption.TopDirectoryOnly);
                bool MakeNewBackup = false;
                //no backups yet so assume its the first run. take a backup.
                if (destinationFolderDirs.Count() == 0)
                {
                    MakeNewBackup = true;
                }
                else
                {
                    //if only the archive folder in there, this can return null.
                    var latestBackupDir = destinationFolderDirs.Where(o => o.FullName.ToUpper() != archiveFolder.ToUpper()).ToList().OrderByDescending(o => o.LastWriteTime).ToList().FirstOrDefault();
                    if (latestBackupDir == null)
                    {
                        MakeNewBackup = true;

                    }
                    else
                    {
                        var latestBackupDirFiles = latestBackupDir.GetFiles("*", SearchOption.TopDirectoryOnly);
                        //loop through the current files, find the most recent backed up version of it, and if the current timestamp is newer than the most recent backup, make a new backup.
                        foreach (var sourceFile in currentSourceFiles)
                        {
                            var mostRecentBackupOfThisFile = latestBackupDirFiles.Where(o => o.Name == sourceFile.Name).SingleOrDefault();
                            if (mostRecentBackupOfThisFile == null)
                            {
                                //file is new. make a backup.
                                MakeNewBackup = true;
                                break;
                            }
                            else if (sourceFile.LastWriteTime != mostRecentBackupOfThisFile.LastWriteTime)
                            {
                                MakeNewBackup = true;
                                break;
                            }
                            //else, we already have the latest version of this file backed up.
                        }
                    }
                }

                if (MakeNewBackup)
                {
                    Console.WriteLine(DateTime.Now + ": Taking new backup.");
                    var newDir = Directory.CreateDirectory(Path.Combine(destinationFolder, DateTime.Now.ToString(Config.FolderDateTimeFormat())));
                    foreach (var sourceFile in currentSourceFiles)
                    {
                        var newFileLoc = Path.Combine(newDir.FullName, sourceFile.Name);
                        string newDirPath = Path.GetDirectoryName(newFileLoc)!;
                        if (!Directory.Exists(newDirPath))
                            Directory.CreateDirectory(newDirPath);
                        sourceFile.CopyTo(newFileLoc);
                    }
                    //take screenshot and put in dir. doing it twice because for some reason the first one sometimes is not current. might just be a hammerwatch thing. idk
                    if (Config.TakeScreenshotFlag() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        using (var image = ScreenCapturer.CaptureDesktop())
                        {
                            image.Save(Path.Combine(newDir.FullName, "SS.JPG"), ImageFormat.Jpeg);
                        }
                        Thread.Sleep(1000);
                        //take screenshot and put in dir
                        using (var image = ScreenCapturer.CaptureDesktop())
                        {
                            image.Save(Path.Combine(newDir.FullName, "SS2.JPG"), ImageFormat.Jpeg);
                        }
                    }
                    if (Config.CompressOldFolders())
                    {
                        
                        if (!Directory.Exists(archiveFolder))
                            Directory.CreateDirectory(archiveFolder);

                        //we just made a backup so this should never return nothing, but i'm putting the dirs into a list
                        //before filtering just because i know that won't error out if it is empty.
                        var dirsInBackupFolder = destinationFolderDI.GetDirectories("*",SearchOption.TopDirectoryOnly).ToList()
                                                                    .Where(o=>o.FullName.ToUpper() != archiveFolder.ToUpper()).ToList();
                        int folderCount = dirsInBackupFolder.Count();
                        int numDirsToArchive = folderCount - Config.MaxFoldersBeforeCompressing();
                        if (numDirsToArchive > 0)
                        {
                            var orderedDirs = dirsInBackupFolder.OrderBy(o => o.CreationTimeUtc).ToList();
                            var dirsToProcess = orderedDirs.Take(numDirsToArchive).ToList();
                            foreach (var dir in dirsToProcess)
                            {
                                string zipFilepath = Path.Combine(archiveFolder, dir.Name + ".zip");
                                if(File.Exists(zipFilepath))
                                {
                                    Console.WriteLine("Zip already exists... deleting it.");
                                    File.Delete(zipFilepath);
                                }
                                ZipFile.CreateFromDirectory(dir.FullName, zipFilepath, Config.CompressionLevel(), true);
                                Thread.Sleep(250);
                                try
                                {
                                    dir.Delete(true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to delete directory. Shit might get weird. {ex.Message}");
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(Convert.ToInt32(Config.DelayBetweenChecks() * 1000.0));
            }
        }
    }
}
