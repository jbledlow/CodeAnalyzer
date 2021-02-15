/*********************************************************************************
 *   DetectorsAndActions.cs: Package to contain syntax detectors and actions
 *   
 *   Author: Jonathan Ledlow
 *   Date: 10 Feb 2021
 *   
 *   
 *   Updates:
 *      - Initial Dev
 *      - Added Factory class
 *      - Refactored class names to relate specifically to C#
 *      - Added detector for interface
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace CodeAnalyzer
{
    ///////////////////////////////////////////////////////////////////////////
    ///                       Detectors                                     ///
    ///////////////////////////////////////////////////////////////////////////
    public class CSBaseDetector : IDetector
    {
        protected List<IAction> ActionsList = new List<IAction>(); //{ get; set; }
        //public DataManager dataManager { get; set; }
        protected IDetector _nextDetector;

        public virtual void AddAction(IAction action)
        {
            ActionsList.Add(action);
        }

        public virtual void DoActions(AnalysisObject analysisObject)
        {
            foreach (var action in ActionsList)
            {
                action.DoAction(analysisObject);
            }
        }

        public virtual void DoTest(List<string> tokens)
        {
            Console.WriteLine("Unable to Identify the following: {0}", string.Join(" ",tokens.ToArray()));
            //return true;
        }

        public virtual IDetector SetNext(IDetector detector)
        {
            this._nextDetector = detector;

            return detector;
        }
    }
    /// <summary>
    /// A class for detecting classes in a list of tokens
    /// </summary>
    public class CSClassDetector : CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("class") && tokens.Contains("{"))
            {
                string name = tokens[tokens.IndexOf("class") + 1];
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Class, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            } else
            {
                _nextDetector?.DoTest(tokens);
            }
            
        }
    }

    public class CSInterfaceDetector: CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("interface") && tokens.Contains("{"))
            {
                string name = tokens[tokens.IndexOf("interface") + 1];
                // For now, I am just treating is as a conditional
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Interface, tokens, name);
                this.DoActions(analysisObject);
                //eturn true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }

        }
    }

    /// <summary>
    /// A class for detection simple statments in a list of tokens
    /// </summary>
    public class CSStatementDetector : CSBaseDetector, IDetector
    {
        
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains(";") &&
                tokens.IndexOf(";") == tokens.Count - 1 &&
                !tokens.Contains("for") &&
                !tokens.Contains("if") &&
                !tokens.Contains("else") &&
                !tokens.Contains("while") &&
                !tokens.Contains("switch"))
            {
                string name = null;
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Statement, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
            
        }        
    }

    /// <summary>
    /// A class for detecting a function in a list of tokens
    /// </summary>
    public class CSFuncDetector : CSBaseDetector
    {

        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("(") &&
                tokens.Contains(")") &&
                tokens.Contains("{") &&
                !tokens.Contains("for") &&
                !tokens.Contains("foreach") &&
                !tokens.Contains("if") &&
                !tokens.Contains("while") &&
                !tokens.Contains("else") &&
                !tokens.Contains("switch") &&
                !tokens.Contains("catch") &&
                !tokens.Contains("=>") &&
                (tokens.IndexOf("(") < tokens.IndexOf(")") && tokens.IndexOf(")") < tokens.IndexOf("{")))
            {
                string name = tokens[tokens.IndexOf("(") - 1];
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Function, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
        }
    }

    /// <summary>
    /// Detector class to test if one of the conditional statements
    /// </summary>
    public class CSConditionalDetector : CSBaseDetector
    {

        public override void DoTest(List<string> tokens)
        {
            
            if (IsFor(tokens) || IsWhile(tokens) || IsIfElse(tokens) || IsSwitch(tokens) || IsTryCatch(tokens))
            {
                string name = "Conditional";
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Conditional, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
        }

        private bool IsIfElse(List<string> tokens)
        {
            return (tokens.Contains("if") && 
                tokens.Contains("(") && 
                tokens.Contains(")") && 
                (tokens.Last() == ";" || tokens.Last() == "{")) ||
                // else check
                (tokens.Contains("else") && 
                (tokens.Last() == ";" || tokens.Last() == "{"));

        }

        private bool IsFor(List<string> tokens)
        {
            return (tokens.Contains("for") || tokens.Contains("foreach")) && 
                tokens.Contains("(") && 
                tokens.Contains(")") && 
                tokens.Contains("{");
        }

        private bool IsWhile(List<string> tokens)
        {
            // need to test for "while (condition) {" as well as "while(true) ;"
            return tokens.Contains("while") && 
                tokens.Contains("(") && 
                tokens.Contains(")") && 
                (tokens.Contains("{") || tokens.Contains(";"));


        }

        private bool IsSwitch(List<string> tokens)
        {
            return tokens.Contains("switch") && 
                tokens.Contains("(") && 
                tokens.Contains(")") && 
                tokens.Contains("{");
        }

        private bool IsTryCatch(List<string> tokens)
        {
            return (tokens.Contains("try") || tokens.Contains("catch"));
        }
    }

    /// <summary>
    ///  Detector to test if a namespace
    /// </summary>
    public class CSNamespaceDetector : CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("namespace") && tokens.Contains("{"))
            {
                string name = tokens[tokens.IndexOf("namespace") + 1];
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Namespace, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
        }
    }

    public class CSEndOfScopeDetector : CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("}"))
            {
                string name = "scopeEnd";
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Statement, tokens, name);
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
            
        }
    }

    public class CSPropAndEnumDetector : CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("{") &&
                !tokens.Contains("for") &&
                !tokens.Contains("if") &&
                !tokens.Contains("while") &&
                !tokens.Contains("else") &&
                !tokens.Contains("switch") &&
                !tokens.Contains("class") &&
                !tokens.Contains("interface") &&
                !tokens.Contains("("))
            {
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.PropEnum, tokens, "PropEnum");
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
        }
    }

    public class CSLambdaDetector : CSBaseDetector
    {
        public override void DoTest(List<string> tokens)
        {
            if (tokens.Contains("=>") && tokens.Contains("{"))
            {
                AnalysisObject analysisObject = new AnalysisObject(DataManager.ScopeType.Lambda, tokens, "Lambda");
                this.DoActions(analysisObject);
                //return true;
            }
            else
            {
                _nextDetector?.DoTest(tokens);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    ///                         ACTIONS                                     ///
    ///////////////////////////////////////////////////////////////////////////          

    /// <summary>
    /// Class to add current item to the scope stack.
    /// </summary>
    public class AddScope : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            // Things like if and else can mess up scope
            if (analysisObject.ObjectTokens.Contains("{"))
            {
                DataManager.AddScope(analysisObject);
            }
            else
            {
                DataManager.IncrementScope();
            }
        }
    }

    public class EndScope : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            DataManager.RemoveScope();
        }
    }

    public class ProcessStatement : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            DataManager.AddStatement();
        }
    }

    public class SaveObject : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            DataManager.SaveObject(analysisObject);
        }
    }

    public class CSAnalyzeParams : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            int parenStart = analysisObject.ObjectTokens.IndexOf("(");
            int parenEnd = analysisObject.ObjectTokens.LastIndexOf(")");
            if ((parenEnd - parenStart) > 1)
            {
                DataManager.CheckUsing(analysisObject.ObjectTokens.GetRange(parenStart + 1, (parenEnd - parenStart) - 1));
            }
        }
    }

    public class CSCheckInheritance : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            if (analysisObject.ObjectTokens.Contains(":"))
            {
                int index = analysisObject.ObjectTokens.IndexOf(":");
                DataManager.CheckInheritance(analysisObject.ObjectTokens.GetRange(index+1, analysisObject.ObjectTokens.Count-index-1));
            }
        }
    }

    public class CSCheckAssociation : IAction
    {
        public void DoAction(AnalysisObject analysisObject)
        {
            DataManager.CheckAssociation(analysisObject.ObjectTokens);
        }
    }



    ///////////////////////////////////////////////////////////////////////////
    ///                        Factory                                      ///
    ///////////////////////////////////////////////////////////////////////////
    public interface AbstractDetectorFactory
    {
        AbstractFunctionalAnalysis CreateFunctionalAnalysisDetectors();
        AbstractRelationshipAnalysis CreateRelationshipAnalysisDetectors();
        AbstractClassScannerAnalysis CreateClassScannerAnalysisDetectors();
    }

    public class CSharpDetectorFactory : AbstractDetectorFactory
    {
        public AbstractClassScannerAnalysis CreateClassScannerAnalysisDetectors()
        {
            return new CSClassScannerAnalysis();
        }

        public AbstractFunctionalAnalysis CreateFunctionalAnalysisDetectors()
        {
            return new CSFunctionalAnalysis();
        }

        public AbstractRelationshipAnalysis CreateRelationshipAnalysisDetectors()
        {
            return new CSClassRelationshipAnalysis();
        }
    }

    public interface AbstractFunctionalAnalysis
    {
        IDetector GetDetectorChain();
    }

    public interface AbstractRelationshipAnalysis
    {
        IDetector GetDetectorChain();
    }

    public interface AbstractClassScannerAnalysis
    {
        IDetector GetDetectorChain();
    }

    class CSFunctionalAnalysis : AbstractFunctionalAnalysis
    {
        public IDetector GetDetectorChain()
        {
            //namespace detector
            IDetector namespaceDetector = new CSNamespaceDetector();
            namespaceDetector.AddAction(new AddScope());
            namespaceDetector.AddAction(new SaveObject());
            // class detector
            IDetector classDetector = new CSClassDetector();
            classDetector.AddAction(new AddScope());
            classDetector.AddAction(new SaveObject());
            // interface detector
            IDetector interfaceDetector = new CSInterfaceDetector();
            interfaceDetector.AddAction(new AddScope());
            // properties and enums
            IDetector propEnumDetector = new CSPropAndEnumDetector();
            propEnumDetector.AddAction(new AddScope());
            //function detector
            IDetector functionDetector = new CSFuncDetector();
            functionDetector.AddAction(new AddScope());
            functionDetector.AddAction(new SaveObject());
            // lambda detector
            IDetector lambdaDetector = new CSLambdaDetector();
            lambdaDetector.AddAction(new AddScope());
            // conditional detector
            IDetector conditionalDetector = new CSConditionalDetector();
            conditionalDetector.AddAction(new AddScope());
            // End of scope detector
            IDetector endScopeDetector = new CSEndOfScopeDetector();
            endScopeDetector.AddAction(new EndScope());
            // statement detector
            IDetector statementDetector = new CSStatementDetector();
            statementDetector.AddAction(new ProcessStatement());
            // tie up everything with the default
            IDetector baseDetector = new CSBaseDetector();
            namespaceDetector.SetNext(classDetector)
                .SetNext(interfaceDetector)
                .SetNext(propEnumDetector)
                .SetNext(functionDetector)
                .SetNext(lambdaDetector)
                .SetNext(conditionalDetector)
                .SetNext(endScopeDetector)
                .SetNext(statementDetector)
                .SetNext(baseDetector);
            return namespaceDetector;
        }
    }

    class CSClassRelationshipAnalysis : AbstractRelationshipAnalysis
    {
        public IDetector GetDetectorChain()
        {
            IDetector namespaceDetector = new CSNamespaceDetector();
            namespaceDetector.AddAction(new AddScope());
            //namespaceDetector.AddAction(new SaveObject());
            // class detector
            IDetector classDetector = new CSClassDetector();
            classDetector.AddAction(new AddScope());
            classDetector.AddAction(new CSCheckInheritance());
            //classDetector.AddAction(new SaveObject());
            // interface detector
            IDetector interfaceDetector = new CSInterfaceDetector();
            interfaceDetector.AddAction(new AddScope());
            // properties and enums
            IDetector propEnumDetector = new CSPropAndEnumDetector();
            propEnumDetector.AddAction(new AddScope());
            //function detector
            IDetector functionDetector = new CSFuncDetector();
            functionDetector.AddAction(new AddScope());
            functionDetector.AddAction(new CSAnalyzeParams());
            //functionDetector.AddAction(new SaveObject());
            // lambda detector
            IDetector lambdaDetector = new CSLambdaDetector();
            lambdaDetector.AddAction(new AddScope());
            // conditional detector
            IDetector conditionalDetector = new CSConditionalDetector();
            conditionalDetector.AddAction(new AddScope());
            // End of scope detector
            IDetector endScopeDetector = new CSEndOfScopeDetector();
            endScopeDetector.AddAction(new EndScope());
            // statement detector
            IDetector statementDetector = new CSStatementDetector();
            statementDetector.AddAction(new ProcessStatement());
            statementDetector.AddAction(new CSCheckAssociation());
            // tie up everything with the default
            IDetector baseDetector = new CSBaseDetector();
            namespaceDetector.SetNext(classDetector)
                .SetNext(interfaceDetector)
                .SetNext(propEnumDetector)
                .SetNext(functionDetector)
                .SetNext(lambdaDetector)
                .SetNext(conditionalDetector)
                .SetNext(endScopeDetector)
                .SetNext(statementDetector)
                .SetNext(baseDetector);
            return namespaceDetector;
        }
    }

    class CSClassScannerAnalysis : AbstractClassScannerAnalysis
    {
        public IDetector GetDetectorChain()
        {
            IDetector namespaceDetector = new CSNamespaceDetector();
            namespaceDetector.AddAction(new AddScope());
            namespaceDetector.AddAction(new SaveObject());
            // class detector
            IDetector classDetector = new CSClassDetector();
            classDetector.AddAction(new AddScope());
            classDetector.AddAction(new SaveObject());
            // interface detector
            IDetector interfaceDetector = new CSInterfaceDetector();
            interfaceDetector.AddAction(new AddScope());
            // properties and enums
            IDetector propEnumDetector = new CSPropAndEnumDetector();
            propEnumDetector.AddAction(new AddScope());
            //function detector
            IDetector functionDetector = new CSFuncDetector();
            functionDetector.AddAction(new AddScope());
            //functionDetector.AddAction(new SaveObject());
            // lambda detector
            IDetector lambdaDetector = new CSLambdaDetector();
            lambdaDetector.AddAction(new AddScope());
            // conditional detector
            IDetector conditionalDetector = new CSConditionalDetector();
            conditionalDetector.AddAction(new AddScope());
            // End of scope detector
            IDetector endScopeDetector = new CSEndOfScopeDetector();
            endScopeDetector.AddAction(new EndScope());
            // statement detector
            IDetector statementDetector = new CSStatementDetector();
            statementDetector.AddAction(new ProcessStatement());
            // tie up everything with the default
            IDetector baseDetector = new CSBaseDetector();
            namespaceDetector.SetNext(classDetector)
                .SetNext(interfaceDetector)
                .SetNext(propEnumDetector)
                .SetNext(functionDetector)
                .SetNext(lambdaDetector)
                .SetNext(conditionalDetector)
                .SetNext(endScopeDetector)
                .SetNext(statementDetector)
                .SetNext(baseDetector);
            return namespaceDetector;
        }
    }
}



