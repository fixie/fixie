@echo off

powershell -NoProfile -ExecutionPolicy Bypass -Command "& '.\build.ps1' %*;"