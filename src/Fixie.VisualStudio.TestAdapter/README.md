# Fixie's Visual Studio Test Adapter

## Introduction

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
4. Visual Studio requires that new versions of the runner be installed by
installing a NuGet package.  It is not enough to simply have the dlls under
your packages folder.  As a consequence, the VS runner doesn't run on the Fixie
solution itself. See https://github.com/fixie/fixie.integration for
a sample project which can be used to test the Visual Studio Test Adapter.
5. It is highly likely that misleading errors will be reported in the Tests Output window upon upgrading.
When users of the Test Adapter upgrade to a new version Fixie, it is recommended that they remove the
old version, close Visual Studio, reopen Visual Studio, and install the new version
of Fixie.  
6. If you're having problems discovering or running tests, you may need to reset
Visual Studio's cache of runner assemblies. Shut down all instances of Visual Studio,
delete the folder `%TEMP%\VisualStudioTestExplorerExtensions`, and be sure that
your project is only linked against a single version of Fixie.

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
registers one Visual Studio TestCase instance *per method group*, instead
of per method.  It does so by setting the `TestCase.FullyQualifiedName` to
the method group of a test case. In the case of overloaded test methods,
the same method group will be reported to Visual Studio more than once
during the discovery phase, but Visual Studio detects these duplicates and handles
them properly as one, logging that fact to the Output pane.

Thankfully, Visual Studio can be given any number of `TestResult` objects for
the same `FullyQualifiedName`, and Fixie
can give each `TestResult` an arbitrary `DisplayName` including parameter
value information.  This combination allows the Visual Studio test runner to
display each individual test case's success or failure, grouping parameterized
and overloaded cases under the method name, while avoiding glitches for
dynamically-generated test case parameters.  In other words, a Visual Studio
"Test Case" is really a method group name, and a Visual Studio "Test Result"
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

There are likely other scenarios in which right clicking on test methods will
fail to successfully execute the intended test.  In all cases, the Test Explorer
window does report the truth about what actually executed.

## Development on the Test Adapter

Recommended Workflow:

1. Set up a local NuGet package source pointing at the package/ folder in the
root of your copy of the Fixie repository.
2. Implement a change within the Fixie solution.
3. `build package` at the command line to locally produce a new build of the
NuGet package.
4. Note the creation of the package file inside the package/ folder.
5. Open the `fixie.integration` solution, uninstall Fixie from all projects,
close Test Explorer, and close all instances of Visual Studio.
6. Delete the contents of `%TEMP%\VisualStudioTestExplorerExtensions` and reopen
the `fixie.integration` solution.
7. In `fixie.integration`, resinstall Fixie but from your local package
source.
8. Run tests within the `fixie.integration` solution to test the effect of
your changes.