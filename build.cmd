@echo off

.\tools\NuGet.exe restore src\Fixie.sln -source "https://nuget.org/api/v2/" -RequireConsent -o "src\packages"

powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.\build.ps1' %*;"