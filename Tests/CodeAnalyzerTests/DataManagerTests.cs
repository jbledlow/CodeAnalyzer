using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CodeAnalyzer;
using System.Collections.Generic;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class DataManagerTests
    {
        [TestMethod]
        public void DataManager_AddScope_CurrentsShouldMatchInput()
        {
            string Namespace = "codeAnalyzer";
            string Class = "Parser";
            string Function = "parse";
            List<string> tokens = new List<string>();
            PrivateType dm = new PrivateType(typeof(DataManager));
            AnalysisObject no = new AnalysisObject(DataManager.ScopeType.Namespace, tokens, Namespace);
            AnalysisObject co = new AnalysisObject(DataManager.ScopeType.Class, tokens, Class) ;
            AnalysisObject fo = new AnalysisObject(DataManager.ScopeType.Function, tokens, Function);

            dm.InvokeStatic("AddScope", no);
            dm.InvokeStatic("AddScope", co);
            dm.InvokeStatic("AddScope", fo);

            Assert.AreEqual(dm.GetStaticFieldOrProperty("_currentNameSpace"), Namespace);
            Assert.AreEqual(dm.GetStaticFieldOrProperty("_currentClass"), Class);
            Assert.AreEqual(dm.GetStaticFieldOrProperty("_currentFunction"), Function);


        }

        [TestMethod]
        public void DataManager_AddScope_ShouldThrowErrorOnFunctionOutsideClass()
        {
            PrivateType dm = new PrivateType(typeof(DataManager));
            bool exceptionThrown = false;
            try
            {
                dm.InvokeStatic("AddScope", DataManager.ScopeType.Function, "parse");
            }
            catch (Exception)
            {
                exceptionThrown = true;   
            }
            Assert.IsTrue(exceptionThrown);
        }
        /* Replaced with generic
        [TestMethod]
        public void DataManager_saveNameSpacetoXML_ShouldPrintOut()
        {
            PrivateType dm = new PrivateType(typeof(DataManager));
            dm.InvokeStatic("saveNamespaceToXml", "myNamespace");
            dm.InvokeStatic("saveClassToXml", "myClass", "myNamespace");
            dm.InvokeStatic("saveFunctionToXml", "myFunc", "myClass", "myNamespace");
            string result = Convert.ToString(dm.InvokeStatic("CreatePath", "myNamespace", "myClass", "myFunc"));
            dm.InvokeStatic("updateFunctionAttributes", result, 3, 10);
            
        } */
    }
}
