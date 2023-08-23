namespace Emerald.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        PlusToken,
        StarToken,
        SlashToken,
        MinusToken,
        LiteralToken,
        EndOfFileToken,
        WhiteSpaceToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        
        // Expressions
        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ParenthesizedExpression
    }

}