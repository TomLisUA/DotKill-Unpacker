using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using DotKill.KillProtect;

internal class Program
{
    private static IntPtr ThisConsole = GetConsoleWindow();

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static string GetPath(string[] arguments)
    {
        const string logo = "                                     ____        _   _  ___ _ _ \r\n                                    |  _ \\  ___ | |_| |/ (_) | |\r\n                                    | | | |/ _ \\| __| ' /| | | |\r\n                                    | |_| | (_) | |_| . \\| | | |\r\n                                    |____/ \\___/ \\__|_|\\_\\_|_|_|";
        const string title = "                                    DOTNET CLEANER TOOL BY LOCKT";
        string modulePath = "";

        ShowWindow(ThisConsole, 9);
        Console.SetWindowSize(102, 22);
        Console.SetBufferSize(102, 9001);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(logo);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n{title}\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Title = "DotKill Unpacker by LockT#3341";

        if (arguments.Length == 1)
        {
            modulePath = arguments[0];
        }
        else if (arguments.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" [+] ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Path: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            modulePath = Console.ReadLine()?.Replace("\"", "");
            Console.ResetColor();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(logo);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{title}\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        return modulePath;
    }

    private static void PrintMessage(string label, string message, ConsoleColor labelColor, ConsoleColor messageColor)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" [");
        Console.ForegroundColor = labelColor;
        Console.Write(label);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("] ");
        Console.ForegroundColor = messageColor;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }

    private static void PrintRemoved(string message) => PrintMessage("Removed", message, ConsoleColor.Red, ConsoleColor.Green);

    private static void PrintTimed(string message) => PrintMessage(DateTime.Now.ToString("HH:mm:ss"), message, ConsoleColor.Blue, ConsoleColor.White);

    private static void PrintPath(string message) => PrintMessage("SavePath", message, ConsoleColor.Magenta, ConsoleColor.White);

    private static void PrintExit() => PrintMessage("Exit", "Press any key to exit.", ConsoleColor.Red, ConsoleColor.White);

    private static void Main(string[] args)
    {
        try
        {
            string loadPath = GetPath(args);
            ModuleDefMD module = ModuleDefMD.Load(loadPath);

            string junkAntiDe4Dot = AntiDe4Dot.Execute(module);
            string[] results = junkAntiDe4Dot.Split('+');
            string dedot = results[0];
            string junk = results[1];

            PrintRemoved($"Anti De4Dot ........: {dedot}");
            PrintRemoved($"Junk ...............: {junk}");
            PrintRemoved($"Maths ..............: {MathProtection.Execute(module)}");
            PrintRemoved($"Anti Decompiler ....: {AntiDecompiler.Execute(module)}");
            PrintRemoved($"CFlow ..............: {CFlow.Execute(module)}");

            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintTimed("Assembly is saving now");
            Thread.Sleep(200);
            Console.Write(".");
            Thread.Sleep(200);
            Console.Write(".");
            Thread.Sleep(200);
            Console.WriteLine(".");

            string directoryPath = Path.GetDirectoryName(loadPath);
            if (directoryPath != null && !directoryPath.EndsWith("\\"))
            {
                directoryPath += "\\";
            }
            string savePath = directoryPath + Path.GetFileNameWithoutExtension(loadPath) + "_dotkill" + Path.GetExtension(loadPath);

            module.Write(savePath, new ModuleWriterOptions(module)
            {
                PEHeadersOptions = { NumberOfRvaAndSizes = 13u },
                Logger = DummyLogger.NoThrowInstance
            });

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            PrintTimed("Assembly is saved:\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintPath(savePath);
            Console.ForegroundColor = ConsoleColor.White;
            PrintExit();
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.ResetColor();
        }
    }
}