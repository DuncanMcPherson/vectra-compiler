using Vectra.Bytecode;

namespace Vectra.Compiler;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Vectra.Compiler <input.vec> [output.vbc]");
            return;
        }

        var inputPath = args[0];
        var outputPath = args.Length > 1 ? args[1] : Path.ChangeExtension(inputPath, ".vbc");

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file '{inputPath}' does not exist.");
            return;
        }

        var sourceCode = File.ReadAllText(inputPath);
        // TODO: Build Lexer
        // TODO: Build Parser
        // TODO: Build TypeChecker

        var writer = new BytecodeWriter();
        // TODO: Write bytecode to file
        
        Console.WriteLine($"Compiled '{inputPath}' to '{outputPath}' successfully.");
    }
}