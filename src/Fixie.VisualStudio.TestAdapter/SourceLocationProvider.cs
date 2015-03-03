using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Fixie.VisualStudio.TestAdapter
{
    public class SourceLocationProvider
    {
        readonly IDictionary<string, TypeDefinition> types;

        public SourceLocationProvider(string assemblyFileName)
        {
            var readerParameters = new ReaderParameters { ReadSymbols = true };
            var module = ModuleDefinition.ReadModule(assemblyFileName, readerParameters);
            
            types = new Dictionary<string, TypeDefinition>();

            foreach (var type in module.GetTypes())
                types[type.FullName] = type;
        }

        public bool TryGetSourceLocation(MethodGroup methodGroup, out SourceLocation sourceLocation)
        {
            sourceLocation = null;

            var className = methodGroup.Class;
            var methodName = methodGroup.Method;

            SequencePoint sequencePoint;

            if (TryGetSequencePoint(className, methodName, out sequencePoint))
                sourceLocation = new SourceLocation(sequencePoint.Document.Url, sequencePoint.StartLine);

            return sourceLocation != null;
        }

        bool TryGetSequencePoint(string className, string methodName, out SequencePoint sequencePoint)
        {
            sequencePoint = null;

            MethodDefinition testMethod;
            if (TryGetMethod(className, methodName, out testMethod))
            {
                CustomAttribute asyncStateMachineAttribute;

                if (TryGetAsyncStateMachineAttribute(testMethod, out asyncStateMachineAttribute))
                    testMethod = GetStateMachineMoveNextMethod(asyncStateMachineAttribute);

                sequencePoint = FirstUnhiddenSequencePoint(testMethod.Body);
            }
            
            return sequencePoint != null;
        }

        bool TryGetMethod(string className, string methodName, out MethodDefinition method)
        {
            method = null;
            TypeDefinition type;

            if (TryGetType(className, out type))
            {
                var matches = type.GetMethods().Where(m => m.Name == methodName).ToArray();

                if (matches.Length == 1)
                    method = matches[0];
            }

            return method != null;
        }

        bool TryGetType(string className, out TypeDefinition type)
        {
            return types.TryGetValue(StandardizeTypeName(className), out type);
        }

        static bool TryGetAsyncStateMachineAttribute(MethodDefinition method, out CustomAttribute attribute)
        {
            attribute = method.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == "AsyncStateMachineAttribute");
            return attribute != null;
        }

        static MethodDefinition GetStateMachineMoveNextMethod(CustomAttribute asyncStateMachineAttribute)
        {
            var stateMachineType = (TypeDefinition)asyncStateMachineAttribute.ConstructorArguments[0].Value;
            var stateMachineMoveNextMethod = stateMachineType.GetMethods().First(m => m.Name == "MoveNext");
            return stateMachineMoveNextMethod;
        }

        static SequencePoint FirstUnhiddenSequencePoint(MethodBody body)
        {
            const int lineNumberIndicatingHiddenLine = 16707566; //0xfeefee

            foreach (var instruction in body.Instructions)
                if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine != lineNumberIndicatingHiddenLine)
                    return instruction.SequencePoint;

            return null;
        }

        static string StandardizeTypeName(string className)
        {
            //Mono.Cecil respects ECMA-335 for the FullName of a type, which can differ from Type.FullName.
            //In order to make reliable comparisons between the class part of a MethodGroup, the class part
            //must be standardized to the ECMA-335 format.
            //
            //ECMA-335 specifies "/" instead of "+" to indicate a nested type.

            return className.Replace("+", "/");
        }
    }
}