namespace Vectra.Compiler.Token;

internal enum TokenType
{
    Identifier,
    Number,
    String,
    Keyword,
    Symbol,
    Operator,
    Whitespace,
    Comment,
    EndOfFile
}