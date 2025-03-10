using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Reflection;

namespace DotKill.KillProtect
{
    class MathProtection
    {
        public static int removed = 0;

        public static int Execute(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types.Where(t => t.HasMethods).ToArray())
            {
                foreach (MethodDef method in type.Methods.Where(m => m.HasBody && m.Body.HasInstructions).ToArray())
                {
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var instruction = method.Body.Instructions[i];
                        if (instruction.Operand == null) continue;

                        if (TryReplaceMathCall<double>(method, i, OpCodes.Ldc_R8, "(System.Double)"))
                            continue;
                        if (TryReplaceMathCall<float>(method, i, OpCodes.Ldc_R4, "(System.Single)"))
                            continue;
                        if (TryReplaceMathCall<int>(method, i, OpCodes.Ldc_I4, "(System.Int32)"))
                            continue;
                        if (TryReplaceMathCall<long>(method, i, OpCodes.Ldc_I8, "(System.Int64)"))
                            continue;
                        if (TryReplaceMathCall<bool>(method, i, OpCodes.Ldc_I4, "(System.Boolean)"))
                            continue;
                        if (TryReplaceMathCall<double>(method, i, OpCodes.Ldc_R8, "(System.Double,System.Double)", 2))
                            continue;
                        if (TryReplaceMathCall<float>(method, i, OpCodes.Ldc_R4, "(System.Single,System.Single)", 2))
                            continue;
                        if (TryReplaceMathCall<int>(method, i, OpCodes.Ldc_I4, "(System.Int32,System.Int32)", 2))
                            continue;
                        if (TryReplaceMathCall<double, int>(method, i, new[] { OpCodes.Ldc_R8, OpCodes.Ldc_I4 }, "(System.Double,System.Int32)", 2))
                            continue;
                    }
                }
            }
            return removed;
        }

        private static bool TryReplaceMathCall<T>(MethodDef method, int index, OpCode ldcOpCode, string signature, int operandCount = 1)
        {
            if (method.Body.Instructions[index].OpCode != OpCodes.Call ||
                !method.Body.Instructions[index].Operand.ToString().Contains("System.Math::") ||
                !method.Body.Instructions[index].Operand.ToString().Contains(signature))
                return false;

            MemberRef memberRef = method.Body.Instructions[index].Operand as MemberRef;
            MethodBase invoke = typeof(Math).GetMethod(memberRef.Name, Enumerable.Repeat(typeof(T), operandCount).ToArray());

            object[] args = new object[operandCount];
            for (int j = 0; j < operandCount; j++)
            {
                if (method.Body.Instructions[index - 1 - j].OpCode != ldcOpCode)
                    return false;
                args[operandCount - 1 - j] = method.Body.Instructions[index - 1 - j].Operand;
            }

            object result = invoke.Invoke(null, args);

            method.Body.Instructions[index].OpCode = ldcOpCode;
            method.Body.Instructions[index].Operand = result;
            for (int j = 0; j < operandCount; j++)
            {
                method.Body.Instructions[index - 1 - j].OpCode = OpCodes.Nop;
            }

            removed++;
            return true;
        }

        private static bool TryReplaceMathCall<T1, T2>(MethodDef method, int index, OpCode[] ldcOpCodes, string signature, int operandCount = 2)
        {
            if (method.Body.Instructions[index].OpCode != OpCodes.Call ||
                !method.Body.Instructions[index].Operand.ToString().Contains("System.Math::") ||
                !method.Body.Instructions[index].Operand.ToString().Contains(signature))
                return false;

            MemberRef memberRef = method.Body.Instructions[index].Operand as MemberRef;
            MethodBase invoke = typeof(Math).GetMethod(memberRef.Name, new Type[] { typeof(T1), typeof(T2) });

            object[] args = new object[operandCount];
            for (int j = 0; j < operandCount; j++)
            {
                if (method.Body.Instructions[index - 1 - j].OpCode != ldcOpCodes[j])
                    return false;
                args[operandCount - 1 - j] = method.Body.Instructions[index - 1 - j].Operand;
            }

            object result = invoke.Invoke(null, args);

            method.Body.Instructions[index].OpCode = ldcOpCodes[0];
            method.Body.Instructions[index].Operand = result;
            for (int j = 0; j < operandCount; j++)
            {
                method.Body.Instructions[index - 1 - j].OpCode = OpCodes.Nop;
            }

            removed++;
            return true;
        }
    }
}