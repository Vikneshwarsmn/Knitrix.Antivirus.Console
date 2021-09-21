using System;
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

    static void Main(string[] args)
    {
        Console.WriteLine("Scan Path: " + MALWARE_SAMPLES_PATH);
        

        

        // check paths
        //if (!File.Exists(scanPath) || !File.Exists(databasePath))
        //{
        //    Console.WriteLine("Scan path or database path is not exist! Exiting...");
        //    PrintExitMessage();
        //}

        // do scan
        try
        {
            // initialize libclamav
            PrintLog("Initializing libclamav...");
            ClamMain.Initialize();
            PrintLog("libclamav initialized.");

            // create new engine
            PrintLog("Creating new engine instance...");
            using (var engine = ClamMain.CreateEngine())
            {
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

                // scan the file
                PrintLog("Scanning file...");
                var result = engine.ScanFile(MALWARE_SAMPLES_PATH);
                PrintLog("SCAN FINISHED.");
                Console.WriteLine("Scanned:      " + result.Scanned);
                Console.WriteLine("IsVirus:      " + result.IsVirus);
                Console.WriteLine("MalwareName:  " + result.IsVirus);

                PrintLog("Releasing engine...");
            }
            PrintLog("Engine released.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(ex.ToString());
        }

        // exit
        Console.WriteLine();
        Console.WriteLine();
        PrintExitMessage();
    }

    static void PrintExitMessage()
    {
        Console.Write("Press anykey to exit...");
        Console.Read();
        Environment.Exit(0);
    }

    static void PrintLog(string message)
    {
        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}   {message}");
    }

    static string GetFilePath()
    {
        Console.WriteLine();
        Console.Write("   File path: ");
        var path = Console.ReadLine();
        Console.WriteLine();
        return path;
    }
}
