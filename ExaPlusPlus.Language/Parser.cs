using System.Xml.Linq;

namespace ExaPlusPlus.Language;

// literals
public record NumericLiteral(int value): ASTNode;
public record Variable(EVariableType VariableType): ASTNode;
public record EnvVariable(string name): Variable(EVariableType.Env);

// expressions
public record BinaryExpression(ASTNode left, ETokenType type, ASTNode right): ASTNode;
public record MinusExpression(ETokenType type, ASTNode expression): ASTNode;

public record NotExpression(ETokenType type, ASTNode expression) : ASTNode;

// statements
public record GrabStatement(ASTNode expression): ASTNode;
public record LinkStatement(List<ASTNode> data) : ASTNode;
public record DropStatement() : ASTNode;
public record SendStatement(List<ASTNode> data) : ASTNode;
public record DeleteStatement() : ASTNode;
public record SeekStatement(ASTNode amount) : ASTNode;
public record MakeStatement() : ASTNode;
public record WriteStatement(List<ASTNode> data) : ASTNode;
public record CutStatement():ASTNode;
public record SwizStatement(ASTNode expression,ASTNode order): ASTNode;
public record KillStatement(): ASTNode;
public record DieStatement(): ASTNode;
public record IncStatement(Variable expression) : ASTNode;
public record DecStatement(Variable expression) : ASTNode;
public record CompoundStatement(Variable variable,ETokenType token, ASTNode expresion) : ASTNode;
public record ConstantDeclarationStatement(string name, NumericLiteral expression) : ASTNode;

public record WaitStatement() : ASTNode;

public record EXADefinitionStatement(string name,StatementList list):ASTNode;
public record SpawnStatement(List<string> exas): ASTNode;
public record EmptyStatement(): ASTNode;
public record ExpressionStatement(ASTNode expression): ASTNode;
public record AssignmentStatement(Variable Variable, ASTNode value) : ASTNode;
public record IfStatement(ASTNode condition, ASTNode ifBody, ASTNode? elseBody) : ASTNode;
public record ForeverStatement(ASTNode statements) : ASTNode;
public record ContinueStatement() : ASTNode;
public record BreakStatement() : ASTNode;
public record WhileStatement(ASTNode condition, ASTNode body) : ASTNode;
public record ModeStatement(): ASTNode;

public record StatementList(List<ASTNode> statements) : ASTNode;
public record ProgramStatement(StatementList list) : ASTNode;


public class Parser
{
    private Tokenizer _tokenizer;
    private Token _lookahead;

    Dictionary<string, int> _constantsTable = new Dictionary<string, int>();
    public ASTNode Parse(string input)
    {
        _tokenizer = new Tokenizer(input);
        _lookahead = _tokenizer.NextToken();

        var res = StatementList();

        return new ProgramStatement(res);
    }

    #region Statements

    public EXADefinitionStatement EXADefinitionStatement()
    {
        Consume(ETokenType.Exa);
        var name=Consume(ETokenType.Identifier);
        Consume(ETokenType.LBrace);
        var list = StatementList(ETokenType.RBrace);
        Consume(ETokenType.RBrace);
        
        return new EXADefinitionStatement(name.Value,list);
    }

    public StatementList StatementList(ETokenType stopLookahead = ETokenType.EOF)
    {
        var list = new List<ASTNode>() { };
        while (_lookahead.Type != stopLookahead) list.Add(Statement());

        return new StatementList(list);
    }



    private ASTNode Statement()
    {
        if (_lookahead.Type == ETokenType.Semicolon)
        {
            Consume(ETokenType.Semicolon);
            return new EmptyStatement();
        }


        if (_lookahead.Type == ETokenType.Exa) return EXADefinitionStatement();

        if (_lookahead.Type == ETokenType.Send) return SendSatement();
        if (_lookahead.Type == ETokenType.Grab) return GrabStatement();
        if (_lookahead.Type == ETokenType.Link) return LinkStatement();
        if (_lookahead.Type == ETokenType.Drop) return DropStatement();
        if (_lookahead.Type == ETokenType.Delete) return DeleteStatement();
        if (_lookahead.Type == ETokenType.Spawn) return SpawnStatement();
        if (_lookahead.Type == ETokenType.Make) return MakeStatement();
        if (_lookahead.Type == ETokenType.Write) return WriteStatement();
        if (_lookahead.Type == ETokenType.If) return IfStatement();
        if (_lookahead.Type == ETokenType.Forever) return ForeverStatement();
        if (_lookahead.Type == ETokenType.Break) return BreakStatement();
        if (_lookahead.Type == ETokenType.Continue) return ContinueStatement();
        if (_lookahead.Type == ETokenType.While) return WhileStatement();
        if (_lookahead.Type == ETokenType.Seek) return SeekStatement();
        if (_lookahead.Type == ETokenType.Cut) return CutStatement();
        if (_lookahead.Type == ETokenType.Swiz) return SwizStatement();
        if (_lookahead.Type == ETokenType.Kill) return KillStatement();
        if (_lookahead.Type == ETokenType.Die) return DieStatement();
        if (_lookahead.Type == ETokenType.Mode) return ModeStatement();
        if (_lookahead.Type == ETokenType.Wait) return WaitStatement();
        if (_lookahead.Type == ETokenType.Const) return ConstantStatement();

        


        return ExpressionStatement();

    }

