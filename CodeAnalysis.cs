/**
 * CodeAnalysis.cs
 * Original Author: Jonathan Ledlow
 * Date Last Modified: 17 Feb 2021
 * 
 * Summary:
 * 
 * This package defines the interface to which detectors and actions will conform.
 * A detector is used to test for specific language patters and may have a list
 * of actions that are triggered. Actions are used to perform a specific type
 * of processing. Often, these actions will be used to interact with a form of
 * data management.
 * 
 * Contents:
 *      public interface IDetector
 *      public interface IAction
 * 
 * Dependencies:
 * 
 * The interfaces contained herein depend only upon .NET system libraries
 * 
 * Change History:
 *      - Initial implementation
 *      - Refactored IDetector.DoActions and IAction.DoAction to accept AnalysisObject
 *      - Added IDetector.SetNext to inplement chain of responsibility
 * 
 */

using System.Collections.Generic;

namespace CodeAnalyzer
{
    /// <summary>
    /// Interface definition for a syntax detector
    /// </summary>
    public interface IDetector
    {
        void DoTest(List<string> tokens);
        void AddAction(IAction action);
        void DoActions(AnalysisObject analysisObject);
        IDetector SetNext(IDetector detector);
    }

    /// <summary>
    /// Interface definition for an action which a detector will take upon 
    /// a successful test.
    /// </summary>
    public interface IAction
    {
        void DoAction(AnalysisObject analysisObject);
    }
    
}
