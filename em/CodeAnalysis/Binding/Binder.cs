using System;
using Emerald.CodeAnalysis.Syntax;

namespace Emerald.CodeAnalysis.Binding  
{
    internal enum BoundNodeKind
    {
        LiteralExpression,
        UnaryExpression,
        BinaryExpression
    }
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
    internal enum BoundUnaryOperatorKind
    {
        Identity,
        Negation
    }
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override Type Type => Value.GetType();
        public object Value { get; }
    }
    internal sealed class BoundUnaryExpression : BoundExpression{
        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression right)
        {
            OperatorKind = operatorKind;
            Operand = right;
        }
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type Type => Operand.Type;
        public BoundUnaryOperatorKind OperatorKind { get; }
        public BoundExpression Operand { get; }
    }
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }
    internal sealed class BoundBinaryExpression : BoundExpression{
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Right = right;
        }
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression; //BinaryExpression
        public override Type Type => Left.Type;
        public BoundExpression Left { get; }
        public BoundBinaryOperatorKind OperatorKind { get; }
        public BoundExpression Right { get; }
    }
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();
        public IEnumerable<string> Diagnostics => _diagnostics;
        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch(syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                
                default:
                    throw new Exception($"Unexpected sybtax {syntax.Kind}");    
            }
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.LiteralToken.Value as int? ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, boundOperand.Type);
            if(boundOperatorKind == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not defined for types {boundLeft.Type} and {boundRight.Type}");
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperatorKind, boundOperand);
        }
        
        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindUnaryExpression(syntax.Left);
            var boundRight = BindUnaryExpression(syntax.Right);
            var boundOperatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            return new BoundBinaryExpression(boundLeft, boundOperatorKind, boundRight);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if(operandType != typeof(int))
                return null;

             switch(kind)
             {
                case SyntaxKind.PlusToken:
                    return BoundUnaryOperatorKind.Identity;
                case SyntaxKind.MinusToken:
                    return BoundUnaryOperatorKind.Negation;
                default:
                    throw new Exception($"Unexpected unary operator {kind}");        
             }
        }
        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if(leftType != typeof(int) || rightType != typeof(int))
                return null;

             switch(kind)
             {
                case SyntaxKind.PlusToken:
                    return BoundBinaryOperatorKind.Addition;
                case SyntaxKind.MinusToken:
                    return BoundBinaryOperatorKind.Subtraction;
                case SyntaxKind.StarToken:
                    return BoundBinaryOperatorKind.Multiplication;
                case SyntaxKind.SlashToken:
                    return BoundBinaryOperatorKind.Division;
                default:
                    throw new Exception($ "Unexpected binary operator {kind}");                
             }
        }
    }
}