/**
 * DataManager.cs
 * Original Author: Jonathan Ledlow
 * Date Last Modified: 17 Feb 2021
 * 
 * Summary:
 * 
 * This package defines the classes with which analysis data is manipulated and stored.
 * The DataManager class is defined as a static class, as it is not intended that multiple
 * instances of the DataManager are instantiated. Future changes may push the static modifier
 * into packages that depend on this package to allow multiple objects to exist (for parallel
 * or concurrent processing). Additionally, DataManager may need to extracted as a base class
 * and have subclasses specific to different language sets.
 * 
 * Contents:
 *      - public class AnalysisObject
 *      - public static class DataManager
 * 
 * Dependencies:
 *      - Extensive use of the XML library
 * 
 * 
 * Change History:
 *      - Added File as a top level node (below the root) in XML Structure
 *      - Replaced specific save to xml funtions with a general function
 *        to handle any object type
 *      - Refactored Association checking into a set of smaller functions
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CodeAnalyzer
{
    /// <summary>
    /// Used to pass expressions returned from the parser with their type and tokens
    /// </summary>
    public class AnalysisObject
    {
        public DataManager.ScopeType ObjectType { get; set; }
        public List<string> ObjectTokens { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Create an instance of the AnalysisObject
        /// </summary>
        /// <param name="scopeType"></param>
        /// <param name="tokens"></param>
        /// <param name="name"></param>
        public AnalysisObject(DataManager.ScopeType scopeType, List<string> tokens, string name)
        {
            ObjectType = scopeType;
            ObjectTokens = tokens;
            Name = name;
        }
    }

    /// <summary>
    /// Static class to handle processing and saving of data
    /// </summary>
    public static class DataManager
    {
        // Private variables
        private static Stack<(string, ScopeType)> scopeStack = new Stack<(string, ScopeType)>();
        private static XmlDocument xdoc = new XmlDocument();
        private static int _currentScopeCount = 0;
        private static int _currentStatementCount = 0;
        private static int _currentNewLineCount = 0;
        private static string _currentNameSpace = null;
        private static string _currentClass = null;
        private static string _currentFunction = null;
        private static bool _inProperty = false;
        private static string _currentFile;
        // Enum of expression types, used by analysisObject
        public enum ScopeType { Namespace, Class, Function, Conditional, Statement, Interface, PropEnum, Lambda, File }

        /// <summary>
        /// Create DataManager object and initialize XML
        /// </summary>
        static DataManager()
        {
            xdoc.LoadXml("<!DOCTYPE analysisResults [ <!ELEMENT File ANY><!ELEMENT Namespace ANY><!ELEMENT Class ANY><!ELEMENT Function ANY><!ATTLIST Namespace id ID #REQUIRED>]><analysisResults></analysisResults>");
        }

        /// <summary>
        /// Set the current filename being parsed
        /// </summary>
        /// <param name="fileName"></param>
        public static void SetCurrentFile(string fileName)
        {
            _currentFile = fileName;
            AnalysisObject fileObject = new AnalysisObject(ScopeType.File, null, fileName);
            saveNodeToXML(fileObject, "/analysisResults");
        }

        /// <summary>
        /// Add an analysisObject onto the scope stack. Calls specific function based on object type
        /// </summary>
        /// <param name="analysisObject"></param>
        public static void AddScope(AnalysisObject analysisObject)
        {
            _currentScopeCount++;
            switch (analysisObject.ObjectType)
            {
                case ScopeType.Namespace:
                    AddNamespaceScope(analysisObject);
                    break;
                case ScopeType.Class:
                    AddClassScope(analysisObject);
                    break;
                case ScopeType.Function:
                    AddFunctionScope(analysisObject);
                    break;
                case ScopeType.Conditional:
                    AddConditionalScope(analysisObject);
                    break;
                case ScopeType.Lambda:
                    AddLambdaScope(analysisObject);
                    break;
                case ScopeType.PropEnum:
                    _inProperty = true; // for now I won't extract this to function
                    goto case ScopeType.Interface; // They really should allow fall through here.
                case ScopeType.Interface:
                    scopeStack.Push((analysisObject.Name, analysisObject.ObjectType)); // will not extract for now
                    break;
                default:
                    throw new ArgumentException("The provided type is not valid!");
            }   
        }

        /// <summary>
        /// Add a lambda expression to scope stack
        /// </summary>
        /// <param name="analysisObject"></param>
        private static void AddLambdaScope(AnalysisObject analysisObject)
        {
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
            _currentStatementCount++;
        }

        /// <summary>
        /// Add a conditional statement onto the scope stack
        /// </summary>
        /// <param name="analysisObject"></param>
        private static void AddConditionalScope(AnalysisObject analysisObject)
        {
            // make sure that we are inside of a function
            if (_currentFunction == null && _inProperty == false)
            {
                throw new Exception(String.Format("Conditional statements must be inside functions: {0}", string.Join(" ", analysisObject.ObjectTokens.ToArray())));
            }
            else
            {
                scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
                _currentStatementCount++;
            }
        }

        /// <summary>
        /// Add a function object onto the scope stack
        /// </summary>
        /// <param name="analysisObject"></param>
        private static void AddFunctionScope(AnalysisObject analysisObject)
        {
            // must be in class scope only
            if (_currentFunction != null || _currentClass == null)
            {
                Console.WriteLine("CurrentFunction is {0} and CurrentClass is {1}", _currentFunction, _currentClass);
                Console.WriteLine("{0} caused the exception", analysisObject.Name);
                throw new Exception("A function must be declared as a class member.");
            }
            else
            {
                _currentFunction = analysisObject.Name;
                scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
                // reset counts to keep track of statistics
                _currentStatementCount = 0;
                _currentScopeCount = 0;
                _currentNewLineCount = 0;
            }
        }

        /// <summary>
        /// Add class object onto scope stack
        /// </summary>
        /// <param name="analysisObject"></param>
        private static void AddClassScope(AnalysisObject analysisObject)
        {
            // check if class is inside of other class
            if (_currentClass != null)
            {
                analysisObject.Name = _currentClass = _currentClass + "." + analysisObject.Name;
            }
            else
            {
                _currentClass = analysisObject.Name;
            }
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
            
        }

        /// <summary>
        /// Add a namespace object onto scope stack
        /// </summary>
        /// <param name="analysisObject"></param>
        private static void AddNamespaceScope(AnalysisObject analysisObject)
        {
            _currentNameSpace = analysisObject.Name;
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
        }

        /// <summary>
        /// Remove the last object from the scope stack
        /// </summary>
        public static void RemoveScope()
        {
            if (scopeStack.Count < 1)
            {
                throw new InvalidOperationException("Stack does not have any values!");
            }
            var poppedScope = scopeStack.Pop();
            // perform action based on object type
            switch (poppedScope.Item2)
            {
                case ScopeType.Namespace:
                    _currentNameSpace = null;
                    break;
                case ScopeType.Class:
                    // Check if a subclass within another class
                    if (_currentClass.Contains('.'))
                    {
                        _currentClass = _currentClass.Split('.')[0];
                    }
                    else
                    {
                        _currentClass = null;
                    }
                    break;
                case ScopeType.Function:
                    if (!Program.AnalyzeClasses)
                    {
                        updateFunctionAttributes(CreatePath(_currentFile, _currentNameSpace, _currentClass, _currentFunction), _currentScopeCount, _currentNewLineCount, _currentStatementCount);
                    }
                    _currentFunction = null;
                    break;
                case ScopeType.PropEnum:
                    _inProperty = false;
                    break;
                case ScopeType.Conditional:
                    _currentStatementCount++;
                    break;
                case ScopeType.Lambda:
                    _currentStatementCount++;
                    break;
                default:

                    break;
            }
        }

        /// <summary>
        /// Increment line count
        /// </summary>
        public static void AddLine()
        {
            _currentNewLineCount ++;

        }

        /// <summary>
        /// Increment count of statements
        /// </summary>
        public static void AddStatement()
        {
            _currentStatementCount++;
        }

        /// <summary>
        /// Save a node to the XML tree using the XPath
        /// </summary>
        /// <param name="analysisObject"></param>
        /// <param name="path">Fully qualified path to location of parent node</param>
        private static void saveNodeToXML(AnalysisObject analysisObject, string path)
        {
            string fullpath = string.Format("{0}/{1}[@id='{2}']", path, analysisObject.ObjectType.ToString(), analysisObject.Name);
            // Check if it already exists
            if (xdoc.SelectSingleNode(fullpath) !=null )
            {
                return;
            }
            // Get parent node and create new
            try
            {
                // get parent
                var targetParentNode = xdoc.SelectSingleNode(path);
                //create new
                XmlElement newElement = xdoc.CreateElement(analysisObject.ObjectType.ToString());
                
                newElement.SetAttribute("id", analysisObject.Name);
                targetParentNode.AppendChild(newElement);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Could not find parent node.");
            }

            // chack to make sure it was created
            if (xdoc.SelectSingleNode(fullpath) == null)
            {
                Console.WriteLine("{0} not created",fullpath);
            }

        }

        /// <summary>
        /// Update function entry with statistical data
        /// </summary>
        /// <param name="pathToFunction">Fully qualified XPath to function node</param>
        /// <param name="complexity"></param>
        /// <param name="numLines"></param>
        /// <param name="numStatements"></param>
        private static void updateFunctionAttributes(string pathToFunction, int complexity, int numLines, int numStatements)
        {
            // create complexity attr
            var targetFunctionNode = xdoc.SelectSingleNode(pathToFunction);
            XmlAttribute attr = xdoc.CreateAttribute("Complexity");
            attr.Value = complexity.ToString();
            targetFunctionNode.Attributes.SetNamedItem(attr);
            // create lines attr
            attr = xdoc.CreateAttribute("Lines");
            attr.Value = numLines.ToString();
            targetFunctionNode.Attributes.SetNamedItem(attr);
            // create statment attr
            attr = xdoc.CreateAttribute("Statements");
            attr.Value = numStatements.ToString();
            targetFunctionNode.Attributes.SetNamedItem(attr);
            
        }

        /// <summary>
        /// Create path to File node
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Path to node</returns>
        private static string CreatePath(string filename)
        {
            return String.Format("/analysisResults/File[@id='{0}']", _currentFile);
        }

        /// <summary>
        /// create path to namespace node
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="Namespace"></param>
        /// <returns>Path to node</returns>
        private static string CreatePath(string filename, string Namespace)
        {
            return String.Format("/analysisResults/File[@id='{0}']/Namespace[@id='{1}']", filename, Namespace);
        }

        /// <summary>
        /// create path to class node
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="NameSpace"></param>
        /// <param name="ClassName"></param>
        /// <returns>Path to node</returns>
        private static string CreatePath(string filename, string NameSpace, string ClassName)
        {
            return String.Format("/analysisResults/File[@id='{0}']/Namespace[@id='{1}']/Class[@id='{2}']", filename, NameSpace, ClassName);
        }

        /// <summary>
        /// Create path to function node
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="NameSpace"></param>
        /// <param name="ClassName"></param>
        /// <param name="FuncName"></param>
        /// <returns>Path to function node</returns>
        private static string CreatePath(string filename, string NameSpace, string ClassName, string FuncName)
        {
            return String.Format("/analysisResults/File[@id='{0}']/Namespace[@id='{1}']/Class[@id='{2}']/Function[@id='{3}']", filename, NameSpace, ClassName, FuncName);
        }

        /// <summary>
        /// Send data to diplay. Save to XML if option enabled
        /// </summary>
        public static void OutputData()
        {
            Display display = new Display();
            display.DisplayData(xdoc);
            if (Program.WriteXMLFile)
            {
                xdoc.Save("output.xml");
            }
        }

        /// <summary>
        /// Reset DataManager information.
        /// </summary>
        public static void ResetData()
        {
            scopeStack.Clear();
            _currentScopeCount = 0;
            _currentStatementCount = 0;
            _currentNameSpace = null;
            _currentClass = null;
            _currentFunction = null;
            _currentNewLineCount = 0;
        }

        /// <summary>
        /// Generic save data to XML tree
        /// </summary>
        /// <param name="analysisObject"></param>
        public static void SaveObject(AnalysisObject analysisObject)
        {
            switch (analysisObject.ObjectType)
            {
                case ScopeType.Namespace:
                    saveNodeToXML(analysisObject, CreatePath(_currentFile));
                    break;
                case ScopeType.Class:
                    saveNodeToXML(analysisObject, CreatePath(_currentFile, _currentNameSpace));
                    break;
                case ScopeType.Function:
                    saveNodeToXML(analysisObject, CreatePath(_currentFile, _currentNameSpace, _currentClass));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Traverse classes in XML tree for inheritance
        /// </summary>
        /// <param name="tokens"></param>
        public static void CheckInheritance(List<string> tokens)
        {
            // get list of classes
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");
            // loop over each class and test if in tokens
            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                if (tokens.Contains(id))
                {
                    string pathToCurrentClass = CreatePath(_currentFile, _currentNameSpace, _currentClass);
                    var classNode = xdoc.SelectSingleNode(pathToCurrentClass);
                    // check if already has inheritance
                    if (classNode.Attributes["Inheritance"] == null)
                    {
                        XmlAttribute attr = xdoc.CreateAttribute("Inheritance");
                        attr.Value = id;
                        classNode.Attributes.Append(attr);
                    }
                    else
                    {
                        throw new Exception("Class cannot have multiple inheritance.");
                    }
                }
            }
        }

        /// <summary>
        /// Traverse classes in XML to check for association
        /// </summary>
        /// <param name="tokens"></param>
        public static void CheckAssociation(List<string> tokens)
        {
            // make sure we are in class
            if (_currentClass == null)
            {
                return;
            }
            // if we are actually in a function, redirect to using
            if (_currentFunction != null)
            {
                CheckUsing(tokens);
                return;
            }
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");
            SearchClassNodes(tokens, classNodes);
        }

        /// <summary>
        /// Search through class nodes for association
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="classNodes"></param>
        private static void SearchClassNodes(List<string> tokens, XmlNodeList classNodes)
        {
            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                if (tokens.Contains(id) && id != _currentClass)
                {
                    AddAssociation(id);
                }
            }
        }

        /// <summary>
        /// Add association if found
        /// </summary>
        /// <param name="id"></param>
        private static void AddAssociation(string id)
        {
            string pathToCurrentClass = CreatePath(_currentFile, _currentNameSpace, _currentClass);
            var classNode = xdoc.SelectSingleNode(pathToCurrentClass);

            // check if no previous associations and create
            if (classNode.Attributes["Association"] == null)
            {
                XmlAttribute attr = xdoc.CreateAttribute("Association");
                attr.Value = id;
                classNode.Attributes.Append(attr);
            }
            // append association
            else
            {
                if (!classNode.Attributes["Association"].Value.Contains(id))
                {
                    classNode.Attributes["Association"].Value = classNode.Attributes["Association"].Value + ", " + id;
                }
            }
        }

        /// <summary>
        /// Provided for circumstances where something does not need to go on stack.
        /// For instance, and one line if with no {
        /// </summary>
        public static void IncrementScope()
        {
            _currentScopeCount++;
        }

        /// <summary>
        /// Traverse XML classes to check function parameters for using.
        /// </summary>
        /// <param name="tokens"></param>
        public static void CheckUsing(List<string> tokens)
        {
            // get list of classes
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");
            //loop through nodes
            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                // if found and not the current class
                if (tokens.Contains(id) && id != _currentClass)
                {
                    string pathToCurrentClass = CreatePath(_currentFile, _currentNameSpace, _currentClass);
                    var classNode = xdoc.SelectSingleNode(pathToCurrentClass);
                    // check if already contains using, create is none, append if contains
                    if (classNode.Attributes["Using"] == null)
                    {
                        XmlAttribute attr = xdoc.CreateAttribute("Using");
                        attr.Value = id;
                        classNode.Attributes.Append(attr);
                    }
                    else
                    {
                        // make sure it has not already been added from another function
                        if (!classNode.Attributes["Using"].Value.Contains(id))
                        {
                            classNode.Attributes["Using"].Value = classNode.Attributes["Using"].Value + ", " + id;
                        }
                    }
                }
            }
        }
    }
}
