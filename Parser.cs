/**
 * Parser.cs
 * This file is part of the CodeAnalyzer Project for CSE 681 Software Modeling
 * 
 * Summary:
 * 
 * This package provides classes for use in a code analysis program. The classes
 * contained herein are:
 *      - Parser
 *      - TokenSplitter
 *      - Scanner
 *      
 * Dependencies:
 *      - CodeAnalysis.cs interfaces
 *      - DataManager
 *      - AbstractDetectorFactory
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodeAnalyzer
{
    /// <summary>
    /// The Parser class's responsibility is to coordinate the data exchange between the TokenSplitter
    /// and Detector classes.
    /// </summary>
    public class Parser
    {
        private TokenSplitter _tokenSplitter = new TokenSplitter();
        private List<string> _filePaths;
        private IDetector baseDetector;
       
        public Parser(string filePath)
        {
            _filePaths = new List<string>() { filePath };
            AbstractDetectorFactory factory = new CSharpDetectorFactory();
            baseDetector = factory.CreateFunctionalAnalysisDetectors().GetDetectorChain();
            parseFiles();

            var saveDirectory = Path.GetDirectoryName(filePath);
            var outputName = Path.GetFileNameWithoutExtension(filePath) + ".xml";
            DataManager.OutputData(Path.Combine(saveDirectory, outputName));
        }

        /// <summary>
        /// Parser Constructor to initialize Parse and begin parsing
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="pattern"></param>
        /// <param name="recursive"></param>
        public Parser(string basePath, string pattern, bool recursive)
        {
            /// get files
            var dc = new DirectoryCrawler();
            _filePaths = dc.GetFiles(basePath, pattern, recursive);
            // make sure files were returned
            if (_filePaths.Count > 0)
            {
                Console.WriteLine("{0} Files were found.", _filePaths.Count);
                // get detectors and parse files
                AbstractDetectorFactory csFactory = new CSharpDetectorFactory();
                if (Program.AnalyzeClasses)
                {
                    // get list of classes first
                    baseDetector = csFactory.CreateClassScannerAnalysisDetectors().GetDetectorChain();
                    parseFiles();
                    // go back over for relationships
                    baseDetector = csFactory.CreateRelationshipAnalysisDetectors().GetDetectorChain();
                    parseFiles();
                }
                else
                {
                    baseDetector = csFactory.CreateFunctionalAnalysisDetectors().GetDetectorChain();
                    parseFiles();

                }
            }
            else Console.WriteLine("No Files were found!");
            Console.WriteLine("Parsing Has Concluded!");
            // output results
            DataManager.OutputData();
        }

        /// <summary>
        /// Parse Files in the current set of files
        /// </summary>
        private void parseFiles()
        {
            foreach (var path in _filePaths)
            {
                DataManager.SetCurrentFile(path);
                try
                {
                    parse(path);
                }
                catch (Exception)
                {
                    Console.WriteLine("An error occured when parsing file {0}", path);
                    Console.WriteLine("Skipping file.");
                }
                DataManager.ResetData();
            }
        }

        /// <summary>
        /// parse a file at the given path
        /// </summary>
        /// <param name="path">path to file</param>
        private void parse(string path)
        {
            List<string> tokenList;
            // Ask for tokens until the end of the file is reached
            while (!(tokenList = _tokenSplitter.GetTokens(path)).Contains("-1"))
            {
                baseDetector.DoTest(tokenList);
            }
        }
    }

    /// <summary>
    /// The TokenSplitter class's responsibility is to request data from the scanner and split the
    /// resulting string into separate tokens for analysis
    /// </summary>
    public class TokenSplitter
    {
        private Scanner _scanner = new Scanner();

        /// <summary>
        /// Obtain a set of tokens from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of tokens</returns>
        public List<string> GetTokens(string filePath)
        {
            var tokenString = _scanner.GetNext(filePath);
            return splitString(tokenString);
        }

        /// <summary>
        /// Split the string received from scanner into tokens
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private List<string> splitString(string inputString)
        {
            List<string> outputString = new List<string>();
            var sb = new StringBuilder();
            foreach (var c in inputString)
            {
                switch (c)
                {
                    // non-retained delimeters
                    case ' ':
                    case '\n':
                    case ',':
                    case '\r':
                    case '\t':
                        if (sb.Length>0)
                        {
                            // add token
                            outputString.Add(sb.ToString());
                            sb.Clear();
                        }
                        continue;
                    // Retained delimeters
                    case ')':
                    case '(':
                    case '{':
                    case '}':
                    case ':':
                    case ';':
                        if (sb.Length>0)
                        {
                            outputString.Add(sb.ToString());
                        }
                        outputString.Add(c.ToString());
                        sb.Clear();
                        continue;
                    default:
                        // all other chars
                        sb.Append(c);
                        continue;
                }
            }
            if (sb.Length > 0)
            {
                //append anything that was left
                outputString.Add(sb.ToString());
            }
            return outputString;
        }
    }

    /// <summary>
    /// The Scanner class is responsible for scanning the source code files character by character
    /// looking for the characters of interest
    /// </summary>
    public class Scanner
    {
        private StreamReader _streamReader;
        private string _currentPath;
        private List<char> stopChars = new List<char> { ';', '{', '}' };
        private bool eof = false;

        /// <summary>
        /// Get next set of characters up to a stopChar or end of file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetNext(string filePath)
        {
            // check to see if new file
            if (filePath != _currentPath)
            {
                eof = false;
                _streamReader = new StreamReader(filePath);
                _currentPath = filePath;
            }
            var sb = new StringBuilder();

            return ScanChars(sb);
        }

        /// <summary>
        /// Scan file character by character looking for stop characters
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        private string ScanChars(StringBuilder sb)
        {
            char c;
            char b = ' '; // this is used to store the last character.
            // read character and check to see if is token
            while (!IsToken(c = (char)_streamReader.Read()) && eof == false)
            {
                // check if a string
                if (c == '\x22' || c == '\x27')
                {
                    SkipString(c);
                    b = ' ';
                }
                // check if last two chars combine to // or /*
                else if (b == '/' && (c == '/' || c == '*'))
                {
                    sb.Remove(sb.Length - 1, 1);
                    // we only need to know what c is to check for inline vs block
                    SkipComment(c);
                    b = ' ';
                }
                else
                {
                    // increment count of lines if a new line char
                    if (c == '\n')
                    {
                        DataManager.AddLine();
                    }
                    sb.Append(c);
                    b = c;
                }
                eof = _streamReader.EndOfStream;
            }
            if (eof)
            {
                _streamReader.Close();
                // nothing more to be read from this file. Discard anthing left as it is not important
                return "-1";
            }
            //Need to append c as it is a token and ended the while loop
            return sb.Append(c).ToString();
        }

        /// <summary>
        /// Skip strings and chars, as they may contain the tokens we are looking for
        /// </summary>
        /// <param name="c">The type of quotation mark for which the method need to search.</param>
        private void SkipString(char c)
        {
            char b = ' ';
            // just keep reading until the matching quote is found
            while ((b = (char)_streamReader.Read()) != c)
            {
                // must skip over escaped quote characters
                if (b == '\\')
                {
                    _streamReader.Read();
                }
            }   
        }

        /// <summary>
        /// Test character against list of stop chars.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsToken(char c)
        {
            if (stopChars.Contains(c))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Skip characters until the end of comment is found
        /// </summary>
        /// <param name="commentToken"></param>
        private void SkipComment(char commentToken)
        {
            char c = (char)_streamReader.Read();
            char b = commentToken;
            // handle inline comments
            if (commentToken == '/')
            {
                _streamReader.ReadLine();
                return;
            }
            // handle block comments
            while (!(b == '*' && c == '/'))
            {
                // rotate and fetch new
                b = c;
                c = (char)_streamReader.Read();
            }
        }
    }
}
