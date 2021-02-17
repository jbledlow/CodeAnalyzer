using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CodeAnalyzer;
using System.Collections.Generic;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class DisplayTests
    {
        [TestMethod]
        public void Display_SplitMaxWords_ShouldReturnProperSplit()
        {
            string inputString = "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute, System.ComponentModel.EditorBrowsableAttribute, ";
            PrivateObject privateObject = new PrivateObject(typeof(Display));
            List<string> output = privateObject.Invoke("splitMaxWords", inputString, 96) as List<string>;
            output.ForEach(x => Console.WriteLine(x));
        }
    }
}
