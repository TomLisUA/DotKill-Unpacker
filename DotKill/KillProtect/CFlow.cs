using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace DotKill.KillProtect
{
    class CFlow
    {
        public static int removed = 0;

        public static void RemoveUselessNops(MethodDef method)
        {
            IList<Instruction> instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                while (instructions[i].OpCode == OpCodes.Nop && !IsNopTarget(method, instructions[i]))
                {
                    method.Body.Instructions.RemoveAt(i);
                    if (i >= instructions.Count)
                    {
                        break;
                    }
                }
            }
        }

        private static bool IsNopTarget(MethodDef method, Instruction nopInstr)
        {
            return IsNopBranchTarget(method, nopInstr) || IsNopSwitchTarget(method, nopInstr) || IsNopExceptionHandlerTarget(method, nopInstr);
        }

        private static bool IsNopExceptionHandlerTarget(MethodDef method, Instruction nopInstr)
        {
            if (!method.Body.HasExceptionHandlers)
            {
                return false;
            }
            return method.Body.ExceptionHandlers.Any(exceptionHandler =>
                exceptionHandler.FilterStart == nopInstr ||
                exceptionHandler.HandlerEnd == nopInstr ||
                exceptionHandler.HandlerStart == nopInstr ||
                exceptionHandler.TryEnd == nopInstr ||
                exceptionHandler.TryStart == nopInstr);
        }

        private static bool IsNopSwitchTarget(MethodDef method, Instruction nopInstr)
        {
            return method.Body.Instructions
                .Where(t => t.OpCode.OperandType == OperandType.InlineSwitch && t.Operand != null)
                .Select(t => (Instruction[])t.Operand)
                .Any(operands => operands.Contains(nopInstr));
        }

        private static bool IsNopBranchTarget(MethodDef method, Instruction nopInstr)
        {
            return method.Body.Instructions
                .Where(t => t.OpCode.OperandType == OperandType.InlineBrTarget || (t.OpCode.OperandType == OperandType.ShortInlineBrTarget && t.Operand != null))
                .Select(t => (Instruction)t.Operand)
                .Any(instruction => instruction == nopInstr);
        }

        public static int Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes().Where(t => t.HasMethods))
            {
                foreach (var method in type.Methods.Where(m => m.HasBody && m.Body.HasInstructions && m.Body.HasVariables))
                {
                    RemoveLocalVariables(method, module);
                    RemoveUselessNops(method);
                    method.Body.SimplifyBranches();
                    RemovePatternInstructions(method);
                    method.Body.OptimizeBranches();
                    RemoveUselessNops(method);
                }
            }
            return removed;
        }

        private static void RemoveLocalVariables(MethodDef method, ModuleDefMD module)
        {
            var variablesToRemove = method.Body.Variables
                .Where(v => v.Type == module.ImportAsTypeSig(typeof(InsufficientMemoryException)))
                .ToArray();

            foreach (var variable in variablesToRemove)
            {
                method.Body.Variables.Remove(variable);
                removed++;
            }
        }

        private static void RemovePatternInstructions(MethodDef method)
        {
            IList<Instruction> instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count - 12; i++)
            {
                if (instructions[i].IsLdcI4() && instructions[i + 1].IsStloc() &&
                    instructions[i + 2].IsLdcI4() && instructions[i + 3].IsLdcI4() &&
                    instructions[i + 4].IsLdcI4() && instructions[i + 5].OpCode == OpCodes.Xor &&
                    instructions[i + 6].IsLdcI4() && instructions[i + 8].IsLdcI4() &&
                    instructions[i + 9].IsStloc() && instructions[i + 12].OpCode == OpCodes.Nop)
                {
                    i++;
                    while (instructions[i].OpCode != OpCodes.Nop)
                    {
                        method.Body.Instructions.RemoveAt(i);
                        removed++;
                    }
                }
            }
        }
    }
}