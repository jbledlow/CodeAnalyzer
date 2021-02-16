using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeAnalyzer
{
    class Display
    {
        private readonly int TOTALWIDTH =   Console.WindowWidth;
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

        private void PrintFunctionAnalysis(XmlDocument xdoc)
        {
            var namespaces = xdoc.DocumentElement.ChildNodes;

            foreach (XmlNode namspaceNode in namespaces)
            {
                Console.WriteLine("Namespace: {0}", namspaceNode.Attributes["id"].Value);
                foreach (XmlNode classNode in namspaceNode.ChildNodes)
                {
                    
                    Console.WriteLine("".PadLeft(TOTALWIDTH, '-'));
                    Console.WriteLine("\tClass: {0}", classNode.Attributes["id"].Value);
                    foreach (XmlNode functionNode in classNode)
                    {
                        Console.WriteLine("\t\tFunction: {0}", functionNode.Attributes["id"].Value);
                        Console.WriteLine("\t\t\tComplexity: {0}", functionNode.Attributes["Complexity"]?.Value);
                        //printWrapped(functionNode.Attributes["Complexity"]?.Value, 24, TOTALWIDTH);
                        Console.WriteLine("\t\t\tLine Count: {0}", functionNode.Attributes["Lines"]?.Value);
                        //printWrapped(functionNode.Attributes["Lines"]?.Value, 24, TOTALWIDTH);
                    }


                }
            }
        }

        private void PrintClassRelationships(XmlDocument xdoc)
        {
            var namespaces = xdoc.DocumentElement.ChildNodes;

            foreach (XmlNode child in namespaces)
            {
                Console.WriteLine("Namespace: {0}", child.Attributes["id"].Value);
                foreach (XmlNode grandchild in child.ChildNodes)
                {
                    Console.WriteLine("".PadLeft(TOTALWIDTH, '-'));
                    Console.WriteLine("\tClass: {0}", grandchild.Attributes["id"].Value);
                    Console.WriteLine("\t\tInheritance:");
                    printWrapped(grandchild.Attributes["Inheritance"]?.Value, 24, TOTALWIDTH);
                    Console.WriteLine("\t\tAssociation:");
                    printWrapped(grandchild.Attributes["Association"]?.Value, 24, TOTALWIDTH);
                    Console.WriteLine("\t\tUsing:");
                    printWrapped(grandchild.Attributes["Using"]?.Value, 24, TOTALWIDTH);
                }
            }
        }

        private void printWrapped(string inputString, int columnStart, int columnEnd)
        {
            if (inputString == null)
            {
                Console.WriteLine("None\n".PadLeft(columnStart + 5, ' '));
                return;
            }
            var width = columnEnd - columnStart;
            var lines = splitMaxWords(inputString, width);
            foreach (var line in lines)
            {
                string paddedLine = line.PadLeft(columnStart + line.Length, ' ');
                Console.WriteLine(paddedLine);
            }
                Console.WriteLine("");
        }

        private List<string> splitMaxWords(string inputString, int maxWidth)
        {
            StringBuilder sb = new StringBuilder();
            var words = inputString.Split(' ');
            List<string> lines = new List<string>();
            foreach (var word in words)
            {
                if ((sb.Length + word.Length + 1) < maxWidth )
                {
                    sb.AppendFormat("{0} ", word);
                }
                else
                {
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
            }
            lines.Add(sb.ToString());
            return lines;


        }
    }
}
