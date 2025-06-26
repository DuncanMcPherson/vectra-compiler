using System.Text;
using Vectra.Compiler.Token;

namespace Vectra.Compiler.Lexer;

internal sealed class Lexer
{
    private readonly string _sourceCode;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    private static readonly HashSet<string> Keywords =
    [
        "space",
        "class",
        "void",
        "return",
        "let",
        "this",
        "number",
        "string",
        "bool",
        "get",
        "set",
        "new"
    ];

    private static readonly HashSet<string> MultiCharOperators =
    [
        "==", "!=", ">=", "<="
    ];

    private static readonly HashSet<string> SingleCharOperators =
    [
        "+", "-", "*", "/", "=", "<", ">", "!" // Eventually will need to add support for %
    ];

    // private static readonly HashSet<string> Symbols = ["(", ")", "{", "}", "[", "]", ";", ",", "."];

    public Lexer(string sourceCode)
    {
        _sourceCode = sourceCode;
        _position = 0;
    }

    public List<Token.Token> Tokenize()
    {
        var tokens = new List<Token.Token>();

        while (!IsAtEnd())
        {
            var nextToken = NextToken();
            if (nextToken != null)
                tokens.Add(nextToken);
        }

        tokens.Add(new Token.Token(TokenType.EndOfFile, "", _line, _column));
        return tokens;
    }

    private Token.Token? NextToken()
    {
        SkipWhiteSpaceAndComments();

        if (IsAtEnd())
            return null;
        var start = _position;
        var line = _line;
        var column = _column;

        var c = Advance();

        if (char.IsLetter(c) || c == '_')
            return LexIdentifierOrKeyword(start, line, column);
        if (char.IsDigit(c))
            return LexNumber(start, line, column);
        return c == '"' ? LexString(start, line, column) : LexSymbolOrOperator(c, start, line, column);
    }

    private void SkipWhiteSpaceAndComments()
    {
        while (!IsAtEnd())
        {
            var c = Peek();
            if (char.IsWhiteSpace(c))
            {
                Advance();
            }
            else if (c == '/' && PeekNext() == '/')
            {
                while (Peek() != '\n' && !IsAtEnd())
                    Advance();
            }
            else
            {
                break;
            }
        }
    }

    private Token.Token LexIdentifierOrKeyword(int start, int line, int column)
    {
        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
            Advance();

        var lexeme = _sourceCode[start.._position];
        var type = IsKeyword(lexeme) ? TokenType.Keyword : TokenType.Identifier;
        return new Token.Token(type, lexeme, line, column);
    }

    private Token.Token LexNumber(int start, int line, int column)
    {
        while (!IsAtEnd() && char.IsDigit(Peek()))
        {
            Advance();
        }

        // TODO: add support for decimal numbers
        var lexeme = _sourceCode[start.._position];
        return new Token.Token(TokenType.Number, lexeme, line, column);
    }

    private Token.Token LexString(int _, int line, int column)
    {
        var builder = new StringBuilder();

        while (!IsAtEnd() && Peek() != '"')
        {
            var c = Advance();
            if (c == '\\' && !IsAtEnd())
            {
                var next = Advance();
                builder.Append(next switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '"' => '"',
                    '\\' => '\\',
                    _ => next
                });
            }
            else
            {
                builder.Append(c);
            }
        }

        if (IsAtEnd())
            throw new Exception($"Unterminated string at line {line}, column {column}.");
        Advance();

        return new Token.Token(TokenType.String, builder.ToString(), line, column);
    }

    private Token.Token LexSymbolOrOperator(char c, int _, int line, int column)
    {
        var lexeme = c.ToString();
        var next = Peek();
        var twoChar = lexeme + next;
        if (MultiCharOperators.Contains(twoChar))
        {
            Advance();
            lexeme = twoChar;
        }

        var type = IsOperator(lexeme) ? TokenType.Operator : TokenType.Symbol;
        return new Token.Token(type, lexeme, line, column);
    }

    #region Utilities

    private bool IsAtEnd() => _position >= _sourceCode.Length;
    private char Peek() => IsAtEnd() ? '\0' : _sourceCode[_position];
    private char PeekNext() => (_position + 1 >= _sourceCode.Length) ? '\0' : _sourceCode[_position + 1];

    private char Advance()
    {
        var current = _sourceCode[_position++];
        if (current == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return current;
    }

    private static bool IsKeyword(string lexeme) => Keywords.Contains(lexeme);

    private static bool IsOperator(string lexeme) =>
        SingleCharOperators.Contains(lexeme) || MultiCharOperators.Contains(lexeme);

    #endregion
}