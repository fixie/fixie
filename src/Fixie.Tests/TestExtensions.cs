﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Fixie.Execution;
using Fixie.Internal;

namespace Fixie.Tests
{
    public static class TestExtensions
    {
        const BindingFlags InstanceMethods = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static MethodInfo GetInstanceMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, InstanceMethods);
        }

        public static IReadOnlyList<MethodInfo> GetInstanceMethods(this Type type)
        {
            return type.GetMethods(InstanceMethods);
        }

        public static IEnumerable<string> Lines(this RedirectedConsole console)
        {
            return console.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void Run(this Type sampleTestClass, Listener listener, Convention convention)
        {
            new Runner(listener).RunTypes(sampleTestClass.Assembly, convention, sampleTestClass);
        }

        public static string ReplaceTime(this string original, string patternFormat, string replacement)
        {
            var decSepLocal = GetLocalDecimalSeparator();
            return original.ReplaceTime(patternFormat, replacement, decSepLocal);
        }

        static string GetLocalDecimalSeparator()
        {
            return Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        static string ReplaceTime(this string original, string patternFormat, string replacement, string decSep)
        {
            var timeRegex = string.Format(@"[\d{0}]+", Regex.Escape(decSep));
            var pattern = string.Format(patternFormat, timeRegex);
            return Regex.Replace(original, pattern, replacement);
        }
    }
}