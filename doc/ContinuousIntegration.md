## How do I report results to my continuous integration server?

By default, when the console runner is invoked by TeamCity, the console output is formatted so that TeamCity can detect individual test results for display.

You can generate familiar NUnit- or xUnit-style XML reports by including an extra command line argument. These file formats are often supported by other CI tools:

    Fixie.Console.exe path/to/your/test/project.dll --NUnitXml TestResult.xml
    
or

    Fixie.Console.exe path/to/your/test/project.dll --XUnitXml TestResult.xml

If you opt into an XML report format under TeamCity, you may experience your tests being doubly-reported: once from the XML file, and once from the TeamCity-specific console output formatting.  In this case, you can explicitly suppress the TeamCity console output:

    Fixie.Console.exe path/to/your/test/project.dll  --NUnitXml TestResult.xml --TeamCity off
