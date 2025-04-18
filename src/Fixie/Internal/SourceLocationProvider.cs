using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Fixie.Internal;

class SourceLocationProvider
{
    readonly string assemblyPath;
    Dictionary<string, Dictionary<string, SourceLocation>>? sourceLocations;

    public SourceLocationProvider(string assemblyPath)
    {
        this.assemblyPath = assemblyPath;
    }

    public bool TryGetSourceLocation(string test, [NotNullWhen(true)] out SourceLocation? sourceLocation)
    {
        if (sourceLocations == null)
            sourceLocations = CacheLocations(assemblyPath);

        sourceLocation = null;

        if (TryParse(test, out var className, out var methodName))
            if (sourceLocations.TryGetValue(StandardizeTypeName(className), out var type))
                if (type.TryGetValue(methodName, out var firstOverloadLocation))
                    sourceLocation = firstOverloadLocation;

        return sourceLocation != null;
    }

    static bool TryParse(string fullyQualifiedMethodName, [NotNullWhen(true)] out string? className, [NotNullWhen(true)] out string? methodName)
    {
        var indexOfMemberSeparator = fullyQualifiedMethodName.LastIndexOf(".");

        if (indexOfMemberSeparator >= 0)
        {
            className = fullyQualifiedMethodName.Substring(0, indexOfMemberSeparator);
            methodName = fullyQualifiedMethodName.Substring(indexOfMemberSeparator + 1);

            if (className.Length > 0 && methodName.Length > 0)
                return true;
        }

        className = null;
        methodName = null;
        return false;
    }

    static Dictionary<string, Dictionary<string, SourceLocation>> CacheLocations(string assemblyPath)
    {
        var readerParameters = new ReaderParameters { ReadSymbols = true };
        using var module = ModuleDefinition.ReadModule(assemblyPath, readerParameters);

        return module.GetTypes().Where(type => type.IsClass).ToDictionary(type => type.FullName, MethodLocations);
    }

    static Dictionary<string, SourceLocation> MethodLocations(TypeDefinition type)
    {
        Dictionary<string, SourceLocation> methodLocations = [];

        foreach (var method in type.GetMethods())
        {
            if (!method.IsAbstract)
            {
                var location = FirstOrDefaultSourceLocation(method);

                if (location != null && !methodLocations.ContainsKey(method.Name))
                    methodLocations[method.Name] = location;
            }
        }

        return methodLocations;
    }

    static SourceLocation? FirstOrDefaultSourceLocation(MethodDefinition method)
    {
        var sequencePoint = FirstOrDefaultSequencePoint(method);

        if (sequencePoint != null)
            return new SourceLocation
            {
                CodeFilePath = sequencePoint.Document.Url,
                LineNumber = sequencePoint.StartLine
            };
            
        return null;
    }

    static SequencePoint? FirstOrDefaultSequencePoint(MethodDefinition testMethod)
    {
        if (TryGetAsyncStateMachineAttribute(testMethod, out var asyncStateMachineAttribute))
            testMethod = GetStateMachineMoveNextMethod(asyncStateMachineAttribute);

        return FirstOrDefaultUnhiddenSequencePoint(testMethod.Body);
    }

    static bool TryGetAsyncStateMachineAttribute(MethodDefinition method, [NotNullWhen(true)] out CustomAttribute? attribute)
    {
        attribute = method.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == nameof(AsyncStateMachineAttribute));
        return attribute != null;
    }

    static MethodDefinition GetStateMachineMoveNextMethod(CustomAttribute asyncStateMachineAttribute)
    {
        var stateMachineType = (TypeDefinition)asyncStateMachineAttribute.ConstructorArguments[0].Value;
        var stateMachineMoveNextMethod = stateMachineType.GetMethods().First(m => m.Name == "MoveNext");
        return stateMachineMoveNextMethod;
    }

    static SequencePoint? FirstOrDefaultUnhiddenSequencePoint(MethodBody? body)
    {
        const int lineNumberIndicatingHiddenLine = 16707566; //0xfeefee

        if (body != null)
        {
            foreach (var instruction in body.Instructions)
            {
                var sequencePoint = body.Method.DebugInformation.GetSequencePoint(instruction);
                if (sequencePoint != null && sequencePoint.StartLine != lineNumberIndicatingHiddenLine)
                {
                    return sequencePoint;
                }
            }
        }

        return null;
    }

    static string StandardizeTypeName(string className)
    {
        //Mono.Cecil respects ECMA-335 for the FullName of a type, which can differ from Type.FullName.
        //In order to make reliable comparisons between the class part of a test name, the class part
        //must be standardized to the ECMA-335 format.
        //
        //ECMA-335 specifies "/" instead of "+" to indicate a nested type.

        return className.Replace("+", "/");
    }
}