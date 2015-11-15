﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class TeamCityListener :
        IHandler<AssemblyStarted>,
        IHandler<CaseSkipped>,
        IHandler<CasePassed>,
        IHandler<CaseFailed>,
        IHandler<AssemblyCompleted>
    {
        public void Handle(AssemblyStarted message)
        {
            Message("testSuiteStarted name='{0}'", SuiteName(message.Assembly));
        }

        public void Handle(CaseSkipped message)
        {
            Message("testIgnored name='{0}' message='{1}'", message.Name, message.SkipReason);
        }

        public void Handle(CasePassed message)
        {
            Message("testStarted name='{0}'", message.Name);
            Output(message.Name, message.Output);
            Message("testFinished name='{0}' duration='{1}'", message.Name, DurationInMilliseconds(message.Duration));
        }

        public void Handle(CaseFailed message)
        {
            Message("testStarted name='{0}'", message.Name);
            Output(message.Name, message.Output);
            Message("testFailed name='{0}' message='{1}' details='{2}'", message.Name, message.Exceptions.PrimaryException.Message, message.Exceptions.CompoundStackTrace);
            Message("testFinished name='{0}' duration='{1}'", message.Name, DurationInMilliseconds(message.Duration));
        }

        public void Handle(AssemblyCompleted message)
        {
            Message("testSuiteFinished name='{0}'", SuiteName(message.Assembly));
        }

        static void Message(string format, params string[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity["+format+"]", encodedArgs);
        }

        static void Output(string name, string output)
        {
            if (!String.IsNullOrEmpty(output))
                Message("testStdOut name='{0}' out='{1}'", name, output);
        }

        static string Encode(string value)
        {
            if (value == null)
                return "";

            var builder = new StringBuilder();
            
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '|': builder.Append("||"); break;
                    case '\'': builder.Append("|'"); break;
                    case '[': builder.Append("|["); break;
                    case ']': builder.Append("|]"); break;
                    case '\n': builder.Append("|n"); break; // Line Feed
                    case '\r': builder.Append("|r"); break; // Carriage Return
                    case '\u0085': builder.Append("|x"); break;  // Next Line
                    case '\u2028': builder.Append("|l"); break;  // Line Separator
                    case '\u2029': builder.Append("|p"); break;  // Paragraph Separator
                    default: builder.Append(ch); break;
                }
            }

            return builder.ToString();
        }

        static string DurationInMilliseconds(TimeSpan duration)
        {
            return ((int)Math.Ceiling(duration.TotalMilliseconds)).ToString();
        }

        static string SuiteName(Assembly assembly)
        {
            return Path.GetFileName(assembly.Location);
        }
    }
}