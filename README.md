# disk-usage-csharp
Recursively search sizes of directories, file counts, and directory counts in C#

**Extension of a project for RIT Computer Science Class - Concepts of Parallel and Distributed Systems**

## Intro
This was a rather simple project. Implement a du (disk usage command in Linux) alternative in .NET using C#.  
Big goals:
  - Count Folders at pointed path
  - Count number of files at pointed path
  - Measure the size of the directory
  - Have one feature with running on a single thread (use a regular ForEach statement), and one parallel (Parallel.ForEach)
  
Implementation of this was Object oriented with an emphasis on condensing features where possible. For implementing the -b flag, I deemed it better to simply execute both if statements after resetting the associated values.

## Usage
*Requires .NET 7.0 to run*

Usage: du [-s] [-p] [-b] <path>  
Summarize disk usage of the set of FILES, recursively for directories.  
You MUST specify one of the parameters, -s, -p, or -b  
  -s	Run in single threaded mode  
  -p	Run in parallel mode (uses all available processors)  
  -b	Run in both parallel and single threaded mode  
	Runs parallel followed by sequential mode  
