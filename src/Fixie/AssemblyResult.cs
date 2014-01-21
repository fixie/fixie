using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    [Serializable]
    public class AssemblyResult
    {
        readonly List<ConventionResult> conventionResults;

        public AssemblyResult(string name)
        {
            conventionResults = new List<ConventionResult>();
            Name = name;
        }

        public void Add(ConventionResult classResult)
        {
            conventionResults.Add(classResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(conventionResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ConventionResult> ConventionResults
        {
            get { return conventionResults; }
        }

        public int Passed { get { return conventionResults.Sum(result => result.Passed); } }

        public int Failed { get { return conventionResults.Sum(result => result.Failed); } }

        public int Skipped { get { return conventionResults.Sum(result => result.Skipped); } }

        public int Total
        {
            get { return Passed + Failed + Skipped; }
        }
    }

    [Serializable]
    public class ConventionResult
    {
        readonly List<ClassResult> classResults;

        public ConventionResult(string name)
        {
            classResults = new List<ClassResult>();
            Name = name;
        }

        public void Add(ClassResult classResult)
        {
            classResults.Add(classResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(classResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<ClassResult> ClassResults
        {
            get { return classResults; }
        }

        public int Passed { get { return classResults.Sum(result => result.Passed); } }

        public int Failed { get { return classResults.Sum(result => result.Failed); } }

        public int Skipped { get { return classResults.Sum(result => result.Skipped); } }
    }

    [Serializable]
    public class ClassResult
    {
        readonly List<CaseResult> caseResults;

        public ClassResult(string name)
        {
            caseResults = new List<CaseResult>();
            Name = name;
        }

        public void Add(CaseResult caseResult)
        {
            caseResults.Add(caseResult);
        }

        public string Name { get; private set; }

        public TimeSpan Duration
        {
            get { return new TimeSpan(caseResults.Sum(result => result.Duration.Ticks)); }
        }

        public IReadOnlyList<CaseResult> CaseResults
        {
            get { return caseResults; }
        }

        public int Passed { get { return caseResults.Count(result => result.Status == CaseStatus.Passed); } }

        public int Failed { get { return caseResults.Count(result => result.Status == CaseStatus.Failed); } }

        public int Skipped { get { return caseResults.Count(result => result.Status == CaseStatus.Skipped); } }
    }

    [Serializable]
    public class CaseResult
    {
        public CaseResult(string name, CaseStatus status, TimeSpan duration)
        {
            Name = name;
            Status = status;
            Duration = duration;
        }

        public string Name { get; private set; }
        public CaseStatus Status { get; private set; }
        public TimeSpan Duration { get; private set; }
    }

    [Serializable]
    public enum CaseStatus
    {
        Passed,
        Failed,
        Skipped
    }
}