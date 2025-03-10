using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using de4dot.blocks;
using de4dot.blocks.cflow;

namespace DotKill.KillProtect
{
    class de4dot_blocks
    {
        public static string Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method == null) continue;

                    try
                    {
                        DeobfuscateMethod(method);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to deobfuscate method {method.Name} in type {type.Name}: {ex.Message}");
                        continue;
                    }
                }
            }
            return "N/A";
        }

        private static void DeobfuscateMethod(MethodDef method)
        {
            var blocksCflowDeobfuscator = new BlocksCflowDeobfuscator();
            var blocks = new Blocks(method);

            blocksCflowDeobfuscator.Initialize(blocks);
            blocksCflowDeobfuscator.Deobfuscate();
            blocks.RepartitionBlocks();

            blocks.GetCode(out IList<Instruction> instructions, out IList<ExceptionHandler> exceptionHandlers);
            DotNetUtils.RestoreBody(method, instructions, exceptionHandlers);
        }
    }
}