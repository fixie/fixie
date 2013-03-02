Framework '4.0'

properties {
  $project = 'Fixie'
  $configuration = 'Release'
  $src = resolve-path '.\src'
}

task default -depends Test

task Test -depends Compile {
    $nunitRunner = join-path $src "packages\NUnit.Runners.2.6.2\tools\nunit-console.exe"
    & $nunitRunner $src\$project.Tests\bin\$configuration\$project.Tests.dll /nologo /nodots /framework:net-4.0

    if ($lastexitcode -gt 0)
    {
        throw "{0} unit tests failed." -f $lastexitcode
    }
    if ($lastexitcode -lt 0)
    {
        throw "Unit test run was terminated by a fatal error."
    }
}

task Compile {
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
}