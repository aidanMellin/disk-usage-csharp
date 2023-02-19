using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

/*
* Count number of directories, files, and size of given parent directory
* Similar to du command in Linux
*/
class DU{

    int countFolder;
    int countFile;
    long countByte;
    Stopwatch watch = new Stopwatch();
    string totalPrint;


    public DU(){
        countFolder = 0;
        countFile = 0;
        countByte = 0;
        totalPrint = "";
    }

    private void resetCount(){
        this.countFolder = 0;
        this.countFile = 0;
        this.countByte = 0;
    }

    /*
    * primary driver code
    */
    static void Main(string[] args){
        usage(args);
        string Path = args[1];
        DU diskUsage = new DU();

        DirectoryInfo di;
        diskUsage.totalPrint = $"Directory: '{Path}':\n\n";
        try{
            di = new DirectoryInfo(Path);

            //Execute code here
            char runType = args[0][1];
            diskUsage.execute(runType, di);
            
        }catch (DirectoryNotFoundException){
            Console.WriteLine("Directory Not Found");
            System.Environment.Exit(1);
        }
        Console.WriteLine(diskUsage.totalPrint);
    }

    /*
    * Determine if args passed match valid usage
    */
    static void usage(string[] args){
        if(args.Length != 2){
            Console.WriteLine("Usage: du [-s] [-p] [-b] <path>\nSummarize disk usage of the set of FILES, recursively for directories.\nYou MUST specify one of the parameters, -s, -p, or -b\n-s\tRun in single threaded mode\n-p\tRun in parallel mode (uses all available processors)\n-b\tRun in both parallel and single threaded mode\n\tRuns parallel followed by sequential mode");
            System.Environment.Exit(1);
        }else{
            char runType = args[0][1];
            string validArgs = "sdbp";
            if(!validArgs.Contains(runType)){
                Console.WriteLine("Usage: dotnet run -- [-s] [-p] [-b] <path>\nSummarize disk usage of the set of FILES, recursively for directories.\nYou MUST specify one of the parameters, -s, -p, or -b\n-s\tRun in single threaded mode\n-p\tRun in parallel mode (uses all available processors)\n-b\tRun in both parallel and single threaded mode\n\tRuns parallel followed by sequential mode");
                System.Environment.Exit(1);
            }
        }
    }

    /*
    * Return a formatted string with run time and counts, dependent on run if single or in parallel.
    */
    private string craftPrintString(string type){
        return $"{type} calculated in {this.watch.ElapsedMilliseconds}ms\n{this.countFolder} folders, {this.countFile} files, {this.countByte} bytes\n\n";
    }

    private void execute(char runType, DirectoryInfo di){
        if (runType == 'd'){
            runType = 'p';
        }
        bool both = runType == 'b';
    
        if(runType == 'p' || both){
            this.resetCount();
            this.watch = Stopwatch.StartNew();
            this.parRecurse(di);
            this.watch.Stop();
            this.totalPrint += this.craftPrintString("Parallel");
        }
        if (runType == 's' || both){
            this.resetCount();                
            this.watch = Stopwatch.StartNew();
            this.singRecurse(di);
            this.watch.Stop();
            this.totalPrint += this.craftPrintString("Single");
        }
    }    

    /*
    * Calculate total directories, count of files, and size of all files from a given path using a singular foreach
    * update passed reference parameters
    */
    private void singRecurse(DirectoryInfo dInfo){
        try{
            DirectoryInfo[] directories = dInfo.GetDirectories();
            foreach(DirectoryInfo d in directories){
                this.countFolder += 1;
                foreach(FileInfo f in d.GetFiles()){
                    this.countFile += 1;
                    this.countByte += f.Length;
                }
                singRecurse(d);
            }
        } catch (UnauthorizedAccessException){
            Console.WriteLine("You do not have access to this directory");
        }
    }

    /*
    * Calculate total directories, count of files, and size of all files from a given path using a parallel foreach
    * update passed reference parameters
    */
    private void parRecurse(DirectoryInfo dInfo){
        try{
            DirectoryInfo[] directories = dInfo.GetDirectories();
            Parallel.ForEach(directories, d => {
                this.countFolder += 1;
                foreach(FileInfo f in d.GetFiles()){
                    this.countFile += 1;
                    this.countByte += f.Length;
                }
                parRecurse(d);
            });
        } catch (UnauthorizedAccessException){
            Console.WriteLine("You do not have access to this directory");
        } catch (System.AggregateException e){
                e.Handle((x) =>{
                    if (x is UnauthorizedAccessException){
                        Console.WriteLine("You do not have permission to access all folders in this path.");
                        return true;
                    }
                    return false;
                    });
        }
    }
}