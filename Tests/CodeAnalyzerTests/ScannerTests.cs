using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CodeAnalyzer;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class ScannerTests
    {
        [TestMethod]
        public void Scanner_IsToken_ShouldReturnTrueOnOpenCurlyBrace()
        {
            Scanner scanner = new Scanner();
            Assert.IsTrue(scanner.IsToken('{'));
        }

        [TestMethod]
        public void Scanner_IsToken_ShouldReturnTrueOnClosingCurlyBrace()
        {
            Scanner scanner = new Scanner();
            Assert.IsTrue(scanner.IsToken('}'));
        }

        [TestMethod]
        public void Scanner_IsToken_ShouldReturnFalseOnNewLine()
        {
            Scanner scanner = new Scanner();
            Assert.IsFalse(scanner.IsToken('\n'));
        }

        [TestMethod]
        public void Scanner_IsToken_ShouldReturnTrueOnSemiColon()
        {
            Scanner scanner = new Scanner();
            Assert.IsTrue(scanner.IsToken(';'));
        }

        [TestMethod]
        public void Scanner_GetNext_ShouldReturnClassSignature()
        {
            string filepath = @"C:\Users\jonat\OneDrive\Documents\Courses\CSE 681 - Software Modeling\Projects\Project 2\source\CodeAnalyzer\Tests\CodeAnalyzerTests\TestFiles\Class.txt";
            Scanner scanner = new Scanner();
            CollectionAssert.AreEqual(scanner.GetNext(filepath).ToCharArray(), "public class Scanner\r\n    {".ToCharArray());
        }

        [TestMethod]
        public void Scanner_GetNext_ShouldSkipInlineCommentAndReturnFunc()
        {
            string filepath = @"C:\Users\jonat\OneDrive\Documents\Courses\CSE 681 - Software Modeling\Projects\Project 2\source\CodeAnalyzer\Tests\CodeAnalyzerTests\TestFiles\inlineComment.txt";
            Scanner scanner = new Scanner();
            string tokens = scanner.GetNext(filepath);
            Console.WriteLine(tokens);
            Console.WriteLine("public void MyFunc() {");
            CollectionAssert.AreEqual(tokens.ToCharArray(), "public void MyFunc() {".ToCharArray());
        }


        [TestMethod]
        public void Scanner_GetNext_ShouldSkipBlockCommentAndReturnFunc()
        {
            string filepath = @"C:\Users\jonat\OneDrive\Documents\Courses\CSE 681 - Software Modeling\Projects\Project 2\source\CodeAnalyzer\Tests\CodeAnalyzerTests\TestFiles\blockComment.txt";
            Scanner scanner = new Scanner();
            string tokens = scanner.GetNext(filepath);
            Console.WriteLine(tokens);
            // will return the cr-nl chars
            CollectionAssert.AreEqual(tokens.ToCharArray(), "\r\npublic void MyFunc() {".ToCharArray());
        }

    }
}
