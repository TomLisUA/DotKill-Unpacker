using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace DotKill.KillProtect
{
    class AntiDe4Dot
    {
        public static int countofths = 0;
        public static int removed = 0;
        public static int removedantidedot = 0;

        public static int ExecuteDe4Dot(ModuleDefMD module)
        {
            foreach (TypeDef type in module.GetTypes().Where(t => t.FullName.Contains("Form") && t.HasInterfaces && t.Interfaces.Count == 2).ToArray())
            {
                module.Types.Remove(type);
                removedantidedot++;
            }
            return removedantidedot;
        }

        public static string Execute(ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types.Where(t => t.HasMethods))
            {
                foreach (MethodDef method in type.Methods.Where(m => m.HasBody && m.Body.HasInstructions))
                {
                    RemoveUselessNops(method);
                }
            }
            Execute2(module);
            return $"{countofths}+{removed}";
        }

        private static void RemoveUselessNops(MethodDef method)
        {
            IList<Instruction> instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                while (instructions[i].OpCode == OpCodes.Nop && !IsNopBranchTarget(method, instructions[i]) && !IsNopSwitchTarget(method, instructions[i]) && !IsNopExceptionHandlerTarget(method, instructions[i]))
                {
                    instructions.RemoveAt(i);
                    removed++;
                    if (i >= instructions.Count)
                    {
                        break;
                    }
                }
            }
        }

        private static bool IsNopExceptionHandlerTarget(MethodDef method, Instruction nopInstr)
        {
            return method.Body.HasExceptionHandlers &&
                   method.Body.ExceptionHandlers.Any(eh => eh.FilterStart == nopInstr || eh.HandlerEnd == nopInstr || eh.HandlerStart == nopInstr || eh.TryEnd == nopInstr || eh.TryStart == nopInstr);
        }

        private static bool IsNopSwitchTarget(MethodDef method, Instruction nopInstr)
        {
            return method.Body.Instructions
                .Where(instr => instr.OpCode.OperandType == OperandType.InlineSwitch && instr.Operand != null)
                .Select(instr => (Instruction[])instr.Operand)
                .Any(operands => operands.Contains(nopInstr));
        }

        private static bool IsNopBranchTarget(MethodDef method, Instruction nopInstr)
        {
            return method.Body.Instructions
                .Where(instr => instr.OpCode.OperandType == OperandType.InlineBrTarget || (instr.OpCode.OperandType == OperandType.ShortInlineBrTarget && instr.Operand != null))
                .Select(instr => (Instruction)instr.Operand)
                .Any(target => target == nopInstr);
        }

        public static void Execute2(ModuleDefMD module)
        {
            RemoveTypesWithInterfaces(module);
            RemoveSpecificCustomAttributes(module);
            RemoveTypesWithFakeObfuscators(module);
        }

        private static void RemoveTypesWithInterfaces(ModuleDefMD module)
        {
            for (int i = 0; i < module.Types.Count; i++)
            {
                TypeDef typeDef = module.Types[i];
                if (typeDef.HasInterfaces)
                {
                    for (int j = 0; j < typeDef.Interfaces.Count; j++)
                    {
                        if (typeDef.Interfaces[j].Interface != null &&
                            (typeDef.Interfaces[j].Interface.Name.Contains(typeDef.Name) || typeDef.Name.Contains(typeDef.Interfaces[j].Interface.Name)))
                        {
                            module.Types.RemoveAt(i);
                            countofths++;
                        }
                    }
                }
            }

            foreach (TypeDef type in module.Types.ToList().Where(t => t.HasInterfaces))
            {
                for (int k = 0; k < type.Interfaces.Count; k++)
                {
                    if (type.Interfaces[k].Interface.Name.Contains(type.Name) || type.Name.Contains(type.Interfaces[k].Interface.Name))
                    {
                        module.Types.Remove(type);
                        countofths++;
                    }
                }
            }
        }

        private static void RemoveSpecificCustomAttributes(ModuleDefMD module)
        {
            var attributeNames = new HashSet<string>
            {
                "ConfusedByAttribute", "ZYXDNGuarder", "YanoAttribute", "Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode",
                "SmartAssembly.Attributes.PoweredByAttribute", "SecureTeam.Attributes.ObfuscatedByAgileDotNetAttribute", "ObfuscatedByGoliath",
                "NineRays.Obfuscator.Evaluation", "EMyPID_8234_", "DotfuscatorAttribute", "CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute",
                "BabelObfuscatorAttribute", ".NETGuard", "OrangeHeapAttribute", "WTF", "<ObfuscatedByDotNetPatcher>", "SecureTeam.Attributes.ObfuscatedByCliSecureAttribute",
                "OiCuntJollyGoodDayYeHavin_____________________________________________________", "ProtectedWithCryptoObfuscatorAttribute", "NetGuard"
            };

            foreach (var attribute in module.CustomAttributes.ToList())
            {
                var type = attribute.AttributeType.ResolveTypeDef();
                if (type != null && attributeNames.Contains(type.Name))
                {
                    module.Types.Remove(type);
                    module.CustomAttributes.Remove(attribute);
                    countofths++;
                }
            }
        }

        private static void RemoveTypesWithFakeObfuscators(ModuleDefMD module)
        {
            var fakeObfuscators = new HashSet<string>
            {
                "DotNetPatcherObfuscatorAttribute", "DotNetPatcherPackerAttribute", "DotfuscatorAttribute", "ObfuscatedByGoliath", "dotNetProtector",
                "PoweredByAttribute", "AssemblyInfoAttribute", "BabelAttribute", "CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute", "Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode",
                "NineRays.Obfuscator.Evaluation", "YanoAttribute", "SmartAssembly.Attributes.PoweredByAttribute", "NetGuard", "SecureTeam.Attributes.ObfuscatedByCliSecureAttribute",
                "Reactor", "ZYXDNGuarder", "CryptoObfuscator", "MaxtoCodeAttribute", ".NETReactorAttribute", "BabelObfuscatorAttribute"
            };

            foreach (var type in module.Types.ToList())
            {
                if (fakeObfuscators.Contains(type.Name))
                {
                    module.Types.Remove(type);
                    countofths++;
                }
            }
        }
    }
}