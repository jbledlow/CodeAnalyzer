/**
 * CodeAnalysis.cs
 * This package is intended to be used as a part of the CodeAnalyzer Project
 * for CSE 681 Software Modelling
 * 
 * Summary:
 * 
 * This package defines the interface to which detectors and actions will conform.
 * 
 * Dependencies:
 * 
 * The interfaces contained herein depend only upon .NET system libraries
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /// <summary>
    /// Interface definition for a syntax detector
    /// </summary>
    public interface IDetector
    {
        bool DoTest(List<string> tokens);
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
