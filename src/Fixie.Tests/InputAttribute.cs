﻿namespace Fixie.Tests;

using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InputAttribute : Attribute
{
    public InputAttribute(params object?[] parameters)
        => Parameters = parameters;

    public object?[] Parameters { get; }
}