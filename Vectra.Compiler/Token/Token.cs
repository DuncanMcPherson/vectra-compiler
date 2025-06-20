namespace Vectra.Compiler.Token;

internal sealed class Token
{
    public TokenType Type { get; set; }
    public string Lexeme { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    
    public Token(TokenType type, string lexeme, int line, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Line = line;
        Column = column;
    }

    public override string ToString() => $"{Type} '{Lexeme}' ({Line}:{Column})";
}