    private ASTNode ConstantStatement()
    {
        Consume(ETokenType.Const);
        var variable = Consume(ETokenType.Identifier).Value;
        Consume(ETokenType.Assign);
        var expression = NumericLiteralNeg();
        Consume(ETokenType.Semicolon);
        _constantsTable.Add(variable, expression.value);
        return new EmptyStatement();
    }

    public ASTNode WaitStatement()
    {
        Consume(ETokenType.Wait);
        Consume(ETokenType.Semicolon);
        return new WaitStatement();
    }

    public ASTNode ModeStatement()
    {
        Consume(ETokenType.Mode);
        Consume(ETokenType.Semicolon);
        return new ModeStatement();
    }

    public ASTNode DieStatement()
    {
        Consume(ETokenType.Die);
        Consume(ETokenType.Semicolon);
        return new DieStatement();
    }



    public ASTNode KillStatement()
    {
        Consume(ETokenType.Kill);
        Consume(ETokenType.Semicolon);
        return new KillStatement();
    }

    public ASTNode SwizStatement()
    {
        Consume(ETokenType.Swiz);
        var expression = CompareExpression();
        var order = CompareExpression();
        Consume(ETokenType.Semicolon);
        return new SwizStatement(expression,order);
    }
    public ASTNode CutStatement()
    {
        Consume(ETokenType.Cut);
        Consume(ETokenType.Semicolon);
        return new CutStatement();
    }

    public ASTNode SeekStatement()
    {
        Consume(ETokenType.Seek);
        var amount = CompareExpression();
        Consume(ETokenType.Semicolon);
        return new SeekStatement(amount);
    }

    public ASTNode WhileStatement()
    {
        Consume(ETokenType.While);
        Consume(ETokenType.LParen);
        var condition = CompareExpression();
        Consume(ETokenType.RParen);
        ASTNode body;
        if (_lookahead.Type == ETokenType.LBrace)
        {
            Consume(ETokenType.LBrace);
            body = StatementList(ETokenType.RBrace);
            Consume(ETokenType.RBrace);
        }
        else
        {
            body = Statement();
        }
        return new WhileStatement(condition, body);
    }

    public ASTNode BreakStatement()
    {
        Consume(ETokenType.Break);
        Consume(ETokenType.Semicolon);
        return new BreakStatement();
    }

    public ASTNode ContinueStatement()
    {
                Consume(ETokenType.Continue);
                Consume(ETokenType.Semicolon);
        return new ContinueStatement();
    
    }

    public ASTNode ForeverStatement()
    {
        Consume(ETokenType.Forever);
        if (_lookahead.Type == ETokenType.LBrace)
        {
            Consume(ETokenType.LBrace);
            var body = StatementList(ETokenType.RBrace);
            Consume(ETokenType.RBrace);
            return new ForeverStatement(body);
        }
        else
        {
            var body = Statement();
            return new ForeverStatement(body);
        }

    }

    public ASTNode IfStatement()
    {
        Consume(ETokenType.If);
        Consume(ETokenType.LParen);
        var condition = CompareExpression();
        Consume(ETokenType.RParen);
        ASTNode ifBody;
        if (_lookahead.Type==ETokenType.LBrace)
        {
            Consume(ETokenType.LBrace);
            ifBody = StatementList(ETokenType.RBrace);
            Consume(ETokenType.RBrace);
        }
        else
        {
            ifBody = Statement();
        }
        
        if (_lookahead.Type==ETokenType.Else)
        {
            Consume(ETokenType.Else);
            if (_lookahead.Type == ETokenType.LBrace)
            {
                Consume(ETokenType.LBrace);
                var elseBody = StatementList(ETokenType.RBrace);
                Consume(ETokenType.RBrace);
                return new IfStatement(condition, ifBody, elseBody);
            }
            else
            {
                var elseBody = Statement();
                return new IfStatement(condition, ifBody, elseBody);
            }
        }
        else
        {
            return new IfStatement(condition, ifBody, null);
        }
        
    }



