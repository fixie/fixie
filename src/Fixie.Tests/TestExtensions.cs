﻿using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static IEnumerable<string> Lines(this string multiline)
        {
            return multiline.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void Run(this Type sampleTestClass, object listener, Convention convention)
        {
            var bus = new Bus();
            bus.Subscribe(listener);
            new Runner(bus).RunTypes(sampleTestClass.Assembly, convention, sampleTestClass);
        }
    }
}