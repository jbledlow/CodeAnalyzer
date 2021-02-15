using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Xml;

namespace CodeAnalyzer
{
    public class AnalysisObject
    {
        public DataManager.ScopeType ObjectType { get; set; }
        public List<string> ObjectTokens { get; set; }
        public string Name { get; set; }

        public AnalysisObject(DataManager.ScopeType scopeType, List<string> tokens, string name)
        {
            ObjectType = scopeType;
            ObjectTokens = tokens;
            Name = name;
        }
    }

    public static class DataManager
    {
        private static Stack<(string, ScopeType)> scopeStack = new Stack<(string, ScopeType)>();
        private static XmlDocument xdoc = new XmlDocument();
        private static int _currentScopeCount = 0;
        private static int _currentLineCount = 0;
        private static string _currentNameSpace = null;
        private static string _currentClass = null;
        private static string _currentFunction = null;
        private static bool _inProperty = false;
        public enum ScopeType { Namespace, Class, Function, Conditional, Statement, Interface, PropEnum, Lambda }

        static DataManager()
        {
            xdoc.LoadXml("<!DOCTYPE analysisResults [ <!ELEMENT Namespace ANY><!ELEMENT Class ANY><!ELEMENT Function ANY><!ATTLIST Namespace id ID #REQUIRED>]><analysisResults></analysisResults>");
        }
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

        private static void AddLambdaScope(AnalysisObject analysisObject)
        {
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
            _currentScopeCount++;
            _currentLineCount++;
        }

        private static void AddConditionalScope(AnalysisObject analysisObject)
        {
            if (_currentFunction == null && _inProperty == false)
            {
                throw new Exception(String.Format("Conditional statements must be inside functions: {0}", string.Join(" ", analysisObject.ObjectTokens.ToArray())));
            }
            else
            {
                scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
                _currentScopeCount++;
                _currentLineCount++;
            }
        }

        private static void AddFunctionScope(AnalysisObject analysisObject)
        {
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
                _currentLineCount = 0;
                _currentScopeCount = 0;
            }
            //saveNodeToXML(analysisObject, CreatePath(_currentNameSpace, _currentClass));
        }

        private static void AddClassScope(AnalysisObject analysisObject)
        {
            if (_currentClass != null)
            {
                analysisObject.Name = _currentClass = _currentClass + "." + analysisObject.Name;
                // need to update the object name

            }
            else
            {
                _currentClass = analysisObject.Name;
            }
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
            //saveNodeToXML(analysisObject, CreatePath(_currentNameSpace));
        }

        private static void AddNamespaceScope(AnalysisObject analysisObject)
        {
            _currentNameSpace = analysisObject.Name;
            scopeStack.Push((analysisObject.Name, analysisObject.ObjectType));
            //saveNodeToXML(analysisObject, CreatePath());
        }

