/**
 * Display.cs
 * Original Author: Jonathan Ledlow
 * Date Last Modified: 17 Feb 2021
 * 
 * Summary:
 *      This package defines the display class that is used to format
 *      and output data to the console
 * 
 * 
 * Contents:
 *      - Display
 * 
 * Dependencies:
 *      - XML Library
 * 
 * 
 * Change History:
 *      - Added functions to format wrapped text so it prints properly
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace CodeAnalyzer
{
    /// <summary>
    /// Class to format and display analysis results to the console
    /// </summary>
    public class Display
    {
        private readonly int TOTALWIDTH = 120;//Console.WindowWidth;
        
        /// <summary>
        /// Print data to console.
        /// </summary>
        /// <param name="xdoc"></param>
        public void DisplayData(XmlDocument xdoc)
        {
            if (Program.AnalyzeClasses)
            {
                PrintClassRelationships(xdoc);
            }
            else
            {
                PrintFunctionAnalysis(xdoc);
            }
        }

        /// <summary>
        /// Format and print the functional analysis results
        /// </summary>
        /// <param name="xdoc"></param>
        private void PrintFunctionAnalysis(XmlDocument xdoc)
        {
            var filenames = xdoc.DocumentElement.ChildNodes;
            /// loop through files
            foreach (XmlNode fileNode in filenames)
            {
                Console.WriteLine("".PadLeft(TOTALWIDTH, '='));
                Console.WriteLine("Filename: {0}", fileNode.Attributes["id"].Value);
                Console.WriteLine("".PadLeft(TOTALWIDTH, '='));
                Console.WriteLine("This file has {0} Namespaces", fileNode.ChildNodes.Count);
                Console.WriteLine("This file has {0} Classes", fileNode.SelectNodes("descendant::Class").Count);
                Console.WriteLine("This file has {0} Functions\n", fileNode.SelectNodes("descendant::Function").Count);

                // loop through namespaces in the file node
                foreach (XmlNode namspaceNode in fileNode.ChildNodes)
                {
                    Console.WriteLine("Namespace: {0}", namspaceNode.Attributes["id"].Value);
                    // loop through classes in namespace node
                    foreach (XmlNode classNode in namspaceNode.ChildNodes)
                    {
                        Console.WriteLine("".PadLeft(TOTALWIDTH, '-'));
                        Console.WriteLine("\tClass: {0}", classNode.Attributes["id"].Value);
                        // loop through functions in class node
                        foreach (XmlNode functionNode in classNode)
                        {
                            Console.WriteLine("\t\tFunction: {0}", functionNode.Attributes["id"].Value);
                            Console.WriteLine("\t\t\tComplexity: {0}", functionNode.Attributes["Complexity"]?.Value);
                            Console.WriteLine("\t\t\tLine Count: {0}", functionNode.Attributes["Lines"]?.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Print class analysis results to the console
        /// </summary>
        /// <param name="xdoc"></param>
        private void PrintClassRelationships(XmlDocument xdoc)
        {
            var filenames = xdoc.DocumentElement.ChildNodes;
            // loop through file nodes
            foreach (XmlNode fileNode in filenames)
            {
                Console.WriteLine("".PadLeft(TOTALWIDTH, '='));
                Console.WriteLine("Filename: {0}", fileNode.Attributes["id"].Value);
                Console.WriteLine("".PadLeft(TOTALWIDTH, '='));
                Console.WriteLine("This file has {0} Namespaces", fileNode.ChildNodes.Count);
                Console.WriteLine("This file has {0} Classes", fileNode.SelectNodes("descendant::Class").Count);
                Console.WriteLine("This file has {0} Functions\n", fileNode.SelectNodes("descendant::Function").Count);
                // loop through namespace nodes in file node
                foreach (XmlNode namespaceNode in fileNode.ChildNodes)
                {
                    Console.WriteLine("Namespace: {0}", namespaceNode.Attributes["id"].Value);
                    // loop through class nodes in namespace node and print info
                    foreach (XmlNode classNode in namespaceNode.ChildNodes)
                    {
                        Console.WriteLine("".PadLeft(TOTALWIDTH, '-'));
                        Console.WriteLine("\tClass: {0}", classNode.Attributes["id"].Value);
                        Console.WriteLine("\t\tInheritance:");
                        printWrapped(classNode.Attributes["Inheritance"]?.Value, 24, TOTALWIDTH);
                        Console.WriteLine("\t\tAssociation:");
                        printWrapped(classNode.Attributes["Association"]?.Value, 24, TOTALWIDTH);
                        Console.WriteLine("\t\tUsing:");
                        printWrapped(classNode.Attributes["Using"]?.Value, 24, TOTALWIDTH);
                        Console.WriteLine("\t\tFunctions:");
                        StringBuilder sb = new StringBuilder();
                        foreach (XmlNode functionNode in classNode.ChildNodes)
                        {
                            sb.Append(functionNode.Attributes["id"].Value + ", ");
                        }
                        printWrapped(sb.ToString(), 24, TOTALWIDTH);
                    }
                }
            }
        }

        /// <summary>
        /// Format strings to print between specified columns and wrap overflowing words
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="columnStart"></param>
        /// <param name="columnEnd"></param>
        private void printWrapped(string inputString, int columnStart, int columnEnd)
        {
            if (inputString == null)
            {
                Console.WriteLine("None\n".PadLeft(columnStart + 5, ' '));
                return;
            }
            var width = columnEnd - columnStart;
            // split the string into lines that fit in width
            var lines = splitMaxWords(inputString, width);
            // print lines with padding to start on correct column
            foreach (var line in lines)
            {
                string paddedLine = line.PadLeft(columnStart + line.Length, ' ');
                Console.WriteLine(paddedLine);
            }
                Console.WriteLine("");
        }

        /// <summary>
        /// Split the words in the input string and recombine them into separate strings
        /// that fit within the width
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="maxWidth"></param>
        /// <returns>List of strings at or below max width</returns>
        private List<string> splitMaxWords(string inputString, int maxWidth)
        {
            StringBuilder sb = new StringBuilder();
            // split into separate words
            var words = inputString.Split(' ');
            List<string> lines = new List<string>();
            // loop through words
            foreach (var word in words)
            {
                // check if next word puts it over the width
                if ((sb.Length + word.Length + 1) < maxWidth )
                {
                    sb.AppendFormat("{0} ", word);
                }
                else
                {
                    // add line and start new
                    lines.Add(sb.ToString());
                    sb.Clear();
                    sb.AppendFormat("{0} ", word);
                }
            }
            // add last row of words
            lines.Add(sb.ToString());
            return lines;


        }
    }
}
