﻿namespace Fixie.Tests;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class SkipAttribute : Attribute
{
    public SkipAttribute(string reason) => Reason = reason;

    public string Reason { get; }
}