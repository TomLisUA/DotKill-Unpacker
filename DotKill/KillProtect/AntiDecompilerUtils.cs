using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace DotKill.KillProtect
{
    public static class AntiDecompilerUtils
    {
        private static readonly OpCode[] OpCodeList = { OpCodes.Call, OpCodes.Sizeof, OpCodes.Calli };

        internal static bool DetectCallSizeOfCalli(MethodDef method)
        {
            foreach (var exceptionHandler in method.Body.ExceptionHandlers)
            {
                if (OpCodeList.Contains(exceptionHandler.TryStart.OpCode) && exceptionHandler.TryStart.Operand == null)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool DetectCallUnaligned(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i].IsBr() && instructions[i + 1].OpCode.Code == Code.Unaligned)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool DetectCallConstrained(MethodDef method)
        {
            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i].IsBr() && instructions[i + 1].OpCode == OpCodes.Constrained)
                {
                    return true;
                }
            }
            return false;
        }

        internal static void CallSizeOfCalli(MethodDef method)
        {
            if (!DetectCallSizeOfCalli(method)) return;

            var exceptionHandlers = method.Body.ExceptionHandlers;
            foreach (var exceptionHandler in exceptionHandlers.ToArray())
            {
                if (OpCodeList.Contains(exceptionHandler.TryStart.OpCode) && exceptionHandler.TryStart.Operand == null)
                {
                    var instructions = method.Body.Instructions;
                    int endIndex = instructions.IndexOf(exceptionHandler.TryEnd);
                    for (int i = 0; i < endIndex; i++)
                    {
                        instructions[i].OpCode = OpCodes.Nop;
                    }
                    exceptionHandlers.Remove(exceptionHandler);
                }
            }
        }

        internal static void CallUnaligned(MethodDef method)
        {
            if (!DetectCallUnaligned(method)) return;

            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i].IsBr() && instructions[i + 1].OpCode.Code == Code.Unaligned)
                {
                    instructions.RemoveAt(i + 1);
                }
            }
        }

        internal static void CallConstrained(MethodDef method)
        {
            if (!DetectCallConstrained(method)) return;

            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i].IsBr() && instructions[i + 1].OpCode == OpCodes.Constrained)
                {
                    instructions.RemoveAt(i + 1);
                }
            }
        }
    }
}