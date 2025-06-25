using System.Text;
using Vectra.AST;
using Vectra.AST.Declarations;
using Vectra.AST.Declarations.Interfaces;
using Vectra.AST.Expressions;
using Vectra.AST.Models;
using Vectra.AST.Statements;
using Vectra.Compiler.Token;

namespace Vectra.Compiler;

internal sealed class Parser
{
    private readonly List<Token.Token> _tokens;
    private int _position;

    public Parser(List<Token.Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    /// Parses the provided tokens into an abstract syntax tree (AST) representation
    /// of a Vectra module.
    /// The method assumes that the input tokens are valid and performs a series of
    /// parsing steps to construct a `VectraASTModule` object representing the module
    /// structure, including space declarations and type declarations.
    /// Throws exceptions if the tokens are not in a valid format or if parsing fails.
    /// <returns>
    /// A `VectraASTModule` instance representing the parsed structure of the input tokens.
    /// </returns>
    public VectraASTModule Parse()
    {
        // TODO: Parse enter statements (once supported)
        var parsedSpace = ParseSpaceDeclaration();

        return new VectraASTModule("main", parsedSpace);
    }

    /// Parses a space declaration from the current position in the token list.
    /// The method processes tokens to build a `SpaceDeclarationNode` that represents
    /// a named space along with its type declarations. It checks for syntactical
    /// correctness, including the presence of mandatory keywords and correct token types.
    /// Stops parsing when all type declarations within the space have been processed or
    /// when the token list ends.
    /// Throws exceptions if the tokens do not conform to the expected syntax for
    /// a space declaration.
    /// <param name="parent">The parent `SpaceDeclarationNode` to associate with
    /// the parsed space, if applicable. Defaults to null.</param>
    /// <returns>
    /// A `SpaceDeclarationNode` instance encapsulating the parsed name, types, and
    /// other structural details of the space declaration.
    /// </returns>
    private SpaceDeclarationNode ParseSpaceDeclaration(SpaceDeclarationNode? parent = null)
    {
        Expect("space", "Expected space declaration");

        var nameToken = Advance();
        if (nameToken.Type != TokenType.Identifier)
            throw new Exception($"Expected space name at line {nameToken.Line}");
        var nameBuilder = new StringBuilder();
        nameBuilder.Append(nameToken.Lexeme);
        while (!IsAtEnd() && Match("."))
        {
            var identifierToken = Consume(TokenType.Identifier, "Expected identifier after '.'");
            nameBuilder.Append('.');
            nameBuilder.Append(identifierToken.Lexeme);
        }

        var name = nameBuilder.ToString();
        Expect(";", "Expected ';' after space name");

        var types = new List<ITypeDeclarationNode>();
        while (!IsAtEnd())
        {
            var type = ParseTypeDeclaration();
            types.Add(type);
        }

        return new SpaceDeclarationNode(name, types, new(), parent);
    }

    /// Parses a type declaration node from the input tokens.
    /// The method identifies the type declaration keyword (e.g., "class") and delegates
    /// the parsing logic to the corresponding parsing method based on the keyword.
    /// Throws exceptions if the token is invalid or if the type declaration is not recognized.
    /// <returns>
    /// An `ITypeDeclarationNode` instance representing the parsed type declaration.
    /// </returns>
    private ITypeDeclarationNode ParseTypeDeclaration()
    {
        var typeToken = Consume(TokenType.Keyword, "Expected a keyword");
        return typeToken.Lexeme switch
        {
            "class" => ParseClassDeclaration(),
            _ => throw new Exception(
                $"Unexpected token '{typeToken.Lexeme}' at line {typeToken.Line}, column {typeToken.Column}.")
        };
    }

    /// Parses a class declaration from the tokens, identifying the class name
    /// and extracting its member declarations. The method assumes that the
    /// input tokens provide a valid class definition and includes handling
    /// of opening and closing braces and member definitions.
    /// Throws an exception if the tokens are not in the expected format
    /// or if mandatory components like the class name or braces are missing.
    /// <returns>
    /// A `ClassDeclarationNode` instance representing the parsed class,
    /// including its name and members.
    /// </returns>
    private ClassDeclarationNode ParseClassDeclaration()
    {
        var nameToken = Consume(TokenType.Identifier, "Expected class name");
        // TODO: Inheritance, Implements, etc.
        Expect("{", "Expected '{' after class name");
        var members = new List<IMemberNode>();
        while (!IsAtEnd() && !Match("}"))
        {
            var member = ParseMemberDeclaration();
            members.Add(member);
        }

        return new ClassDeclarationNode(nameToken.Lexeme, members,
            new(nameToken.Line, nameToken.Column, Previous().Line, Previous().Column));
    }

    /// Parses a member declaration from the current position in the token stream
    /// and constructs an abstract syntax tree (AST) representation of the member.
    /// This method identifies and processes signatures for methods, and throws exceptions
    /// if the tokens do not represent a valid member declaration or if the syntax is unsupported.
    /// Currently supports parsing of method declarations only; parsing of fields, properties,
    /// or constructors is not implemented yet.
    /// Throws exceptions if the token at the current position is invalid or unrecognized.
    /// <returns>
    /// An `IMemberNode` representing the parsed member declaration, such as a method declaration.
    /// </returns>
    private IMemberNode ParseMemberDeclaration()
    {
        // TODO: Introduce support for constructors
        
        // Validate that the current token is a valid member declaration (Identifier or Keyword)
        if (!Check(TokenType.Identifier, TokenType.Keyword))
            throw new Exception($"Expected identifier or keyword at {Peek().Line}");
        // get the type of the member (Identifier or Keyword)
        var typeToken = Advance();
        // Validate and retrieve the name of the member;
        var nameToken = Consume(TokenType.Identifier, "Expected identifier after type");
        // Check if the member is a method declaration
        if (Match("("))
            return ParseMethodDeclaration(typeToken, nameToken);
        // Check if the member is a field declaration
        if (Match("=") || Match(";"))
            return ParseFieldDeclaration(typeToken, nameToken);
        // Check if the member is a property declaration
        if (Match("{"))
            return ParsePropertyDeclaration(typeToken, nameToken);
        throw new Exception($"Unknown member declaration at line {Peek().Line}, column {Peek().Column}.");
    }

    private MethodDeclarationNode ParseMethodDeclaration(Token.Token returnType, Token.Token name)
    {
        // We have already skipped the opening '('
        var parameters = new List<Parameter>();
        if (!Match(")"))
        {
            do
            {
                var parameterType = Advance();
                var parameterName = Advance();
                parameters.Add(new Parameter(parameterName.Lexeme, parameterType.Lexeme));
            } while (Match(","));

            Expect(")", "Expected ')' following parameter declarations");
        }

        Expect("{", "Expected '{' to start method body");

        // TODO: Switch MethodDeclarationNode.Body from List<IStatementNode> to BlockStatement
        var statements = new List<IStatementNode>();
        while (!IsAtEnd() && !Match("}"))
        {
            var statement = ParseStatement();
            statements.Add(statement);
        }

        return new MethodDeclarationNode(name.Lexeme, parameters, statements,
            new(returnType.Line, returnType.Column, Previous().Line, Previous().Column), returnType.Lexeme);
    }

    private FieldDeclarationNode ParseFieldDeclaration(Token.Token type, Token.Token name)
    {
        // Field is left uninitialized
        if (Match(";"))
            return new FieldDeclarationNode(name.Lexeme, type.Lexeme, null,
                new(type.Line, type.Column, Previous().Line, Previous().Column));
        // Field is initialized (need to skip the '=' sign)
        Expect("=", "Expected '=' after field declaration");
        var initializer = ParseExpression();
        Expect(";", "Expected ';' after field initializer");
        return new FieldDeclarationNode(name.Lexeme, type.Lexeme, initializer,
            new(type.Line, type.Column, Previous().Line, Previous().Column));
    }

    private PropertyDeclarationNode ParsePropertyDeclaration(Token.Token type, Token.Token name)
    {
        // Default getter and setter to false
        var hasGetter = false;
        var hasSetter = false;
        
        // we have already skipped the opening '{'

        while (!IsAtEnd() && !Match("}"))
        {
            var accessorType = Consume(TokenType.Identifier, "Expected 'get' or 'set' after property declaration");
            switch (accessorType.Lexeme)
            {
                case "get":
                    if (hasGetter)
                        throw new Exception("Property cannot have more than one getter.");
                    hasGetter = true;
                    break;
                case "set":
                    if (hasSetter)
                        throw new Exception("Property cannot have more than one setter.");
                    hasSetter = true;
                    break;
                default:
                    throw new Exception($"Unexpected token '{accessorType.Lexeme}' at line {accessorType.Line}, column {accessorType.Column}.");
            }
            Expect(";", "Expected ';' after property accessor");;
        }
        
        return new PropertyDeclarationNode(name.Lexeme, type.Lexeme, hasGetter, hasSetter, new(type.Line, type.Column, Previous().Line, Previous().Column));
    }

    private IStatementNode ParseStatement()
    {
        if (Match("return"))
            return ParseReturnStatement();

        if (Match("let"))
            return ParseVariableDeclaration(false);

        if (Check(TokenType.Identifier) && PeekNext().Type == TokenType.Identifier) // TODO: refine support for type keywords
            return ParseVariableDeclaration(true);

        return ParseExpressionStatement();
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        var startLocation = Previous();
        if (Match(";"))
        {
            return new ReturnStatementNode(null,
                new(startLocation.Line, startLocation.Column, Previous().Line, Previous().Column));
        }

        var value = ParseExpression();
        Expect(";", "Expected ';' after return value");
        return new ReturnStatementNode(value,
            new(startLocation.Line, startLocation.Column, Previous().Line, Previous().Column));
    }

    private VariableDeclarationNode ParseVariableDeclaration(bool isExplicit)
    {
        string? explicitType = null;
        var typeToken = Peek();
        if (isExplicit)
        {
            explicitType = Advance().Lexeme;
        }
        
        var nameToken = Consume(TokenType.Identifier, "Expected identifier after 'let' or type keyword");
        var name = nameToken.Lexeme;
        IExpressionNode? initializer = null;
        if (Match("="))
            initializer = ParseExpression();
        
        if (!isExplicit && initializer == null)
            throw new Exception($"Implicit variable declaration requires an initializer at line {Peek().Line}, column {Peek().Column}.");
        Expect(";", "Expected ';' after variable declaration");
        return new VariableDeclarationNode(name, explicitType, initializer, new (typeToken.Line, typeToken.Column, Previous().Line, Previous().Column));
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Expect(";", "Expected ';' after expression");
        return new ExpressionStatementNode(expr, expr.Span);
    }

    private IExpressionNode ParseExpression()
    {
        return ParseBinary();
    }

    private IExpressionNode ParseBinary()
    {
        var left = ParsePrimary();

        while (IsBinaryOperator(Peek().Lexeme))
        {
            var opToken = Advance();
            var right = ParsePrimary(); // Precedence coming later
            left = new BinaryExpressionNode(opToken.Lexeme, left, right,
                new SourceSpan(left.Span.StartLine, left.Span.StartColumn, right.Span.EndLine, right.Span.EndColumn));
        }

        return left;
    }

    private IExpressionNode ParsePrimary()
    {
        var token = Advance();

        return token.Type switch
        {
            TokenType.Number or TokenType.String => new LiteralExpressionNode(
                token.Type == TokenType.Number ? int.Parse(token.Lexeme) : token.Lexeme,
                new(token.Line, token.Column, Peek().Line, Peek().Column)),
            TokenType.Identifier => ParsePossibleCall(new IdentifierExpressionNode(token.Lexeme,
                new SourceSpan(token.Line, token.Column, Peek().Line, Peek().Column))),
            // TODO: Add support for "this" keyword
            TokenType.Keyword when token.Lexeme == "this" => ParsePossibleCall(new IdentifierExpressionNode("this",
                new SourceSpan(token.Line, token.Column, token.Line, token.Column + 4))),
            _ => throw new Exception($"Unexpected token '{token.Lexeme}' at line {token.Line}, column {token.Column}.")
        };
    }

    private IExpressionNode ParsePossibleCall(IExpressionNode expr)
    {
        if (Peek().Lexeme != "." || PeekNext().Type != TokenType.Identifier || PeekOffset(2).Lexeme != "(")
            return expr;
        // Consume the "."
        Advance();
        var methodNameToken = Consume(TokenType.Identifier, "Expected method name after '.'");
        var arguments = new List<IExpressionNode>();
        // Consume the "("
        Advance();
        if (!Match(")"))
        {
            do
            {
                arguments.Add(ParseExpression());
            } while (Match(","));

            Expect(")", "Expected ')' after arguments");
        }

        return new CallExpressionNode(expr, arguments, methodNameToken.Lexeme,
            new SourceSpan(expr.Span.StartLine, expr.Span.StartColumn, Previous().Line, Previous().Column));
    }

    #region Utilities

    private bool IsAtEnd() => Peek().Type == TokenType.EndOfFile;
    private Token.Token Peek() => _tokens[_position];
    private Token.Token PeekNext() => _tokens[_position + 1];
    private Token.Token PeekOffset(int offset) => _tokens[_position + offset];
    private Token.Token Advance() => _tokens[_position++];
    private Token.Token Previous() => _tokens[_position - 1];

    private bool Match(string lexeme)
    {
        if (IsAtEnd()) return false;
        if (Peek().Lexeme != lexeme) return false;
        Advance();
        return true;
    }

    private void Expect(string lexeme, string errorMessage)
    {
        if (!Match(lexeme))
            throw new Exception($"{errorMessage} at line {Peek().Line}, column {Peek().Column}.");
    }

    private Token.Token Consume(TokenType type, string errorMessage)
    {
        if (IsAtEnd())
            throw new Exception("Expected token of type " + type + " but reached end of file.");
        if (Peek().Type != type)
            throw new Exception(errorMessage + " at line " + Peek().Line + ", column " + Peek().Column + ".");
        return Advance();
    }

    private bool Check(params TokenType[] types) => types.Contains(Peek().Type);

    private static bool IsBinaryOperator(string lexeme) =>
        lexeme is "+" or "-" or "*" or "/" or "==" or "!=" or ">" or "<" or ">=" or "<=";

    #endregion
}