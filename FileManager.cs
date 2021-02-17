/**
 * FileManager.cs
 * Original Author: Jonathan Ledlow
 * Date Last Modified: 17 Feb 2021
 * 
 * Summary:
 *      This package contains a Directory crawler to obtain file names
 * 
 * 
 * Contents:
 *      - DirectoryCrawler
 * 
 * Dependencies:
 *      - None from within the project
 * 
 * 
 * Change History:
 *      
 * 
 */


using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeAnalyzer
{
    public class DirectoryCrawler
    {
        public List<string> GetFiles(string basePath, string pattern, bool isRecursive)
        {
            basePath = Path.GetFullPath(basePath);
            var option = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            List<string> files = Directory.GetFiles(basePath, pattern, option).ToList<string>();
            return files;
        }
    }
    
}
