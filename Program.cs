/* Package Name: Program
 * Author: Jonathan Ledlow
 * Date: 31 January 2021
 * 
 * This package is the main entry point into program.
 * 
 * It is comprised of a single class:
 *      Program
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;



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

        private static string path;
        private static string pattern;
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
            // Configure app to take arguments
            var app = new CommandLineApplication();
            app.Name = "Code Analyzer";
            var options = app.Option("-o|--options <value>", "Command Line Options", CommandOptionType.MultipleValue);
            var fileArg = app.Option("-f|--file <value>", "Base File Path", CommandOptionType.SingleValue);
            var patternArg = app.Option("-p|--pattern", "File Extension Pattern", CommandOptionType.MultipleValue);
            // parse the args.

            app.OnExecute(() =>
            {
                // check options
                parseArgs(options, fileArg, patternArg);
                new Parser(path, pattern, IsRecursive);
                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static void parseArgs(CommandOption options, CommandOption fileArg, CommandOption patternArg)
        {
            if (options.HasValue())
            {
                parseOptions(options);
            }

            // check filename
            if (fileArg.HasValue())
            {
                path = fileArg.Value();
            }
            else
            {
                throw new ArgumentException("Filename was not provided.");
            }

            // chack patter(s)
            if (patternArg.HasValue())
            {
                pattern = "*.cs";
                foreach (var pattern in patternArg.Values)
                {

                }
            }
            else
            {
                throw new ArgumentException("File Pattern was not provided.");
            }
        }

        private static void parseOptions(CommandOption options)
        {
            foreach (var value in options.Values)
            {
                switch (value)
                {
                    case CLASSANALYSIS:
                        AnalyzeClasses = true;
                        Console.WriteLine("R specified");
                        continue;
                    case RECURSIVE:
                        IsRecursive = true;
                        Console.WriteLine("S specified");

                        continue;
                    case XMLOUTPUT:
                        WriteXMLFile = true;
                        Console.WriteLine("X specified");

                        continue;
                    default:
                        break;
                }
            }
        }

        private static void parseArgs(string[] args)
        {
            Console.WriteLine("Invalid");
                
        
        }

        private static void printHelp()
        {
            Console.WriteLine("Usage: code_analyzer [options] base_file_path file_extensions");
        }
    }
}