        public static void RemoveScope()
        {
            if (scopeStack.Count < 1)
            {
                throw new InvalidOperationException("Stack does not have any values!");
            }
            var poppedScope = scopeStack.Pop();
            switch (poppedScope.Item2)
            {
                case ScopeType.Namespace:
                    _currentNameSpace = null;
                    break;
                case ScopeType.Class:
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
                        updateFunctionAttributes(CreatePath(_currentNameSpace, _currentClass, _currentFunction), _currentScopeCount, _currentLineCount);
                    }
                    _currentFunction = null;
                    break;
                case ScopeType.PropEnum:
                    _inProperty = false;
                    break;
                case ScopeType.Conditional:
                    _currentLineCount++;
                    break;
                case ScopeType.Lambda:
                    _currentLineCount++;
                    break;
                default:

                    break;
            }
        }

        public static void AddStatement()
        {
            _currentLineCount++;
        }

        private static void saveNodeToXML(AnalysisObject analysisObject, string path)
        {
            string fullpath = string.Format("{0}/{1}[@id='{2}']", path, analysisObject.ObjectType.ToString(), analysisObject.Name);
            //Console.WriteLine(fullpath);
            if (xdoc.SelectSingleNode(fullpath) !=null )
            {
                return;
            }
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
        private static void saveNamespaceToXml(string name)
        {
            XmlElement xmlElement = xdoc.CreateElement("Namespace");
            xmlElement.SetAttribute("id", name);
            xdoc.DocumentElement.AppendChild(xmlElement.Clone());
            //xdoc.Save(Console.Out);
            //Console.WriteLine();
        }

        private static void saveClassToXml(string name, string parent)
        {

            XmlElement xmlElement = xdoc.CreateElement("Class");
            xmlElement.SetAttribute("id", name);
            var targetParentNode = xdoc.GetElementById(parent);
            targetParentNode?.AppendChild(xmlElement); // TODO: handle this
            //xdoc.Save(Console.Out);
            //Console.WriteLine();
        }

        private static void saveFunctionToXml(string name, string parent, string grandParent)
        {
            // create the new element
            XmlElement xmlElement = xdoc.CreateElement("Function");
            // set attributes
            xmlElement.SetAttribute("id", name);
            // create the path that should exist
            string nodepath = String.Format("/analysisResults/Namespace[@id='{0}']/Class[@id='{1}']", grandParent, parent);
            // find fully qualified path
            var targetParentNode = xdoc.SelectSingleNode(nodepath);
            targetParentNode.AppendChild(xmlElement);
            
        }

        private static void updateFunctionAttributes(string pathToFunction, int complexity, int numLines)
        {
            //Console.WriteLine("Path is {0}", pathToFunction);
            var targetFunctionNode = xdoc.SelectSingleNode(pathToFunction);
            XmlAttribute attr = xdoc.CreateAttribute("Complexity");
            attr.Value = complexity.ToString();
            targetFunctionNode.Attributes.SetNamedItem(attr);

            attr = xdoc.CreateAttribute("Lines");
            attr.Value = numLines.ToString();
            targetFunctionNode.Attributes.SetNamedItem(attr);
            //xdoc.Save(Console.Out);
            //Console.WriteLine();
        }

        private static string CreatePath()
        {
            return "/analysisResults";
        }

        private static string CreatePath(string Namespace)
        {
            return String.Format("/analysisResults/Namespace[@id='{0}']", Namespace);
        }

        private static string CreatePath(string NameSpace, string ClassName)
        {
            return String.Format("/analysisResults/Namespace[@id='{0}']/Class[@id='{1}']", NameSpace, ClassName);
        }

        private static string CreatePath(string NameSpace, string ClassName, string FuncName)
        {
            return String.Format("/analysisResults/Namespace[@id='{0}']/Class[@id='{1}']/Function[@id='{2}']", NameSpace, ClassName, FuncName);
        }

        public static void OutputData()
        {
            xdoc.Save(Console.Out);
            Console.WriteLine();
        }

        // reset some data after finished with file
        public static void ResetData()
        {
            scopeStack.Clear();
            _currentScopeCount = 0;
            _currentLineCount = 0;
            _currentNameSpace = null;
            _currentClass = null;
            _currentFunction = null;
        }

        internal static void SaveObject(AnalysisObject analysisObject)
        {
            switch (analysisObject.ObjectType)
            {
                case ScopeType.Namespace:
                    saveNodeToXML(analysisObject, CreatePath());
                    break;
                case ScopeType.Class:
                    saveNodeToXML(analysisObject, CreatePath(_currentNameSpace));
                    break;
                case ScopeType.Function:
                    saveNodeToXML(analysisObject, CreatePath(_currentNameSpace, _currentClass));
                    break;
                default:
                    break;
            }
        }

        internal static void CheckInheritance(List<string> tokens)
        {
            // get list of classes
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");

            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                if (tokens.Contains(id))
                {
                    string pathToCurrentClass = CreatePath(_currentNameSpace, _currentClass);
                    var classNode = xdoc.SelectSingleNode(pathToCurrentClass);
                    //XmlAttributeCollection xmlAttributeCollection = classNode.Attributes;
                    if (classNode.Attributes["Inheritance"] == null)
                    {
                        XmlAttribute attr = xdoc.CreateAttribute("Inheritance");
                        attr.Value = id;
                        classNode.Attributes.Append(attr);
                    }
                    else
                    {
                        classNode.Attributes["Inheritance"].Value = classNode.Attributes["Inheritance"].Value + ", " + id;
                    }
                }
            }
        }

        internal static void CheckAssociation(List<string> tokens)
        {
            if (_currentClass == null)
            {
                return;
            }

            if (_currentFunction != null)
            {
                CheckUsing(tokens);
                return;
            }
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");

            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                if (tokens.Contains(id))
                {
                    string pathToCurrentClass = CreatePath(_currentNameSpace, _currentClass);
                    var classNode = xdoc.SelectSingleNode(pathToCurrentClass);
                   
                    //XmlAttributeCollection xmlAttributeCollection = classNode.Attributes;
                    if (classNode.Attributes["Association"] == null)
                    {
                        XmlAttribute attr = xdoc.CreateAttribute("Association");
                        attr.Value = id;
                        classNode.Attributes.Append(attr);
                    }
                    else
                    {
                        if (!classNode.Attributes["Association"].Value.Contains(id))
                        {
                            classNode.Attributes["Association"].Value = classNode.Attributes["Association"].Value + ", " + id;
                        }
                    }
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

        internal static void CheckUsing(List<string> tokens)
        {
            // get list of classes
            XmlNodeList classNodes = xdoc.GetElementsByTagName("Class");
            
            foreach (XmlElement node in classNodes)
            {
                string id = node.Attributes["id"].Value;
                if (tokens.Contains(id))
                {
                    string pathToCurrentClass = CreatePath(_currentNameSpace, _currentClass);
                    var classNode = xdoc.SelectSingleNode(pathToCurrentClass);
                    //XmlAttributeCollection xmlAttributeCollection = classNode.Attributes;
                    if (classNode.Attributes["Using"] == null)
                    {
                        XmlAttribute attr = xdoc.CreateAttribute("Using");
                        attr.Value = id;
                        classNode.Attributes.Append(attr);
                    }
                    else
                    {
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
