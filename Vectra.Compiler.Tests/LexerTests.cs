using Vectra.Compiler.Token;

namespace Vectra.Compiler.Tests;

[TestFixture]
public class LexerTests
{
    private Lexer.Lexer _lexer;

    [SetUp]
    public void Setup()
    {
        _lexer = new Lexer.Lexer("let x = 5;");
    }

    [Test]
    public void Tokenize_EmptyInput_ReturnsOnlyEOF()
    {
        // Arrange
        var lexer = new Lexer.Lexer("");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Should().HaveCount(1);
        tokens[0].Type.Should().Be(TokenType.EndOfFile);
    }

    [Test]
    public void Tokenize_SimpleAssignment_ReturnsCorrectTokens()
    {
        // Arrange is done in Setup

        // Act
        var tokens = _lexer.Tokenize();

        // Assert
        tokens.Should().HaveCount(6); // 'let', 'x', '=', '5', ';', plus EOF
        tokens[0].Type.Should().Be(TokenType.Keyword);
        tokens[0].Lexeme.Should().Be("let");
        tokens[1].Type.Should().Be(TokenType.Identifier);
        tokens[1].Lexeme.Should().Be("x");
        tokens[2].Type.Should().Be(TokenType.Operator);
        tokens[2].Lexeme.Should().Be("=");
        tokens[3].Type.Should().Be(TokenType.Number);
        tokens[3].Lexeme.Should().Be("5");
        tokens[4].Type.Should().Be(TokenType.Symbol);
        tokens[4].Lexeme.Should().Be(";");
        tokens[5].Type.Should().Be(TokenType.EndOfFile);
    }

    [Test]
    public void Tokenize_MultipleKeywords_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("space class void return let this");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.Keyword).Should().Be(6);
        tokens[0].Lexeme.Should().Be("space");
        tokens[1].Lexeme.Should().Be("class");
        tokens[2].Lexeme.Should().Be("void");
        tokens[3].Lexeme.Should().Be("return");
        tokens[4].Lexeme.Should().Be("let");
        tokens[5].Lexeme.Should().Be("this");
    }

    [Test]
    public void Tokenize_Identifiers_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("foo bar_baz _x y123");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.Identifier).Should().Be(4);
        tokens[0].Lexeme.Should().Be("foo");
        tokens[1].Lexeme.Should().Be("bar_baz");
        tokens[2].Lexeme.Should().Be("_x");
        tokens[3].Lexeme.Should().Be("y123");
    }

    [Test]
    public void Tokenize_Numbers_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("123 0 42");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.Number).Should().Be(3);
        tokens[0].Lexeme.Should().Be("123");
        tokens[1].Lexeme.Should().Be("0");
        tokens[2].Lexeme.Should().Be("42");
    }

    [Test]
    public void Tokenize_Strings_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("\"hello\" \"world\"");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.String).Should().Be(2);
        tokens[0].Lexeme.Should().Be("hello");
        tokens[1].Lexeme.Should().Be("world");
    }

    [Test]
    public void Tokenize_EscapedStrings_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("\"hello\\nworld\" \"escaped\\\"quote\"");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.String).Should().Be(2);
        tokens[0].Lexeme.Should().Be("hello\nworld");
        tokens[1].Lexeme.Should().Be("escaped\"quote");
    }

    [Test]
    public void Tokenize_Operators_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("+ - * / = < > ! == != >= <=");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.Operator).Should().Be(12);
        tokens[0].Lexeme.Should().Be("+");
        tokens[1].Lexeme.Should().Be("-");
        tokens[2].Lexeme.Should().Be("*");
        tokens[3].Lexeme.Should().Be("/");
        tokens[4].Lexeme.Should().Be("=");
        tokens[5].Lexeme.Should().Be("<");
        tokens[6].Lexeme.Should().Be(">");
        tokens[7].Lexeme.Should().Be("!");
        tokens[8].Lexeme.Should().Be("==");
        tokens[9].Lexeme.Should().Be("!=");
        tokens[10].Lexeme.Should().Be(">=");
        tokens[11].Lexeme.Should().Be("<=");
    }

    [Test]
    public void Tokenize_Symbols_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("( ) { } [ ] ; , .");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Count(t => t.Type == TokenType.Symbol).Should().Be(9);
        tokens[0].Lexeme.Should().Be("(");
        tokens[1].Lexeme.Should().Be(")");
        tokens[2].Lexeme.Should().Be("{");
        tokens[3].Lexeme.Should().Be("}");
        tokens[4].Lexeme.Should().Be("[");
        tokens[5].Lexeme.Should().Be("]");
        tokens[6].Lexeme.Should().Be(";");
        tokens[7].Lexeme.Should().Be(",");
        tokens[8].Lexeme.Should().Be(".");
    }

    [Test]
    public void Tokenize_CommentsAreSkipped()
    {
        // Arrange
        var lexer = new Lexer.Lexer("let x = 5; // This is a comment\nx = 10;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Should().HaveCount(10); // 5 for first statement, 3 for second, plus EOF
        tokens.Any(t => t.Lexeme.Contains("comment")).Should().BeFalse();
    }

    [Test]
    public void Tokenize_LineAndColumnAreTrackedCorrectly()
    {
        // Arrange
        var lexer = new Lexer.Lexer("let x = 5;\nlet y = 10;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens[0].Line.Should().Be(1); // let
        tokens[0].Column.Should().Be(1);
        tokens[5].Line.Should().Be(2); // let on second line
        tokens[5].Column.Should().Be(1);
    }

    [Test]
    public void Tokenize_UnterminatedString_ThrowsException()
    {
        // Arrange
        var lexer = new Lexer.Lexer("\"unterminated");

        // Act & Assert
        var action = () => lexer.Tokenize();
        action.Should().Throw<Exception>().WithMessage("*Unterminated string*");
    }

    [Test]
    public void Tokenize_ComplexExpression_ReturnsCorrectTokens()
    {
        // Arrange
        var lexer = new Lexer.Lexer("let result = (a + b) * (c - d) / 2;");

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        tokens.Should().HaveCount(18); // 17 tokens plus EOF

        // Verify selected tokens
        tokens[0].Type.Should().Be(TokenType.Keyword);
        tokens[0].Lexeme.Should().Be("let");

        tokens[3].Type.Should().Be(TokenType.Symbol);
        tokens[3].Lexeme.Should().Be("(");

        tokens[5].Type.Should().Be(TokenType.Operator);
        tokens[5].Lexeme.Should().Be("+");

        tokens[9].Type.Should().Be(TokenType.Symbol);
        tokens[9].Lexeme.Should().Be("(");

        tokens[14].Type.Should().Be(TokenType.Operator);
        tokens[14].Lexeme.Should().Be("/");

        tokens[15].Type.Should().Be(TokenType.Number);
        tokens[15].Lexeme.Should().Be("2");

        tokens[16].Type.Should().Be(TokenType.Symbol);
        tokens[16].Lexeme.Should().Be(";");
    }
}