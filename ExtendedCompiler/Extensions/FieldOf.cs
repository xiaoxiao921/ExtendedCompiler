using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtendedCompiler.Extensions
{
    /// <summary>
    /// Class that provides <see cref="fieldof(void*)"/>
    /// </summary>
    public static class FieldOf
    {
        private static string _fieldofReference;
        internal static string FieldOfReference
        {
            get
            {
                if (_fieldofReference == null)
                {
                    var assembly = AssemblyDefinition.ReadAssembly(typeof(FieldOf).Assembly.Location);
                    var operatorExtensionsTypeDefinition = assembly.MainModule.Types.First(t => t.Name == nameof(FieldOf));
                    _fieldofReference = operatorExtensionsTypeDefinition.Methods.First(m => m.Name == nameof(fieldof)).FullName;
                }

                return _fieldofReference;
            }
        }

        private static MethodReference _getFieldFromHandleReference;
        internal static MethodReference GetFieldFromHandleReference
        {
            get
            {
                if (_getFieldFromHandleReference == null)
                {
                    var assembly = AssemblyDefinition.ReadAssembly(typeof(MethodBase).Assembly.Location);
                    var fieldInfoType = assembly.MainModule.Types.First(t => t.Name == nameof(FieldInfo));
                    _getFieldFromHandleReference = fieldInfoType.Methods.First(
                        m => m.Name == nameof(FieldInfo.GetFieldFromHandle) &&
                        m.Parameters.Count == 1 &&
                        m.Parameters[0].ParameterType.Name == nameof(RuntimeFieldHandle));
                }

                return _getFieldFromHandleReference;
            }
        }

        internal static FieldReference GetFieldReference(this TypeDefinition type, string fieldName) =>
            type.Fields.First(f => f.Name == fieldName);

        internal static void CecilFixup(AssemblyDefinition assemblyDefinition)
        {
            foreach (var type in assemblyDefinition.MainModule.Types)
            {
                foreach (var method in type.Methods)
                {
                    var methodBody = method.Body;

                    var instructionsToRemove = new List<Instruction>();
                    foreach (var instruction in methodBody.Instructions)
                    {
                        if (instruction.IsCallToFieldOf())
                        {
                            var typeDefinition = (TypeDefinition)instruction.Previous.Previous.Previous.Operand;
                            instruction.Previous.OpCode = OpCodes.Ldtoken;
                            instruction.Previous.Operand = type.Module.ImportReference(typeDefinition.GetFieldReference((string)instruction.Previous.Operand));
                            instruction.Operand = type.Module.ImportReference(GetFieldFromHandleReference);

                            instructionsToRemove.Add(instruction.Previous.Previous.Previous);
                            instructionsToRemove.Add(instruction.Previous.Previous);
                        }
                    }

                    foreach (var instructionToRemove in instructionsToRemove)
                    {
                        methodBody.Instructions.Remove(instructionToRemove);
                    }
                }
            }
        }

        private static bool IsCallToFieldOf(this Instruction instruction) =>
            (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
            instruction.Operand is MethodReference methodReference &&
            methodReference.FullName == FieldOfReference;

        /// <summary>
        /// Example for a field
        /// <code>fieldof<Program>(nameof(Program.fieldName));</code>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static unsafe FieldInfo fieldof(Type type, string fieldName)
        {
            return typeof(object).GetField(nameof(object.ToString));
        }
    }
}
