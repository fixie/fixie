The *.nuspec file in this folder was originally created
with the following command, and then manually edited to
package its DLL:

	..\tools\NuGet.exe spec TestDriven.Framework

From that nuspec file, the nupkg file can be regenerated
with the following command:

    ..\tools\NuGet.exe pack TestDriven.Framework.nuspec

Because the root nuget.config lists this folder as a
package source, this package can be referenced by
projects in the solution.