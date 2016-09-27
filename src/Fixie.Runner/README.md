# Fixie's `dotnet test` Runner

## Introduction

This project provides both a console runner as well as integration with the Visual Studio
Test Explorer window, for test assemblies that are build with .NET Core project types.

For pre-.NET Core project types, the legacy Test Explorer plugin is no longer supported.
For that, use Fixie 1.x which contains Fixie.VisualStudio.TestAdapter.dll. Due to Visual
Studio's problematic handling of such plugins, especially in the presence of modern
`dotnet test` integration, legacy project support was removed from Fixie 2.x.

The runner may be invoked by end users at the command line, or by IDEs. Command line arguments
determine which mode we are in. The presence of --port ##### indicates that an IDE is making
the request. IDEs also redundantly send --designtime when they send --port:

1) IDE requests test discovery:
    --designtime --port ##### --list

2) IDE requests test execution:
    --designtime --port #### --wait-command

3) Console mode is assumed whenever --port is omitted. When
   --port is omitted, the following arguments would make no
   sense: --designtime, --wait-command.

Visual Studio's test running infrastructure has several fundamental flaws
which limit integration with test frameworks. Fixie's implementation relies
on a few reasonable compromises, which in unision provide a safe-by-default
set of behaviors, no UI glitches in the Test Explorer window, and minimal
surprises in the behavior of the right-click context menu's "Run Tests" option.

## Visual Studio's Assumptions and Limitations

1. In a code editor, the right-click context menu can provide a "Run Tests"
option when a test framework provides accurate line number information for
discovered test methods, and when the discovered tests'
`FullyQualifiedName` meets certain undocumented restrictions.
Experimentation shows that `FullyQualifiedName` may be of the
form `<full-name-of-test-class>.<method-name>`. Straying from that format
in any way, such as placing argument information like `()` or `(1,2)` at
the end, will break the right-click "Run Tests" behavior.
2. Even when these rules are followed, Visual Studio's right click "Run
Tests" behavior is broken in the presence of overloaded test methods.
3. Visual Studio has poor support for parameterized test methods, for which
the arguments are not known ahead of execution time. It assumes that
`FullyQualifiedName` values will pefectly match between the discovery
phase and the execution phase. Otherwise, you get a glitchy experience as
Visual Studio tries and fails to match up actual execution results
with the list of tests found at discovery time.

## Fixie's Compromises

First, recall that a "method group" in .NET terms is a class name + method
name, omitting any information about return type or parameters.

For overloaded test methods, Fixie deliberately reports the same line number for
each overload in a method group.  In other words, Fixie reports a line number for
the method group of the method that the user right-clicked on.  Specifically,
Fixie reports the line number of the first occurrence of the method name that
the user selects.

For inherited test methods, Fixie deliberately reports no line number.  The
attempt is ambiguous, and Visual Studio cannot meaningfully use this information.
Inherited tests are still correctly discovered and appear in Test Explorer,
so they can be executed by selecting them in Test Explorer like any other test.

To meet the needs of both overloads and parameterized test methods, Fixie
registers one Visual Studio `Test` instance *per method group*, instead
of per method.  It does so by setting the `FullyQualifiedName` to
the method group of a test case, and by only reporting each discovered
method group once during discovery.

Thankfully, Visual Studio can be given any number of `TestResult` objects for
the same `Test.FullyQualifiedName`, and Fixie
can give each `TestResult` an arbitrary `DisplayName` including parameter
value information.  This combination allows the Visual Studio test runner to
display each individual test case's success or failure, grouping parameterized
and overloaded cases under the method name, while avoiding glitches for
dynamically-generated test case parameters.  In other words, a Visual Studio
"Test" is really a method group name, and a Visual Studio "Test Result"
is really an individual invocation of some method in that method group.

## "Won't Fix" Bugs

Even with these compromises, the user may experience surprises in atypical
situations, due to the limitations of the Visual Studio testing infrastructure.
        
Because the main list of discovered test cases is really a list of discovered
test method groups, the count will appear smaller than the number of actual
test results, in the presence of parameterized or overloaded test methods.
Underneath each method group in the list, though, each individual method
invocation is correctly reported.

Right clicking overloaded test methods may surprisingly run *more* than you
intended, as it will really run the whole method group.
        
Right clicking on inherited test methods will fail to successfully execute
the intended test, since the request is ambiguous.