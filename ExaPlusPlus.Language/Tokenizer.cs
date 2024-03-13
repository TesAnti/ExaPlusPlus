using System.Text.RegularExpressions;

namespace ExaPlusPlus.Language;

public record Token(ETokenType Type,string Value,int Line, int Column);

public enum ETokenType
{
    Numeric,
    Plus,
    Minus,
    Inc,
    Dec,
    Mul,
    Div,
    CompoundPlus,
    CompoundMinus,
    CompoundMul,
    CompoundDiv,
    Mod,
    LParen,
    RParen,
    LBrace,
    RBrace,
    EOF,
    
    
    Assign,

    Equals,
    NotEquals,
    Greater,
    Less,
    Not,



    // in-game keywords
    Grab,
    Link,
    Drop,
    Delete,
    Spawn,
    Make,
    Write,
    Seek,
    Cut,
    Swiz,
    Kill,
    Die,
    Mode,
    Wait,
    

    If,
    Else,
    Forever,
    Break,
    Continue,
    While,
    
    Exa,
    Const,

    Identifier,
    

    Semicolon,
    Comma,

    Send
}

public class Tokenizer(string code)
{
    private int _position = 0;
    private string _code = code.Replace("\r","");

    public bool HasNextToken()
    {
        return _position < _code.Length;
    }

    private Dictionary<string, ETokenType?> _specs = new()
    {
        // comment
        {@"^\/\/[^\n]*",null},
        // multi-line comment
        {@"^\/\*.*\*\/",null},
        // whitespace
        {@"^\s+",null},

        
        {@"^;",ETokenType.Semicolon},
        {@"^,",ETokenType.Comma},
        {@"^\d+",ETokenType.Numeric},
       
        // comparison
        
        {@"^==",ETokenType.Equals},
        {@"^!=",ETokenType.NotEquals},
        {@"^!",ETokenType.Not},
        {@"^>",ETokenType.Greater},
        {@"^<",ETokenType.Less},

        // compound operators

        {@"^\+=",ETokenType.CompoundPlus},
        {@"^\-=",ETokenType.CompoundMinus},
        {@"^\*=",ETokenType.CompoundMul},
        {@"^\/=",ETokenType.CompoundDiv},



        // operators

        {@"^\+\+",ETokenType.Inc},
        {@"^\-\-",ETokenType.Dec},

        



        {@"^\+",ETokenType.Plus},
        {@"^\-",ETokenType.Minus},
        {@"^\*",ETokenType.Mul},
        {@"^\/",ETokenType.Div},
        {@"^%",ETokenType.Mod},
        {@"^=",ETokenType.Assign},
        
        

        
        

        // parentheses
        {@"^\(",ETokenType.LParen},
        {@"^\)",ETokenType.RParen},

        // braces
        {@"^\{",ETokenType.LBrace},
        {@"^\}",ETokenType.RBrace},

        // keywords
        {@"^grab\b",ETokenType.Grab},
        {@"^link\b",ETokenType.Link},
        {@"^drop\b",ETokenType.Drop},
        {@"^send\b",ETokenType.Send},
        {@"^delete\b",ETokenType.Delete},
        {@"^spawn\b",ETokenType.Spawn},
        {@"^make\b",ETokenType.Make},
        {@"^write\b",ETokenType.Write},
        {@"^if\b",ETokenType.If},
        {@"^else\b",ETokenType.Else},
        {@"^forever\b",ETokenType.Forever},
        {@"^break\b",ETokenType.Break},
        {@"^continue\b",ETokenType.Continue},
        {@"^while\b",ETokenType.While},
        {@"^seek\b",ETokenType.Seek},
        {@"^cut\b",ETokenType.Cut},
        {@"^swiz\b",ETokenType.Swiz},
        {@"^kill\b",ETokenType.Kill},
        {@"^die\b",ETokenType.Die},
        {@"^mode\b",ETokenType.Mode},
        {@"^wait\b",ETokenType.Wait},




        {@"^exa\b",ETokenType.Exa},
        {@"^const\b",ETokenType.Const},

        
        {@"^#?[a-zA-Z_][a-zA-Z0-9_]*",ETokenType.Identifier},
        

        




    };
    public Token NextToken()
    {
        if (_position >= _code.Length)
        {
            var lines = _code.Substring(0, _position).Split('\n');
            var line= lines.Length+1;
            var column = lines.Last().Length+1;
            return new Token(ETokenType.EOF, "",line,column);
        }

        var code = new string(_code.AsSpan(_position));

        foreach (var pair in _specs)
        {
            var matched = Regex.Match(code, pair.Key,RegexOptions.Singleline);
            if (matched.Success)
            {
                var value = matched.Value;
                _position += value.Length;
                if (pair.Value == null)
                {
                    return NextToken();
                }
                var lines = _code.Substring(0, _position- value.Length).Split('\n');
                var line = lines.Length + 1;
                var column = lines.Last().Length+1;
                return new Token(pair.Value.Value, value, line,column);
            }
        }

        throw new Exception($"Unexpected token: {code[0]}");
    }
}