using ExaPlusPlus.Language;

namespace ExaPlusPlus.Tests;

public static class CompileHelper
{
    public static string Compile(string input)
    {
        var parser = new Parser();
        var result = parser.Parse(input);
        var compiler = new Compiler();
        result.Accept(compiler);
        return compiler.Compilation.ToString();
    }
}