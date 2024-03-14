using System.Text;
using ExaPlusPlus.Language;
using static ExaPlusPlus.Tests.CompileHelper;

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

            
            var result = Compile(input);
            
            Assert.AreEqual(expected.Trim(), result.Trim());
        }

        [Test]
        // used to generate wiki documentation
        public void ShouldCompileAllStatements()
        {
            StringBuilder sb = new();

            //// simple statements
            //sb.AppendLine(CombineForWiki("grab 100;"));
            //sb.AppendLine(CombineForWiki("link 100;"));
            //sb.AppendLine(CombineForWiki("drop;"));
            //sb.AppendLine(CombineForWiki("make;"));
            //sb.AppendLine(CombineForWiki("seek 100;"));
            //sb.AppendLine(CombineForWiki("swiz x 1234;"));
            //sb.AppendLine(CombineForWiki("kill;"));
            //sb.AppendLine(CombineForWiki("mode;"));

            //// advanced statements
            //sb.AppendLine(CombineForWiki("send 100;"));
            //sb.AppendLine(CombineForWiki("write 100;"));
            //sb.AppendLine(CombineForWiki("cut;"));
            //sb.AppendLine(CombineForWiki("die;"));
            //sb.AppendLine(CombineForWiki("wait;"));

            //sb.AppendLine(CombineForWiki("x = read;"));
            //sb.AppendLine(CombineForWiki("x = recv;"));
            
            //sb.AppendLine(CombineForWiki("x = 2+3*4"));

            // control flow

            sb.AppendLine(Compile(@"

exa test{
  x=1;
}
spawn test;
"));

            var result= sb.ToString();



            Assert.Pass();
        }

 

        private string CombineForWiki(string input)
        {
            input = input.Trim();
            // replace newlines with <br>
            
            var output = Compile(input);
            output = output.Trim();
            output = output
                .Replace("\r","")
                .Replace("\n", "")
                .Replace("HALT","");

            string res="```"+input+"```|```"+output+"```";
            
            return res;
        }
        
    }
}