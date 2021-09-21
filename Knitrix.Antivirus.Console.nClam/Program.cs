using System;
using System.Linq;
using nClam;
using System.IO;
using System.Diagnostics;
using Knitrix.Antivirus.Console.Utilities;

class Program
{
    private static ClamClient CLAM_CLIENT;
    private static readonly string MALWARE_SAMPLES_PATH = @"E:\Antivirus\malware-sample-library-master";
    private static long FILE_COUNT = 0;
    private static long CLEAN_FILES = 0;
    private static long INFECTED_FILES = 0;
    private static long ERROR_FILES = 0;
    private static long TOTAL_SCAN_SIZE = 0;

    static void Main(string[] args)
    {
        Console.Title = "Knitrix.Antivirus.Console";

        CLAM_CLIENT = new ClamClient("localhost", 3310);
        var pingResult = CLAM_CLIENT.PingAsync().Result;
        if (!pingResult)
        {
            Console.WriteLine("Scan Engine is Not Running. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Scan Path: " + MALWARE_SAMPLES_PATH);
        PrintLog("Scanning Started.");

        var watch = new Stopwatch();
        watch.Start();

        GetDirectoryReadyForScanning(MALWARE_SAMPLES_PATH);

        watch.Stop();

        Console.WriteLine("-------------------------------------------");
        Console.WriteLine("Scan Report");
        Console.WriteLine("Total Files Scanned: " + FILE_COUNT);
        Console.WriteLine("Total Clean Files: " + CLEAN_FILES);
        Console.WriteLine("Total Infected Files: " + INFECTED_FILES);
        Console.WriteLine("Total Faulty Files: " + ERROR_FILES);
        Console.WriteLine("Total Data Scanned: " +  Utilities.GetFileSize(TOTAL_SCAN_SIZE));
        Console.WriteLine("Total Execution Time: " + Utilities.GetTimeElapsed(watch.ElapsedMilliseconds));

        Console.ReadKey();
    }

    private static void GetDirectoryReadyForScanning(string folderPath)
    {
        // Start with drives if you have to search the entire computer.
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

        SearchDirectoryRecursive(dirInfo);
    }

    private static void SearchDirectoryRecursive(DirectoryInfo root)
    {
        DirectoryInfo[] subDirs = null;
        FileInfo[] files = null;

        // First, process all the files directly under this folder
        if (!((root.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint))
        {
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unauthorized Access " + root.FullName);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Directory Not Found " + root.FullName);
            }
            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    string fileShortDescription = Utilities.FileShortDescription(fi.FullName);
                    PrintLog("Current File: " + fileShortDescription);
                    ScanFile(fi.FullName);
                    Console.WriteLine(Environment.NewLine);
                    FILE_COUNT++;
                    TOTAL_SCAN_SIZE += fi.Length;
                }
            }
            // Now find all the subdirectories under this directory.
            try
            {
                subDirs = root.GetDirectories();
            }

            catch (UnauthorizedAccessException e)
            {

                Console.WriteLine("Unauthorized Access " + root.FullName);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Directory Not Found " + root.FullName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Other Error " + root.FullName + e.Message);
            }
            if (subDirs != null)
            {
                foreach (DirectoryInfo dirInfo in subDirs)
                {

                    try
                    {
                        SearchDirectoryRecursive(dirInfo);
                    }
                    catch (PathTooLongException ex)
                    {
                        Console.WriteLine(String.Format("Path too long for file name : {0}", dirInfo.Name));
                    }
                }
            }
        }
    }

    private static void ScanFile(string fileToScan)
    {
        var scanResult = CLAM_CLIENT.ScanFileOnServerAsync(fileToScan).Result;
        switch (scanResult.Result)
        {
            case ClamScanResults.Clean:
                Console.WriteLine("The file is clean!");
                CLEAN_FILES++;
                break;
            case ClamScanResults.VirusDetected:
                Console.WriteLine("Virus Found!");
                Console.WriteLine("Virus name: {0}", scanResult.InfectedFiles.First().VirusName);
                Utilities.QuarantineContainer(fileToScan);
                INFECTED_FILES++;
                break;
            case ClamScanResults.Error:
                Console.WriteLine("Woah an error occured! Error: {0}", scanResult.RawResult);
                ERROR_FILES++;
                break;
        }
    }

    private static void PrintLog(string message)
    {
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}   {message}");
    }
}