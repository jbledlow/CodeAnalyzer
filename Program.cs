/**
 * Program.cs
 * This file is part of the CodeAnalyzer Project for CSE 681 Software Modeling
 * 
 * Summary:
 * 
 * Main Entry point to the code analyzer program
 * 
 * Contents:
 *      - Program
 *      
 * Dependencies:
 *      - CodeAnalysis.cs interfaces
 *      - DataManager
 *      - AbstractDetectorFactory
 * 
 */

using System;
using System.Collections.Generic;

namespace CodeAnalyzer
{
    /// <summary>
    /// Driver class to parse command line options and start application
    /// </summary>
    static class Program
    {
        private const string RECURSIVE = "/S";
        private const string CLASSANALYSIS = "/R";
        private const string XMLOUTPUT = "/X";
        // Static properties to hold analysis options
        public static bool IsRecursive { get; set; } = false;
        public static bool AnalyzeClasses { get; set; } = false;
        public static bool WriteXMLFile { get; set; } = false;

        private static string path = null;
        private static List<string> patterns = new List<string>();
        /// <summary>
        /// Main entry point to the program
        /// </summary>
        /// <param name="args">Is expecting a minimum of three pieces of two pieces of information.
        /// It needs a file path and a pattern for the files.
        /// It can take options which are used to set the above parameters</param>
        static void Main(string[] args)
        {
            // Make sure there are enough argument. Print help if not.
            if (args.Length < 3)
            {
                printHelp();
            }

            try
            {
                parseArgs(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error occurred while parsing the arguments: {0}", ex.Message);
                return;
            }

            Console.WriteLine("XML Option is {0}", WriteXMLFile);
            Console.WriteLine("Class option is {0}", AnalyzeClasses);
            Console.WriteLine("Recursive Search is {0}", IsRecursive);

            try
            {
                new Parser(path, "*.cs", IsRecursive);
            }
            catch (Exception ex)
            {
                printHelp();
                Console.Write("An exception from which the program could not recover has occurred: {0}", ex.Message);
            }
        }
        
        /// <summary>
        /// Parse the arguments passed to the program from the command line
        /// </summary>
        /// <param name="args"></param>
        private static void parseArgs(string[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "/R":
                    case "/r":
                        AnalyzeClasses = true;
                        i++;
                        continue;
                    case "/S":
                    case "/s":
                        IsRecursive = true;
                        i++;
                        continue;
                    case "/X":
                    case "/x":
                        WriteXMLFile = true;
                        i++;
                        continue;
                        // get filepath
                    case "/D":
                    case "/d":
                        i++;
                        CheckValid(args, i);
                        path = args[i];
                        i++;
                        continue;
                        // get patterns
                    case "/P":
                    case "/p":
                        i++;
                        CheckValid(args, i);
                        //look forward to see if there is another pattern arg
                        while (i < args.Length - 1 && !args[i + 1].StartsWith("/"))
                        {
                            patterns.Add(args[i]);
                            i++;
                        }
                        patterns.Add(args[i]);
                        i++;
                        continue;
                    default:
                        throw new ArgumentException("Unknown Argument: {0}", args[i]);
                }
            }
            ValidateParsedArgs();
        }

        /// <summary>
        /// Validate that we have a filename and override patterns
        /// </summary>
        private static void ValidateParsedArgs()
        {
            if (path == null)
            {
                throw new ArgumentException("A Directory Path must be provided");
            }
            patterns.Clear();
            patterns.Add("*.cs");
            Console.WriteLine("Patterns are currently disabled. Only *.cs files will be analyzed.");
        }

        /// <summary>
        /// Checks to make sure that the next argument after /D and /P are not other options
        /// </summary>
        /// <param name="args"></param>
        /// <param name="i"></param>
        private static void CheckValid(string[] args, int i)
        {
            if (args[i].StartsWith("/"))
            {
                throw new ArgumentException(String.Format("Invalid argument \"{0}\".", args[i]));

            }
        }

        /// <summary>
        /// Print help 
        /// </summary>
        private static void printHelp()
        {
            Console.WriteLine("Usage: code_analyzer [options: /S /R /X] /D base_file_path /P file_extensions");
        }
    }
}
