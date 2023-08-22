namespace Emerald.CodeAnalysis
{
    public enum SyntaxKind
    {
        // Tokens
        LiteralToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        
        // Expressions
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression
    }

}