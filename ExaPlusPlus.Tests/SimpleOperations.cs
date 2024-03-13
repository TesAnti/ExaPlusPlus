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
";

            
            var result = CompileHelper.Compile(input);
            
            Assert.AreEqual(expected.Trim(), result.Trim());
        }

        
        
    }
}