    public ASTNode WriteStatement()
    {
        Consume(ETokenType.Write);
        List<ASTNode> parameters = new List<ASTNode>();
        parameters.Add(CompareExpression());
        while (_lookahead.Type == ETokenType.Comma)
        {
            Consume(ETokenType.Comma);
            parameters.Add(CompareExpression());
        }
        return new WriteStatement(parameters);
    }

    public ASTNode MakeStatement()
    {
        Consume(ETokenType.Make);
        Consume(ETokenType.Semicolon);
        return new MakeStatement();
    }

    public ASTNode SpawnStatement()
    {
        Consume(ETokenType.Spawn);
        var exa = Consume(ETokenType.Identifier);
        List<string> result = new List<string>(){exa.Value};
        while (_lookahead.Type == ETokenType.Comma)
        {
            Consume(ETokenType.Comma);
            exa = Consume(ETokenType.Identifier);
            result.Add(exa.Value);
        }
        
        
        Consume(ETokenType.Semicolon);


        return new SpawnStatement(result);
    }

    public ASTNode DeleteStatement()
    {
        Consume(ETokenType.Delete);
        Consume(ETokenType.Semicolon);
        return new DeleteStatement();

    }

    public ASTNode SendSatement()
    {
        Consume(ETokenType.Send);
        List<ASTNode> parameters = new List<ASTNode>();
        parameters.Add(CompareExpression());
        while (_lookahead.Type == ETokenType.Comma)
        {
            Consume(ETokenType.Comma);
            parameters.Add(CompareExpression());
        }
        Consume(ETokenType.Semicolon);
        return new SendStatement(parameters);

    }

    private ASTNode DropStatement()
    {
        Consume(ETokenType.Drop);
        Consume(ETokenType.Semicolon);
        return new DropStatement();
    }

    private ASTNode LinkStatement()
    {
        Consume(ETokenType.Link);
        List<ASTNode> parameters = new List<ASTNode>();
        parameters.Add(CompareExpression());
        while (_lookahead.Type == ETokenType.Comma)
        {
            Consume(ETokenType.Comma);
            parameters.Add(CompareExpression());
        }
        Consume(ETokenType.Semicolon);
        return new LinkStatement(parameters);
    }

    public ASTNode GrabStatement()
    {
        Consume(ETokenType.Grab);
        var expression = CompareExpression();
        Consume(ETokenType.Semicolon);
        return new GrabStatement(expression);
    }

    public ExpressionStatement ExpressionStatement()
    {
        var expression = AssignmentStatement();
        
        return new ExpressionStatement(expression);
    }

    public ASTNode AssignmentStatement()
    {
        if (_lookahead.Type == ETokenType.Identifier)
        {
            var variable = Variable();
            if (_lookahead.Type == ETokenType.Inc)
            {
                return IncStatement(variable);
            }

            if (_lookahead.Type == ETokenType.Dec)
            {
                return DecStatement(variable);
            }

            if (IsCompoundAssignmentOperator(_lookahead.Type))
            {
                return CompoundAssignmentStatement(variable);
            }

            Consume(ETokenType.Assign);
            var value = CompareExpression();
            return new AssignmentStatement(variable, value);
        }
        Consume(ETokenType.Semicolon);
        return CompareExpression();

    }

    private ASTNode CompoundAssignmentStatement(Variable variable)
    {
        var op = Consume(_lookahead.Type);
        var value = CompareExpression();
        Consume(ETokenType.Semicolon);
        return new CompoundStatement(variable, op.Type, value);
    }

    private bool IsCompoundAssignmentOperator(ETokenType lookaheadType)
    {
        return lookaheadType == ETokenType.CompoundPlus || lookaheadType == ETokenType.CompoundMinus ||
               lookaheadType == ETokenType.CompoundMul || lookaheadType == ETokenType.CompoundDiv;
    }

    public ASTNode IncStatement(Variable variable)
    {
        Consume(ETokenType.Inc);
        Consume(ETokenType.Semicolon);
        return new IncStatement(variable);
    }

    public ASTNode DecStatement(Variable variable)
    {
        
        Consume(ETokenType.Dec);
        Consume(ETokenType.Semicolon);
        return new DecStatement(variable);
    }

    #endregion



    #region Expressions


