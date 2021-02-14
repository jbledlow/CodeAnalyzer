using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CodeAnalyzer;
using System.Collections.Generic;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class TokenSplitterTests
    {
        /*
        [TestMethod]
        public void TokenSplitter_ShouldReturnMatchingListForFunction()
        {
            string inputString = "public int GetSomething(string info, int number)\r\n \t{";
            List<string> correctSplit = new List<string> { "public", "int", "GetSomething", "(", "string", "info", "int", "number", ")", "{" };
            TokenSplitter ts = new TokenSplitter();
            var outputString = ts.splitString(inputString);
            outputString.ForEach(i => Console.Write("[{0}]\t", i));
            Console.WriteLine("output string has {0:N} elements", outputString.Count);
            CollectionAssert.AreEqual(outputString, correctSplit);
        }

        [TestMethod]
        public void TokenSplitter_ShouldReturnMatchingListForClass()
        {
            string inputString = "public class MyClass : YourClass, IYourInterface\r\n          {";
            List<string> correctSplit = new List<string> { "public", "class", "MyClass", ":", "YourClass", "IYourInterface", "{"};
            TokenSplitter ts = new TokenSplitter();
            var outputString = ts.splitString(inputString);
            outputString.ForEach(i => Console.Write("[{0}]\t", i));
            Console.WriteLine("output string has {0:N} elements", outputString.Count);
            CollectionAssert.AreEqual(outputString, correctSplit);
        }
        */
        [TestMethod]
        public void TokenSplitter_GetTokens_ShouldReturnMatchingClassStringSet ()
        {
            TokenSplitter ts = new TokenSplitter();
            string path = @"C:\Users\jonat\OneDrive\Documents\Courses\CSE 681 - Software Modeling\Projects\Project 2\source\CodeAnalyzer\Tests\CodeAnalyzerTests\TestFiles\splitterTestString.txt";
            List<string> correctSplit = new List<string> { "public", "class", "MyClass", ":", "YourClass", "IYourInterface", "{" };
            List<string> tokens = ts.GetTokens(path);
            tokens.ForEach(i => Console.Write("{0}, ", i));
            CollectionAssert.AreEqual(tokens, correctSplit);
            //Continue for the rest of the file
            correctSplit = new List<string> { "public", "int", "GetSomething", "(", "string", "info", "int", "number", ")", "{" };
            tokens = ts.GetTokens(path);
            tokens.ForEach(i => Console.Write("{0}, ", i));
            CollectionAssert.AreEqual(tokens, correctSplit);
            correctSplit = new List<string> { "-1" };
            tokens = ts.GetTokens(path);

            tokens.ForEach(i => Console.Write("{0}, ", i));
            CollectionAssert.AreEqual(tokens, correctSplit);

        }
    }
}
