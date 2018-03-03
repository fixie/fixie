namespace Fixie.VisualStudio.TestAdapter
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class SourceLocationProvider
    {
        readonly string assemblyPath;
        IDictionary<string, TypeDefinition> types;

        public SourceLocationProvider(string assemblyPath)
        {
            this.assemblyPath = assemblyPath;
            types = null;
        }

        public bool TryGetSourceLocation(MethodGroup methodGroup, out SourceLocation sourceLocation)
        {
            if (types == null)
                types = CacheTypes(assemblyPath);

            var className = methodGroup.Class;
            var methodName = methodGroup.Method;

            sourceLocation = GetMethods(className)
                .Where(m => m.Name == methodName)
                .Select(FirstOrDefaultSequencePoint)
                .Where(x => x != null)
                .OrderBy(x => x.StartLine)
                .Select(x => new SourceLocation(x.Document.Url, x.StartLine))
                .FirstOrDefault();

            return sourceLocation != null;
        }

        static IDictionary<string, TypeDefinition> CacheTypes(string assemblyPath)
        {
            var readerParameters = new ReaderParameters { ReadSymbols = true, InMemory = true };
            var module = ModuleDefinition.ReadModule(assemblyPath, readerParameters);

            var types = new Dictionary<string, TypeDefinition>();

            foreach (var type in module.GetTypes())
                types[type.FullName] = type;

            return types;
        }

        IEnumerable<MethodDefinition> GetMethods(string className)
            => types.TryGetValue(StandardizeTypeName(className), out TypeDefinition type)
                ? type.GetMethods()
                : Enumerable.Empty<MethodDefinition>();

        static SequencePoint FirstOrDefaultSequencePoint(MethodDefinition testMethod)
        {
            CustomAttribute asyncStateMachineAttribute;

            if (TryGetAsyncStateMachineAttribute(testMethod, out asyncStateMachineAttribute))
                testMethod = GetStateMachineMoveNextMethod(asyncStateMachineAttribute);

            return FirstOrDefaultUnhiddenSequencePoint(testMethod.Body);
        }

        static bool TryGetAsyncStateMachineAttribute(MethodDefinition method, out CustomAttribute attribute)
        {
            attribute = method.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == typeof(AsyncStateMachineAttribute).Name);
            return attribute != null;
        }

        static MethodDefinition GetStateMachineMoveNextMethod(CustomAttribute asyncStateMachineAttribute)
        {
            var stateMachineType = (TypeDefinition)asyncStateMachineAttribute.ConstructorArguments[0].Value;
            var stateMachineMoveNextMethod = stateMachineType.GetMethods().First(m => m.Name == "MoveNext");
            return stateMachineMoveNextMethod;
        }

        static SequencePoint FirstOrDefaultUnhiddenSequencePoint(MethodBody body)
        {
            const int lineNumberIndicatingHiddenLine = 16707566; //0xfeefee

            foreach (var instruction in body.Instructions)
            {
                var sequencePoint = body.Method.DebugInformation.GetSequencePoint(instruction);
                if (sequencePoint != null && sequencePoint.StartLine != lineNumberIndicatingHiddenLine)
                {
                    return sequencePoint;
                }
            }

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