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

    Stopwatch watch = new Stopwatch();
    string totalPrint;

    (int folderCount, int fileCount, long byteCount) par;
    (int folderCount, int fileCount, long byteCount) sing;

    public DU(){
        totalPrint = "";
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
    private string craftPrintString(string type, (int countFolder, int countFile, long countByte) passedValues){
        return $"{type} calculated in {this.watch.ElapsedMilliseconds}ms\n{passedValues.countFolder} folders, {passedValues.countFile} files, {passedValues.countByte} bytes\n\n";
    }

    private void execute(char runType, DirectoryInfo di){
        if (runType == 'd'){
            runType = 'p';
        }
        bool both = runType == 'b';
    
        if(runType == 'p' || both){
            this.watch = Stopwatch.StartNew();
            this.calculateCountParallel(di);
            this.watch.Stop();
            this.totalPrint += this.craftPrintString("Parallel", this.par);
        }
        if (runType == 's' || both){
            this.watch = Stopwatch.StartNew();
            this.calculateCountNoParallel(di);
            this.watch.Stop();
            this.totalPrint += this.craftPrintString("Single", this.sing);
        }
    }    

    /*
    * Calculate total directories, count of files, and size of all files from a given path using a singular foreach
    * update counts in the sing tuple.
    *
    * As a side note, the code catches essentially two of the same errors, and this is because in testing on different platforms, I encountered different errors.
    */
    private void calculateCountNoParallel(DirectoryInfo dInfo){
        try{
            DirectoryInfo[] directories = dInfo.GetDirectories();
            FileInfo[] files = dInfo.GetFiles();

            foreach(DirectoryInfo d in directories){
                this.sing.folderCount += 1;
                foreach(FileInfo f in files){
                    this.sing.fileCount += 1;
                    this.sing.byteCount += f.Length;
                }
                calculateCountNoParallel(d);
            }
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

    /*
    * Calculate total directories, count of files, and size of all files from a given path using a parallel foreach
    * update passed reference parameters via Interlocked.
    *
    * As a side note, the code catches essentially two of the same errors, and this is because in testing on different platforms, I encountered different errors.
    */
    private void calculateCountParallel(DirectoryInfo dInfo){
        try{
            DirectoryInfo[] directories = dInfo.GetDirectories();
            FileInfo[] files = dInfo.GetFiles();

            Parallel.ForEach(directories, d => {
                Interlocked.Increment(ref this.par.folderCount);
                foreach(FileInfo f in files){
                    Interlocked.Increment(ref this.par.fileCount);
                    Interlocked.Add(ref this.par.byteCount, f.Length);
                }
                calculateCountParallel(d);
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