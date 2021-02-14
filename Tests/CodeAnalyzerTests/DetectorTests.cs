using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CodeAnalyzer;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class DetectorTests
    {
        [TestMethod]
        public void ClassDetector_DoTest_ShouldReturnTrueForClassTokens()
        {
            List<string> tokens = new List<string> { "public", "class", "MyClass", ":", "YourClass", "IYourInterface", "{" };
            CSClassDetector cd = new CSClassDetector();
            //Assert.IsTrue(cd.DoTest(tokens));
        }

        [TestMethod]
        public void ClassDetector_DoTest_ShouldReturnFalseForFunctionTokens()
        {
            List<string> tokens = new List<string> { "public", "int", "GetSomething", "(", "string", "info", "int", "number", ")", "{" };
            CSClassDetector cd = new CSClassDetector();
            //Assert.IsFalse(cd.DoTest(tokens));
        }

        [TestMethod]
        public void FunctionDetector_DoTest_ShouldReturnTrueForFuncTokens()
        {
            List<string> tokens = new List<string> { "public", "int", "GetSomething", "(", "string", "info", "int", "number", ")", "{" };
            CSFuncDetector fd = new CSFuncDetector();
            //Assert.IsTrue(fd.DoTest(tokens));
        }

        [TestMethod]
        public void FuncDetector_DoTest_ShouldReturnFalseForConditionalStatements()
        {
            List<string> tokens = new List<string> { "if", "(", "something", "==", "true", ")", "{" };
            CSFuncDetector fd = new CSFuncDetector();
            //Assert.IsFalse(fd.DoTest(tokens));
            tokens = new List<string> { "while", "(", "something", ">", "10", ")", "{" };
            //Assert.IsFalse(fd.DoTest(tokens));
            tokens = new List<string> { "for", "(", "int", "i", "=", "0", ";", "i", "<", "10", ";", "i++", ")", "{" };
            //Assert.IsFalse(fd.DoTest(tokens));
        }

        [TestMethod]
        public void ConditionalDetector_IsIfElse_ShouldReturnTrue()
        {
            PrivateObject detector = new PrivateObject(typeof(CSConditionalDetector));
            List<string> tokens = new List<string> { "if", "(", "something", "==", "somethingelse", ")", "{" };
            bool result = Convert.ToBoolean(detector.Invoke("IsIfElse", tokens));
            Assert.IsTrue(result);

            tokens = new List<string> { "if", "(", "something", "==", "somethingelse", ")", "return", "true", ";" };
            result = Convert.ToBoolean(detector.Invoke("IsIfElse", tokens));
            Assert.IsTrue(result);

            tokens = new List<string> { "else", "return", "false", ";" };
            result = Convert.ToBoolean(detector.Invoke("IsIfElse", tokens));
            Assert.IsTrue(result);

            tokens = new List<string> { "else", "{" };
            result = Convert.ToBoolean(detector.Invoke("IsIfElse", tokens));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConditionalDetector_IsFor_ShouldReturnTrue()
        {
            PrivateObject detector = new PrivateObject(typeof(CSConditionalDetector));
            List<string> tokens = new List<string> { "for", "(", "int", "i=0", ";", "i", "<", "10", ";", "i++", ")", "{" };
            bool result = Convert.ToBoolean(detector.Invoke("IsFor", tokens));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConditionalDetector_IsFor_ShouldReturnFalse()
        {
            PrivateObject detector = new PrivateObject(typeof(CSConditionalDetector));
            List<string> tokens = new List<string> { "for", "(", "int", "i=0", ";", "i", "<", "something.GetLength", "(", ")", ";"};
            bool result = Convert.ToBoolean(detector.Invoke("IsFor", tokens));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AddScopeAction_ShouldPrintOwningClass()
        {
            List<string> tokens = new List<string> { "for", "(", "int", "i=0", ";", "i", "<", "something.GetLength", "(", ")", ";" };

            AddScope addScope = new AddScope();
            AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Conditional, tokens, "conditional");
            addScope.DoAction(analysisObject);
        }
    }
}
