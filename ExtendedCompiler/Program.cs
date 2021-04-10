using ExtendedCompiler.Extensions;
using Mono.Cecil;

namespace ExtendedCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var assemblyPath = args[0];
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

            FieldOf.CecilFixup(assemblyDefinition);
            MethodOf.CecilFixup(assemblyDefinition);

            assemblyDefinition.Write(assemblyPath + "_patched");
        }
    }
}
