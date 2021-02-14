using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    public class DirectoryCrawler
    {
        public List<string> GetFiles(string basePath, string pattern, bool isRecursive)
        {
            var option = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            List<string> files = Directory.GetFiles(basePath, pattern, option).ToList<string>();
            return files;
        }
    }
    
}
