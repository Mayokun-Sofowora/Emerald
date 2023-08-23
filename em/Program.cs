using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;

using Emerald.CodeAnalysis;
using Emerald.CodeAnalysis.Syntax;


namespace Emerald
{
    internal static class Program{
        private static void Main()
        {
            var showTree = false;
            while(true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                if(line == "#showTree")
                {   
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing Parse Trees." : "Not Showing Parse Trees.");  
                    continue;
                }
                else if(line == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                //var parser = new Parser(line);
                //var syntaxTree = parser.Parse();
                var syntaxTree = SyntaxTree.Parse(line); //for better API
                if(showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!syntaxTree.Diagnostics.Any())
                {
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);
                    Console.ResetColor();
                }
            }
        }
        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);
            
            if(node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }
            
            Console.WriteLine();
            
            indent += isLast ? "   " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            //indent += "    "; //indent 4 spaces.
            foreach(var child in node.GetChildren())
                PrettyPrint(child, indent, child == lastChild);
        }
    }
    
}