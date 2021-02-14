using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CodeAnalyzer;

namespace CodeAnalyzerTests
{
    [TestClass]
    public class DirectoryCrawlerTests
    {
        static string path;
        static List<string> files;
        [ClassInitialize]
        public static void BeforeClass(TestContext tc)
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), "temp/");
            Directory.CreateDirectory(path);
            files = new List<string> { Path.Combine(path,"testfile1.cs"), Path.Combine(path,"testfile2.cs") };
            foreach(string file in files)
            {
                File.Create(file);
            }
        }


        [TestMethod]
        public void DirectoryCrawler_ShouldReturnFilesInTempFolder ()
        {
            Console.WriteLine("Running Test\n");
            Console.WriteLine("Path is {0}", path);
            DirectoryCrawler dc = new DirectoryCrawler();
            var dcFiles = dc.GetFiles(path, "*.cs", false);
            foreach (var file in dcFiles)
            {
                Console.WriteLine(file);
            }
            CollectionAssert.AreEqual(dcFiles, files);
        }
    }
}
