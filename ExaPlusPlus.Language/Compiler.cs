using System.Linq;
using System.Text;

namespace ExaPlusPlus.Language;

public class Compiler
{
    public StringBuilder Compilation { get; set; }= new StringBuilder();
    public StringBuilder AdditionalCompilation { get; } = new StringBuilder();
    public string Visit(NumericLiteral numericLiteral)
    {
        return numericLiteral.value.ToString();
    }
    public string Visit(Variable variable)
    {
        if (variable.VariableType == EVariableType.EOF)
        {
            Compilation.AppendLine("TEST EOF");
            return "T";
        }

        if (variable.VariableType == EVariableType.Host)
        {
            Compilation.AppendLine("HOST T");
            return "T";
        }

        
            
        return variable.VariableType.ToString().ToUpper();
    }

    public string Visit(EnvVariable envVar)
    {
        return envVar.name;
    }
    
    private bool IsCompareOperation(ETokenType tokenType) =>
        tokenType == ETokenType.Equals ||
        tokenType == ETokenType.NotEquals ||
        tokenType == ETokenType.Greater ||
        tokenType == ETokenType.Less;

    private bool _comparationInversionFlag = false;
    public string Visit(BinaryExpression binaryExpression)
    {
        

        if (IsCompareOperation(binaryExpression.type))
        {
            _comparationInversionFlag = false;
            string operation="";
            switch (binaryExpression.type)
            {
                case ETokenType.Equals:
                    operation = "=";
                    break;
                case ETokenType.NotEquals:
                    operation = "=";
                    _comparationInversionFlag = true;
                    break;
                case ETokenType.Greater:
                    operation = ">";
                    break;
                case ETokenType.Less:
                    operation = "<";
                    break;
            }

            var first = binaryExpression.left.Accept(this);
            var second = binaryExpression.right.Accept(this);
            Compilation.AppendLine($"TEST {first} {operation} {second}");
            return "T";

        }
        else
        {
            string command = "";
            switch (binaryExpression.type)
            {
                case ETokenType.Plus:
                    command = "ADDI";
                    break;
                case ETokenType.Minus:
                    command = "SUBI";
                    break;
                case ETokenType.Mul:
                    command = "MULI";
                    break;
                case ETokenType.Div:
                    command = "DIVI";
                    break;
                case ETokenType.Mod:
                    command = "MODI";
                    break;


            }
            
            var first = binaryExpression.left.Accept(this);
            var second = binaryExpression.right.Accept(this);
            Compilation.AppendLine(
                $"{command} {first} {second} T");
            return "T";
        }
            
    }

    public string Visit(MinusExpression minusExpression)
    {
        return $"-{minusExpression.expression.Accept(this)}";
    }

    public string Visit(NotExpression notExpression)
    {
        
        return Visit(new BinaryExpression(notExpression.expression,ETokenType.NotEquals,new NumericLiteral(1)));
    }

    public string Visit(AssignmentStatement assignmentStatement)
    {
        return $"COPY {assignmentStatement.value.Accept(this)} {assignmentStatement.Variable.Accept(this)}";
    }
    
    public string Visit(ProgramStatement programStatement)
    {
        programStatement.list.Accept(this);
        Compilation.AppendLine("HALT");
        Compilation.AppendLine(AdditionalCompilation.ToString());
        return Compilation.ToString();
    }

    public string? Visit(EXADefinitionStatement exaDefinitionStatement)
    {
        var tmp = Compilation;

        AdditionalCompilation.AppendLine("MARK " + exaDefinitionStatement.name);
        Compilation = AdditionalCompilation;
        exaDefinitionStatement.list.Accept(this);
        Compilation.AppendLine("HALT");
        Compilation= tmp;
        return null;
    }

    

    

    public string? Visit(SendStatement sendStatement)
    {
        foreach (ASTNode node in sendStatement.data)
        {
            var line = $"COPY {node.Accept(this)} M";
            Compilation.AppendLine(line);
        }
        
        return null;
    }

    public string? Visit(SpawnStatement spawnStatement)
    {
        foreach (string exa in spawnStatement.exas)
        {
            Compilation.AppendLine($"REPL {exa}");
        }
        
        return null;
    }

    public string? Visit(DeleteStatement deleteStatement)
    {
        Compilation.AppendLine("WIPE");
        return null;
    }

    public string? Visit(MakeStatement makeStatement)
    {
        Compilation.AppendLine("MAKE");
        return null;
    }

    public string? Visit(WriteStatement writeStatement)
    {
        foreach (ASTNode node in writeStatement.data)
        {
            var line = $"COPY {node.Accept(this)} F";
            Compilation.AppendLine(line);
        }
        
        return null;
    }

    public string? Visit(EmptyStatement emptyStatement)
    {
        return null;
    }

    public string? Visit(ExpressionStatement expressionStatement)
    {
        Compilation.AppendLine(expressionStatement.expression.Accept(this));
        return null;
    }

    public string? Visit(GrabStatement grabStatement)
    {
        var line = $"GRAB {grabStatement.expression.Accept(this)}";
        Compilation.AppendLine(line);
        return null;
    }

    public string? Visit(LinkStatement linkStatement)
    {
        foreach (ASTNode node in linkStatement.data)
        {
            var line = $"LINK {node.Accept(this)}";
            Compilation.AppendLine(line);
        }
            
        return null;
    }

