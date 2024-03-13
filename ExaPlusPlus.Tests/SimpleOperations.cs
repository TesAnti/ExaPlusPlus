using ExaPlusPlus.Language;

namespace ExaPlusPlus.Tests
{
    public class Tests
    {

        [Test]
        public void ShouldGenerateExaCodeFromStatements()
        {
            var input = @"
grab 1;
drop;
";
            var expected = @"
GRAB 1
DROP
".Replace("\r","");

            
            var result = Compile(input);
            
            Assert.AreEqual(expected.Trim(), result.Trim());
        }

        private static string Compile(string input)
        {
            var parser = new Parser();
            var result = parser.Parse(input);
            var compiler = new Compiler();
            result.Accept(compiler);
            return compiler.Compilation.ToString();
        }
    }
}