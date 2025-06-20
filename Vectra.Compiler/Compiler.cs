using Vectra.Bytecode;

namespace Vectra.Compiler;

public static class Compiler
{
    // TODO: Handle failures gracefully
    public static void Compile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine($"File '{sourcePath}' does not exist.");
            return;
        }
        // TODO: Multiple file support
        var sourceCode = File.ReadAllText(sourcePath);
        var lexer = new Lexer.Lexer(sourceCode);
        var tokens = lexer.Tokenize();

        var parser = new Parser(tokens);
        var module = parser.Parse();

        var bytecodeGenerator = new BytecodeGenerator();
        var program = bytecodeGenerator.Generate(module);
        // TODO: Support for custom output locations
        BytecodeWriter.WriteToFile(program);
    }
}