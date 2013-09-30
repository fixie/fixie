﻿using System;
using Should;

namespace Fixie.Samples.ChangeCaseName
{
    public class CalculatorTests
    {
        readonly Calculator calculator;
        string test;

        public CalculatorTests()
        {
            calculator = new Calculator();
        }

        public void ShouldAdd()
        {
            test.ShouldBeNull();
            test = "ShouldAdd";

            calculator.Add(2, 3).ShouldEqual(5);
        }

    }
}