    public string? Visit(DropStatement dropStatement)
    {
        Compilation.AppendLine("DROP");
        return null;
    }

    public string? Visit(SeekStatement seekStatement)
    {
        var line = $"SEEK {seekStatement.amount.Accept(this)}";
        Compilation.AppendLine(line);
        return null;
    }

    public string? Visit(SwizStatement swizStatement)
    {
        var line = $"SWIZ {swizStatement.expression.Accept(this)} {swizStatement.order.Accept(this)}";
        Compilation.AppendLine(line);
        return null;
    }

    public string? Visit(StatementList statementList)
    {

        foreach (var statement in statementList.statements)
        {
            var res = statement.Accept(this);
            if (res != null)
                Compilation.AppendLine(res);
        }
        return null;
    }

    int block_counter = 0;
    public string? Visit(IfStatement ifStatement)
    {
        
        var label = $"IF"+block_counter;
        Compilation.AppendLine($"; {label}");
        block_counter++;
        ifStatement.condition.Accept(this);
        if (ifStatement.elseBody != null)
        {
            if (_comparationInversionFlag)
                Compilation.AppendLine($"TJMP {label}_ELSE");
            else
                Compilation.AppendLine($"FJMP {label}_ELSE");
        }
        else
        {
            if (_comparationInversionFlag)
                Compilation.AppendLine($"TJMP {label}_END");
            else
                Compilation.AppendLine($"FJMP {label}_END");
        }
            

        ifStatement.ifBody.Accept(this);
        Compilation.AppendLine($"JUMP {label}_END");
        if (ifStatement.elseBody != null)
        {
            Compilation.AppendLine($"MARK {label}_ELSE");
            ifStatement.elseBody.Accept(this);
        }
        
        Compilation.AppendLine($"MARK {label}_END");
        return null;
    }

    Stack<string> _loopStack = new Stack<string>();
    public string? Visit(ForeverStatement foreverStatement)
    {
        var label = $"FOREVER"+block_counter;
        block_counter++;
        Compilation.AppendLine($"; {label}");
        _loopStack.Push(label);
        Compilation.AppendLine($"MARK {label}_START");
        
        foreverStatement.statements.Accept(this);
        Compilation.AppendLine($"JUMP {label}_START");
        Compilation.AppendLine($"MARK {label}_END");
        _loopStack.Pop();
        return null;
    }

    public string? Visit(BreakStatement breakStatement)
    {
        var line = $"JUMP {_loopStack.Peek()}_END";
        Compilation.AppendLine(line);
        return null;
    }

    public string? Visit(ContinueStatement continueStatement)
    {
        var line = $"JUMP {_loopStack.Peek()}_START";
        Compilation.AppendLine(line);
        return null;
    }

    public string? Visit(WhileStatement whileStatement)
    {
        var label = $"WHILE"+block_counter;
        Compilation.AppendLine($"; {label}");
        block_counter++;
        _loopStack.Push(label);
        Compilation.AppendLine($"MARK {label}_START");
        whileStatement.condition.Accept(this);
        if (_comparationInversionFlag)
            Compilation.AppendLine($"TJMP {label}_END");
        else
            Compilation.AppendLine($"FJMP {label}_END");
        whileStatement.body.Accept(this);
        Compilation.AppendLine($"JUMP {label}_START");
        Compilation.AppendLine($"MARK {label}_END");
        _loopStack.Pop();
        return null;
    }

    public string? Visit(CutStatement cutStatement)
    {
        Compilation.AppendLine($"VOID F");
        return null;
    }

    public string? Visit(KillStatement killStatement)
    {
        Compilation.AppendLine($"KILL");
        return null;
    }

    public string? Visit(DieStatement dieStatement)
    {
        Compilation.AppendLine($"HALT");
        return null;
    }

    public string Visit(ModeStatement dieStatement)
    {
        Compilation.AppendLine($"MODE");
        return null;
    }

    public string Visit(IncStatement incStatement)
    {
        Compilation.AppendLine($"ADDI {incStatement.expression.Accept(this)} 1 {incStatement.expression.Accept(this)}");
        return null;
    }

    public string Visit(DecStatement incStatement)
    {
        Compilation.AppendLine($"SUBI {incStatement.expression.Accept(this)} 1 {incStatement.expression.Accept(this)}");
        return null;
    }

    public string Visit(WaitStatement waitStatement)
    {
        Compilation.AppendLine($"VOID M");
        return null;
    }

    public string Visit(CompoundStatement compoundStatement)
    {
        switch (compoundStatement.token)
        {
            case ETokenType.CompoundPlus:
                return Visit(new AssignmentStatement(compoundStatement.variable,
                    new BinaryExpression(compoundStatement.variable, ETokenType.Plus, compoundStatement.expresion)));
            case ETokenType.CompoundMinus:
                return Visit(new AssignmentStatement(compoundStatement.variable,
                    new BinaryExpression(compoundStatement.variable, ETokenType.Minus, compoundStatement.expresion)));
            case ETokenType.CompoundMul:
                return Visit(new AssignmentStatement(compoundStatement.variable,
                    new BinaryExpression(compoundStatement.variable, ETokenType.Mul, compoundStatement.expresion)));
            case ETokenType.CompoundDiv:
                return Visit(new AssignmentStatement(compoundStatement.variable,
                    new BinaryExpression(compoundStatement.variable, ETokenType.Div, compoundStatement.expresion)));

        }

        return null;
    }

}