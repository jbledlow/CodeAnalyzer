using System;
using System.IO;

namespace CodeAnalyzer
{
    public class FileManagement
    {
        public FileManagement()
        {
        }

        public class FileReader
        {
            private StreamReader _streamReader;
            private int _currentFileLocation;
            private string _currentPath;
            public string CurrentPath
            {
                get
                {
                    return _currentPath;
                }
                set
                {
                    if (_currentPath == value)
                    {
                        return;
                    }
                    // We have a new file, so reset the file position
                    _currentFileLocation = 0;
                    _currentPath = value;
                }
            }

            public FileReader(string path)
            {
                CurrentPath = path;
                _streamReader = new StreamReader(_currentPath);
            }

            public char readChar()
            {
                return (char)_streamReader.Read();
            }
        }
    }
}
