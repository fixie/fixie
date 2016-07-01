The *.nuspec files in this folder were originally created
with the following commands, and then manually edited to
package their respective DLLs:

	..\tools\NuGet.exe spec TestDriven.Framework
	..\tools\NuGet.exe spec Microsoft.VisualStudio.TestPlatform.ObjectModel

From those nuspec files, the nupkg files can be regenerated
with the following commands:

    ..\tools\NuGet.exe pack TestDriven.Framework.nuspec
    ..\tools\NuGet.exe pack Microsoft.VisualStudio.TestPlatform.ObjectModel.nuspec

Because the root nuget.config lists this folder as a
package source, these packages can be referenced by
projects in the solution.