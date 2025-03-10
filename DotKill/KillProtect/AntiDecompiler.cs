using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace DotKill.KillProtect
{
    class AntiDecompiler
    {
        public static int processedMethodCount = 0;

        public static int Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method == null || !method.HasBody || !method.Body.HasInstructions)
                    {
                        continue;
                    }

                    try
                    {
                        AntiDecompilerPhase.Execute(method);
                        processedMethodCount++;
                    }
                    catch (Exception ex)
                    {
                        // Log the exception for debugging purposes
                        Console.WriteLine($"Error processing method {method.Name}: {ex}");
                    }
                }
            }
            return processedMethodCount;
        }
    }
}