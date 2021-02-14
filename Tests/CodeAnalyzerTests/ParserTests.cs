using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CodeAnalyzer;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parser_Parse_ShouldReadToEndOfFile ()
        {
            string path = @"C:\Users\jonat\OneDrive\Documents\Courses\CSE 681 - Software Modeling\Projects\Project 2\source\CodeAnalyzer";
            Parser parser = new Parser(path,"*.cs",false);
        }
        
    }
}
