using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ExtendedCompiler.Extensions
{
    /// <summary>
    /// Class that provides <see cref="methodof(void*)"/>
    /// </summary>
    public static class MethodOf
    {
        private static string _methodofReference;
        internal static string MethodofReference
        {
            get
            {
                if (_methodofReference == null)
                {
                    var assembly = AssemblyDefinition.ReadAssembly(typeof(MethodOf).Assembly.Location);
                    var operatorExtensionsTypeDefinition = assembly.MainModule.Types.First(t => t.Name == nameof(MethodOf));
                    _methodofReference = operatorExtensionsTypeDefinition.Methods.First(m => m.Name == nameof(methodof)).FullName;
                }

                return _methodofReference;
            }
        }

        private static MethodReference _getMethodFromHandleReference;
        internal static MethodReference GetMethodFromHandleReference
        {
            get
            {
                if (_getMethodFromHandleReference == null)
                {
                    var assembly = AssemblyDefinition.ReadAssembly(typeof(MethodBase).Assembly.Location);
                    var methodBaseTypeDefinition = assembly.MainModule.Types.First(t => t.Name == nameof(MethodBase));
                    _getMethodFromHandleReference = methodBaseTypeDefinition.Methods.First(m => m.Name == nameof(MethodBase.GetMethodFromHandle));
                }

                return _getMethodFromHandleReference;
            }
        }

        internal static void CecilFixup(string assemblyPath)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

            foreach (var type in assemblyDefinition.MainModule.Types)
            {
                foreach (var method in type.Methods)
                {
                    var methodBody = method.Body;

                    foreach (var instruction in methodBody.Instructions)
                    {
                        if (instruction.IsCallToMethodOf())
                        {
                            instruction.Previous.OpCode = OpCodes.Ldtoken;
                            instruction.Operand = type.Module.ImportReference(GetMethodFromHandleReference);
                        }
                    }
                }
            }

            assemblyDefinition.Write(assemblyPath + "_patched");
        }

        private static bool IsCallToMethodOf(this Instruction instruction) =>
            (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
            instruction.Operand is MethodReference methodReference &&
            methodReference.FullName == MethodofReference;

        /// <summary>
        /// Example for a void method that has no parameter (<see cref="System.Action"/> <see cref="System.Delegate"/>) :
        /// <code>methodof((delegate*&lt;void&gt;)MethodGroup);</code>
        /// </summary>
        /// <param name="methodPointer"></param>
        /// <returns></returns>
        public static unsafe MethodBase methodof(void* methodPointer)
        {
            return typeof(object).GetMethod(nameof(object.ToString));
        }
    }
}
