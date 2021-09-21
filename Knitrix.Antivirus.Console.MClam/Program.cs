using System;
using System.Diagnostics;
using System.IO;
using Knitrix.Antivirus.Console.Utilities;
using MClam;

class Program
{
    private static readonly string MALWARE_SAMPLES_PATH = @"E:\Antivirus\malware-sample-library-master";
    private static readonly string DATABASE_PATH = @"E:\Antivirus\clamav\db";
    private static long FILE_COUNT = 0;
    private static long CLEAN_FILES = 0;
    private static long INFECTED_FILES = 0;
    private static long ERROR_FILES = 0;
    private static long TOTAL_SCAN_SIZE = 0;
    private static ClamEngine engine;

    static void Main(string[] args)
    {
        Console.WriteLine("Scan Path: " + MALWARE_SAMPLES_PATH);

        var watch = new Stopwatch();
        watch.Start();

        try
        {
            // initialize libclamav
            PrintLog("Initializing libclamav...");
            ClamMain.Initialize();
            PrintLog("libclamav initialized.");

            // create new engine
            PrintLog("Creating new engine instance...");
            engine = ClamMain.CreateEngine();
            PrintLog("Engine instance is created.");

            // load database
            PrintLog("Loading database...");
            engine.Load(DATABASE_PATH);
            PrintLog("Database loaded.");

            // compile engine
            PrintLog("Compiling engine...");
            engine.Compile();
            PrintLog("Engine compiled.");

            PrintLog("Scanning Started.");

            GetDirectoryReadyForScanning(MALWARE_SAMPLES_PATH);

            PrintLog("SCAN FINISHED.");

            PrintLog("Releasing engine");
            PrintLog("Engine released.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(ex.ToString());
        }

        watch.Stop();

        Console.WriteLine("-------------------------------------------");
        Console.WriteLine("Scan Report");
        Console.WriteLine("Total Files Scanned: " + FILE_COUNT);
        Console.WriteLine("Total Clean Files: " + CLEAN_FILES);
        Console.WriteLine("Total Infected Files: " + INFECTED_FILES);
        Console.WriteLine("Total Faulty Files: " + ERROR_FILES);
        Console.WriteLine("Total Data Scanned: " + Utilities.GetFileSize(TOTAL_SCAN_SIZE));
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
        var scanResult = engine.ScanFile(MALWARE_SAMPLES_PATH);

        if (scanResult.IsVirus)
        {
            Console.WriteLine("Virus Found!");
            Console.WriteLine("Virus name: {0}", scanResult.MalwareName);
            Utilities.QuarantineContainer(fileToScan);
            INFECTED_FILES++;
        }
        else
        {
            Console.WriteLine("The file is clean!");
            CLEAN_FILES++;
        }
    }

    static void PrintLog(string message)
    {
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}   {message}");
    }
}
