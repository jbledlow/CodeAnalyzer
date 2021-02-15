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
 * 
 * This package depends on the CodeAnalsis package that provides an interaface to 
 * the syntax detectors
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
       
        // what if there are overloaded??
        public Parser(string basePath, string pattern, bool recursive)
        {
            var dc = new DirectoryCrawler();
            _filePaths = dc.GetFiles(basePath, pattern, recursive);

            


            if (_filePaths.Count > 0)
            {
                AbstractDetectorFactory csFactory = new CSharpDetectorFactory();
                if (Program.AnalyzeClasses)
                {
                    baseDetector = csFactory.CreateClassScannerAnalysisDetectors().GetDetectorChain();
                    parseFiles();
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
            DataManager.OutputData();
        }

        private void parseFiles()
        {
            //Console.WriteLine(_filePaths.Count);
            foreach (var path in _filePaths)
            {
                Console.WriteLine("Parsing {0}", path);
                parse(path);
                DataManager.ResetData();
            }
        }

        public void parse(string path)
        {
            List<string> tokenList;
            while(!(tokenList = _tokenSplitter.GetTokens(path)).Contains( "-1"))
            {
                //Console.WriteLine
                baseDetector.DoTest(tokenList);
            }

            //Console.WriteLine("I have reached the end of this file!");
            
        }
    }

    /// <summary>
    /// The TokenSplitter class's responsiility is to request data from the scanner and split the
    /// resulting string into separate tokens for analysis
    /// </summary>
    public class TokenSplitter
    {
        private Scanner _scanner = new Scanner();
        public TokenSplitter()
        {

        }
        public List<string> GetTokens(string filePath)
        {
            //Console.WriteLine("Asking for tokens from {0}", filePath);
            var tokenString = _scanner.GetNext(filePath);
            return splitString(tokenString);
        }

        private List<string> splitString(string inputString)
        {
            List<string> outputString = new List<string>();
            var sb = new StringBuilder();
            foreach (var c in inputString)
            {
                switch (c)
                {
                    case ' ':
                    case ',':
                    case '\n':
                    case '\r':
                    case '\t':
                        if (sb.Length>0)
                        {
                            outputString.Add(sb.ToString());
                            sb.Clear();
                        }
                        continue;
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
        private List<char> tokens = new List<char> { ';', '{', '}' };
        private bool eof = false;

        // Gets the next candidate string
        public string GetNext(string filePath)
        {
            if (filePath != _currentPath)
            {
                //Console.WriteLine("Change to file {0}", filePath);
                eof = false;
                _streamReader = new StreamReader(filePath);
                _currentPath = filePath;
            }
            var sb = new StringBuilder();

            return ScanChars(sb);
        }

        private string ScanChars(StringBuilder sb)
        {
            char c;
            char b = ' '; // this is used to store the last character.
            // read character and check to see if is token
            while (!IsToken(c = (char)_streamReader.Read()) && eof == false)
            {
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
                    sb.Append(c);
                    b = c;
                }
                
                eof = _streamReader.EndOfStream;
            }

            if (eof)
            {
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
            // just keep reading until the matching quote is found
            while (_streamReader.Read() != c) ;
            //{
            //    _streamReader.Read();
            //}
            
            
        }

        // check to see if character is in the list of tokens.
        public bool IsToken(char c)
        {
            if (tokens.Contains(c))
            {
                return true;
            }
            return false;
        }

        // Skip all characters to end of comment
        public void SkipComment(char commentToken)
        {
            char c = (char)_streamReader.Read();
            char b = commentToken;

            if (commentToken == '/')
            {
                _streamReader.ReadLine();
                return;
            }
            while (!(b == '*' && c == '/'))
            {
                // rotate and fetch new
                b = c;
                c = (char)_streamReader.Read();
            }
        }
    }
}
