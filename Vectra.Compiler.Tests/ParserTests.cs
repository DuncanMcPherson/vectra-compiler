using NUnit.Framework;
using Vectra.AST;
using Vectra.AST.Declarations;
using Vectra.AST.Expressions;
using Vectra.AST.Models;
using Vectra.AST.Statements;
using Vectra.Compiler.Token;

namespace Vectra.Compiler.Tests;

[TestFixture]
public class ParserTests
{
    private List<Token.Token> CreateTokens(params Token.Token[] tokens)
    {
        var tokenList = tokens.ToList();
        // Add EOF token at the end if not already present
        if (tokenList.Count == 0 || tokenList[^1].Type != TokenType.EndOfFile)
        {
            tokenList.Add(new Token.Token(TokenType.EndOfFile, "", 1, 1));
        }
        return tokenList;
    }

    [Test]
    public void Parse_EmptySpaceDeclaration_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("main");
        result.RootSpace.Should().NotBeNull();
        result.RootSpace.Name.Should().Be("MySpace");
        result.RootSpace.Declarations.Should().BeEmpty();
    }

    [Test]
    public void Parse_NestedSpaceName_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "Parent", 1, 7),
            new Token.Token(TokenType.Symbol, ".", 1, 13),
            new Token.Token(TokenType.Identifier, "Child", 1, 14),
            new Token.Token(TokenType.Symbol, ";", 1, 19)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().NotBeNull();
        result.RootSpace.Name.Should().Be("Parent.Child");
    }

    [Test]
    public void Parse_SimpleClassDeclaration_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Symbol, "}", 3, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().NotBeNull();
        result.RootSpace.Declarations.Should().HaveCount(1);
        result.RootSpace.Declarations[0].Should().BeOfType<ClassDeclarationNode>();
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        classNode.Name.Should().Be("MyClass");
        classNode.Members.Should().BeEmpty();
    }

    [Test]
    public void Parse_ClassWithEmptyMethod_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "MyMethod", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 18),
            new Token.Token(TokenType.Symbol, ")", 3, 19),
            new Token.Token(TokenType.Symbol, "{", 3, 21),
            new Token.Token(TokenType.Symbol, "}", 4, 5),
            new Token.Token(TokenType.Symbol, "}", 5, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().NotBeNull();
        result.RootSpace.Declarations[0].Should().BeOfType<ClassDeclarationNode>();
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        classNode.Members.Should().HaveCount(1);
        classNode.Members[0].Should().BeOfType<MethodDeclarationNode>();
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        methodNode.Name.Should().Be("MyMethod");
        methodNode.ReturnType.Should().Be("void");
        methodNode.Parameters.Should().BeEmpty();
        methodNode.Body.Should().BeEmpty();
    }

    [Test]
    public void Parse_MethodWithParameters_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "int", 3, 5),
            new Token.Token(TokenType.Identifier, "Add", 3, 9),
            new Token.Token(TokenType.Symbol, "(", 3, 12),
            new Token.Token(TokenType.Identifier, "int", 3, 13),
            new Token.Token(TokenType.Identifier, "a", 3, 17),
            new Token.Token(TokenType.Symbol, ",", 3, 18),
            new Token.Token(TokenType.Identifier, "int", 3, 20),
            new Token.Token(TokenType.Identifier, "b", 3, 24),
            new Token.Token(TokenType.Symbol, ")", 3, 25),
            new Token.Token(TokenType.Symbol, "{", 3, 27),
            new Token.Token(TokenType.Symbol, "}", 4, 5),
            new Token.Token(TokenType.Symbol, "}", 5, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        methodNode.Name.Should().Be("Add");
        methodNode.ReturnType.Should().Be("int");
        methodNode.Parameters.Should().HaveCount(2);
        methodNode.Parameters[0].Name.Should().Be("a");
        methodNode.Parameters[0].Type.Should().Be("int");
        methodNode.Parameters[1].Name.Should().Be("b");
        methodNode.Parameters[1].Type.Should().Be("int");
    }

    [Test]
    public void Parse_MethodWithReturnStatement_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "int", 3, 5),
            new Token.Token(TokenType.Identifier, "GetValue", 3, 9),
            new Token.Token(TokenType.Symbol, "(", 3, 17),
            new Token.Token(TokenType.Symbol, ")", 3, 18),
            new Token.Token(TokenType.Symbol, "{", 3, 20),
            new Token.Token(TokenType.Keyword, "return", 4, 9),
            new Token.Token(TokenType.Number, "42", 4, 16),
            new Token.Token(TokenType.Symbol, ";", 4, 18),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        methodNode.Body.Should().HaveCount(1);
        methodNode.Body[0].Should().BeOfType<ReturnStatementNode>();
        var returnNode = (ReturnStatementNode)methodNode.Body[0];
        returnNode.Value.Should().BeOfType<LiteralExpressionNode>();
        var literalNode = (LiteralExpressionNode)returnNode.Value;
        literalNode.Value.Should().Be(42);
    }

    [Test]
    public void Parse_EmptyReturnStatement_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "DoSomething", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 21),
            new Token.Token(TokenType.Symbol, ")", 3, 22),
            new Token.Token(TokenType.Symbol, "{", 3, 24),
            new Token.Token(TokenType.Keyword, "return", 4, 9),
            new Token.Token(TokenType.Symbol, ";", 4, 15),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        methodNode.Body.Should().HaveCount(1);
        methodNode.Body[0].Should().BeOfType<ReturnStatementNode>();
        var returnNode = (ReturnStatementNode)methodNode.Body[0];
        returnNode.Value.Should().BeNull();
    }

    [Test]
    public void Parse_ExpressionStatement_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "Test", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 14),
            new Token.Token(TokenType.Symbol, ")", 3, 15),
            new Token.Token(TokenType.Symbol, "{", 3, 17),
            new Token.Token(TokenType.Identifier, "x", 4, 9),
            new Token.Token(TokenType.Symbol, ";", 4, 10),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        methodNode.Body.Should().HaveCount(1);
        methodNode.Body[0].Should().BeOfType<ExpressionStatementNode>();
        var exprStmtNode = (ExpressionStatementNode)methodNode.Body[0];
        exprStmtNode.Expression.Should().BeOfType<IdentifierExpressionNode>();
        var identifierNode = (IdentifierExpressionNode)exprStmtNode.Expression;
        identifierNode.Name.Should().Be("x");
    }

    [Test]
    public void Parse_BinaryExpression_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "int", 3, 5),
            new Token.Token(TokenType.Identifier, "Test", 3, 9),
            new Token.Token(TokenType.Symbol, "(", 3, 13),
            new Token.Token(TokenType.Symbol, ")", 3, 14),
            new Token.Token(TokenType.Symbol, "{", 3, 16),
            new Token.Token(TokenType.Keyword, "return", 4, 9),
            new Token.Token(TokenType.Number, "1", 4, 16),
            new Token.Token(TokenType.Operator, "+", 4, 18),
            new Token.Token(TokenType.Number, "2", 4, 20),
            new Token.Token(TokenType.Symbol, ";", 4, 21),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        var returnNode = (ReturnStatementNode)methodNode.Body[0];
        returnNode.Value.Should().BeOfType<BinaryExpressionNode>();
        var binaryNode = (BinaryExpressionNode)returnNode.Value;
        binaryNode.Operator.Should().Be("+");
        binaryNode.Left.Should().BeOfType<LiteralExpressionNode>();
        binaryNode.Right.Should().BeOfType<LiteralExpressionNode>();
        ((LiteralExpressionNode)binaryNode.Left).Value.Should().Be(1);
        ((LiteralExpressionNode)binaryNode.Right).Value.Should().Be(2);
    }

    [Test]
    public void Parse_MethodCallExpression_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "Test", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 14),
            new Token.Token(TokenType.Symbol, ")", 3, 15),
            new Token.Token(TokenType.Symbol, "{", 3, 17),
            new Token.Token(TokenType.Identifier, "obj", 4, 9),
            new Token.Token(TokenType.Symbol, ".", 4, 12),
            new Token.Token(TokenType.Identifier, "Method", 4, 13),
            new Token.Token(TokenType.Symbol, "(", 4, 19),
            new Token.Token(TokenType.Symbol, ")", 4, 20),
            new Token.Token(TokenType.Symbol, ";", 4, 21),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        var exprStmt = (ExpressionStatementNode)methodNode.Body[0];
        exprStmt.Expression.Should().BeOfType<CallExpressionNode>();
        var callNode = (CallExpressionNode)exprStmt.Expression;
        callNode.Target.Should().BeOfType<IdentifierExpressionNode>();
        callNode.MethodName.Should().Be("Method");
        callNode.Arguments.Should().BeEmpty();
    }

    [Test]
    public void Parse_ThisExpression_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "Test", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 14),
            new Token.Token(TokenType.Symbol, ")", 3, 15),
            new Token.Token(TokenType.Symbol, "{", 3, 17),
            new Token.Token(TokenType.Keyword, "this", 4, 9),
            new Token.Token(TokenType.Symbol, ".", 4, 13),
            new Token.Token(TokenType.Identifier, "Method", 4, 14),
            new Token.Token(TokenType.Symbol, "(", 4, 20),
            new Token.Token(TokenType.Symbol, ")", 4, 21),
            new Token.Token(TokenType.Symbol, ";", 4, 22),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        var exprStmt = (ExpressionStatementNode)methodNode.Body[0];
        exprStmt.Expression.Should().BeOfType<CallExpressionNode>();
        var callNode = (CallExpressionNode)exprStmt.Expression;
        callNode.Target.Should().BeOfType<IdentifierExpressionNode>();
        var target = (IdentifierExpressionNode)callNode.Target;
        target.Name.Should().Be("this");
    }

    [Test]
    public void Parse_MethodCallWithArguments_ReturnsCorrectModule()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Identifier, "Test", 3, 10),
            new Token.Token(TokenType.Symbol, "(", 3, 14),
            new Token.Token(TokenType.Symbol, ")", 3, 15),
            new Token.Token(TokenType.Symbol, "{", 3, 17),
            new Token.Token(TokenType.Identifier, "obj", 4, 9),
            new Token.Token(TokenType.Symbol, ".", 4, 12),
            new Token.Token(TokenType.Identifier, "Method", 4, 13),
            new Token.Token(TokenType.Symbol, "(", 4, 19),
            new Token.Token(TokenType.Number, "1", 4, 20),
            new Token.Token(TokenType.Symbol, ",", 4, 21),
            new Token.Token(TokenType.String, "test", 4, 23),
            new Token.Token(TokenType.Symbol, ")", 4, 29),
            new Token.Token(TokenType.Symbol, ";", 4, 30),
            new Token.Token(TokenType.Symbol, "}", 5, 5),
            new Token.Token(TokenType.Symbol, "}", 6, 1)
        );
        var parser = new Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        var classNode = (ClassDeclarationNode)result.RootSpace.Declarations[0];
        var methodNode = (MethodDeclarationNode)classNode.Members[0];
        var exprStmt = (ExpressionStatementNode)methodNode.Body[0];
        var callNode = (CallExpressionNode)exprStmt.Expression;
        callNode.Arguments.Should().HaveCount(2);
        callNode.Arguments[0].Should().BeOfType<LiteralExpressionNode>();
        callNode.Arguments[1].Should().BeOfType<LiteralExpressionNode>();
        ((LiteralExpressionNode)callNode.Arguments[0]).Value.Should().Be(1);
        ((LiteralExpressionNode)callNode.Arguments[1]).Value.Should().Be("test");
    }

    [Test]
    public void Parse_InvalidInput_ThrowsException()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Identifier, "invalid", 1, 1)
        );
        var parser = new Parser(tokens);

        // Act & Assert
        var action = () => parser.Parse();
        action.Should().Throw<Exception>().WithMessage("*Expected space declaration*");
    }

    [Test]
    public void Parse_MissingClassName_ThrowsException()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Symbol, "{", 2, 7)
        );
        var parser = new Parser(tokens);

        // Act & Assert
        var action = () => parser.Parse();
        action.Should().Throw<Exception>().WithMessage("*Expected class name*");
    }

    [Test]
    public void Parse_UnexpectedMethodName_ThrowsException()
    {
        // Arrange
        var tokens = CreateTokens(
            new Token.Token(TokenType.Keyword, "space", 1, 1),
            new Token.Token(TokenType.Identifier, "MySpace", 1, 7),
            new Token.Token(TokenType.Symbol, ";", 1, 15),
            new Token.Token(TokenType.Keyword, "class", 2, 1),
            new Token.Token(TokenType.Identifier, "MyClass", 2, 7),
            new Token.Token(TokenType.Symbol, "{", 2, 15),
            new Token.Token(TokenType.Identifier, "void", 3, 5),
            new Token.Token(TokenType.Symbol, "(", 3, 10)
        );
        var parser = new Parser(tokens);

        // Act & Assert
        var action = () => parser.Parse();
        action.Should().Throw<Exception>().WithMessage("*Expected identifier after type*");
    }
}