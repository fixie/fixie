﻿using System.Text.RegularExpressions;
using Fixie.Reports;

namespace Fixie.Tests.Reports;

public class TeamCityReportTests : MessagingTests
{
    public async Task ShouldReportResultsToTheConsoleInTeamCityFormat()
    {
        var eol = Environment.NewLine == "\r\n" ? "|r|n" : "|n";

        var output = await Run(environment => new TeamCityReport(environment));

        Regex.Replace(output.Console.NormalizeLineNumbers(), @"duration='\d+'", "duration='#'")
            .ShouldBe(
                $"""
                 ##teamcity[testSuiteStarted name='Fixie.Tests']
                 ##teamcity[testStarted name='{TestClass}.Fail']
                 ##teamcity[testFailed name='{TestClass}.Fail' message='|'Fail|' failed!' details='Fixie.Tests.FailureException{eol}{At("Fail()")}']
                 ##teamcity[testFinished name='{TestClass}.Fail' duration='#']
                 ##teamcity[testStarted name='{TestClass}.FailByAssertion']
                 ##teamcity[testFailed name='{TestClass}.FailByAssertion' message='x should be{eol}{eol}    2{eol}{eol}but was{eol}{eol}    1' details='Fixie.Assertions.ComparisonException{eol}{At("FailByAssertion()")}']
                 ##teamcity[testFinished name='{TestClass}.FailByAssertion' duration='#']
                 ##teamcity[testStarted name='{TestClass}.Pass']
                 ##teamcity[testFinished name='{TestClass}.Pass' duration='#']
                 ##teamcity[testStarted name='{TestClass}.Skip']
                 ##teamcity[testIgnored name='{TestClass}.Skip' message='|0x26a0 Skipped with attribute.']
                 ##teamcity[testFinished name='{TestClass}.Skip' duration='#']
                 ##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.String>("A")']
                 ##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.String>("A")' duration='#']
                 ##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.String>("B")']
                 ##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.String>("B")' duration='#']
                 ##teamcity[testStarted name='{GenericTestClass}.ShouldBeString<System.Int32>(123)']
                 ##teamcity[testFailed name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' message='genericArgument should match the type pattern{eol}{eol}    is string{eol}{eol}but was{eol}{eol}    int' details='Fixie.Assertions.AssertException{eol}{At<SampleGenericTestClass>("ShouldBeString|[T|](T genericArgument)")}']
                 ##teamcity[testFinished name='{GenericTestClass}.ShouldBeString<System.Int32>(123)' duration='#']
                 ##teamcity[testSuiteFinished name='Fixie.Tests']
                 
                 """);
    }
}