    public ASTNode CompareExpression()
    {
        var left = ModExpression();
        // only one comparison per expression
        if (_lookahead.Type == ETokenType.Less || 
               _lookahead.Type == ETokenType.Greater|| _lookahead.Type==ETokenType.Equals|| _lookahead.Type == ETokenType.NotEquals )
        {
            var type = _lookahead.Type;
            Consume(type);
            var right = ModExpression();
            left = new BinaryExpression(left, type, right);
        }
        return left;
    }

    public ASTNode ModExpression()
    {
        var left = AdditiveExpression();
        while (_lookahead.Type == ETokenType.Mod)
        {
            var type = _lookahead.Type;
            Consume(type);
            var right = AdditiveExpression();
            left = new BinaryExpression(left, type, right);
        }
        return left;
    }

    public ASTNode AdditiveExpression()
    {
        var left = MultiplicativeExpression();
        while (_lookahead.Type==ETokenType.Plus||_lookahead.Type==ETokenType.Minus)
        {
            var type = _lookahead.Type;
            Consume(type);
            var right = MultiplicativeExpression();
            left = new BinaryExpression(left, type, right);
        }

        return left;
    }
    
    public ASTNode MultiplicativeExpression()
    {
        var left = UnaryExpression();
        while (_lookahead.Type == ETokenType.Mul || _lookahead.Type == ETokenType.Div)
        {
            var type = _lookahead.Type;
            Consume(type);
            var right = UnaryExpression();
            left = new BinaryExpression(left, type, right);
        }
        return left;
    }

    public ASTNode UnaryExpression()
    {
        var type = _lookahead.Type;
        if (type == ETokenType.Minus)
        {
            Consume(type);
            var expression = PrimaryExpression();
            return new MinusExpression(type, expression);
        }

        if (type == ETokenType.Not)
        {
            Consume(type);
            var expression = PrimaryExpression();
            return new NotExpression(type, expression);
        
        }
     
        return PrimaryExpression();
    }

    public ASTNode PrimaryExpression()
    {
        if (IsLiteral()) return Literal();
        if (_lookahead.Type == ETokenType.Identifier)
        {
            if (_constantsTable.ContainsKey(_lookahead.Value))
            {
                return ConstantReference();
            }
                
            return Variable();
        }
        if (_lookahead.Type == ETokenType.LParen) return ParenthesizedExpression();
        throw new Exception($"Unexpected token '{_lookahead.Value}' at position {_lookahead.Line}:{_lookahead.Column}");
    }

    private ASTNode ConstantReference()
    {
        var name = Consume(ETokenType.Identifier);
        return new NumericLiteral(_constantsTable[name.Value]);
    }


    public ASTNode ParenthesizedExpression()
    {
        Consume(ETokenType.LParen);
        var expression = CompareExpression();
        Consume(ETokenType.RParen);
        return expression;
    }

    #endregion
    
    #region Literals

    public ASTNode Literal()
    {
        switch (_lookahead.Type)
        {
            case ETokenType.Numeric:
                return this.NumericLiteral();

            case ETokenType.Identifier:
                return this.Variable();


            default:
                throw new ArgumentOutOfRangeException();
        }
       
    }


    public Variable Variable()
    {
        var name = Consume(ETokenType.Identifier).Value;


        if (name.StartsWith("#"))
        {
            return new EnvVariable(name);
        }
        EVariableType type;
        switch (name)
        {
            case "x":
                type=EVariableType.X;
                break;
            case "read":
                type=EVariableType.F;
                break;
            case "recv":
                type=EVariableType.M;
                break;
            case "host":
                type=EVariableType.Host;
                break;
            case "eof":
                type = EVariableType.EOF;
                break;
            default:
                throw new NotSupportedException();
        }
        return new Variable(type);
    }



    

    private bool IsLiteral()
    {
        return _lookahead.Type == ETokenType.Numeric;
    }

    private NumericLiteral NumericLiteral()=> new NumericLiteral(int.Parse(Consume(ETokenType.Numeric).Value));

    /// <summary>
    /// Parses a numeric literal and returns it as a negative integer if preceded by a minus sign.
    /// </summary>
    public NumericLiteral NumericLiteralNeg()
    {
        if (_lookahead.Type == ETokenType.Minus)
        {
            Consume(ETokenType.Minus);
            return new NumericLiteral(-int.Parse(Consume(ETokenType.Numeric).Value));
        }
        return new NumericLiteral(int.Parse(Consume(ETokenType.Numeric).Value));
    }

    #endregion


    public Token Consume(ETokenType tokenType)
    {
        var token = _lookahead;

        if (token.Type != tokenType) throw new Exception($"Unexpected token: {token.Type}");

        _lookahead = _tokenizer.NextToken();
        return token;
    }
}