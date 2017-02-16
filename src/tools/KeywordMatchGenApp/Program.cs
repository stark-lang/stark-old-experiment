using System;
using System.Collections.Generic;
using Stark.Compiler.Text;

namespace KeywordMatchGenApp
{

    /// <summary>
    /// Program that pre-calculates the TextMatcher nodes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var types =
                "class|struct|trait|enum";

            var modifiers =
                "abstract|async|const|extern|immutable|internal|isolated|mutable|new|override|partial|private|protected|public|readonly|scoped|sealed|static|transient|unsafe|virtual|volatile";

            var keywords =
                "alias|as|await|base|break|constructor|default|else|extends|false|fatal|fixed|for|from|func|get|if|implements|import|in|is|let|match|meta|module|operator|out|package|params|permanent|ref|requires|return|set|sizeof|this|throw|true|typeof|value|var|where|while|with";

            var text = modifiers + "|" + keywords + "|" + types;
            var entries = text.Split('|');
            Array.Sort(entries);

            Console.WriteLine("ANTLR Lexer:");
            var mapEntries = new Dictionary<string, int>();
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                mapEntries.Add(entry, i);

                Console.WriteLine(entry.ToUpperInvariant() + ": '" + entry + "';");
            }

            Console.WriteLine();
            Console.WriteLine("TokenTypes:");
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var entryName = char.ToUpperInvariant(entry[0]) + entry.Substring(1);
                Console.WriteLine($"[TokenText(\"{entry}\")]");
                Console.WriteLine($"{entryName},");
            }

            Console.WriteLine();
            Console.WriteLine("TextMatcher precalculated:");
            var matcher = new TextMatcher<int>(mapEntries);
            Console.WriteLine("new TextMatcher<TokenType>.CharNode[" + matcher.Nodes.Length + "] {");

            for (var i = 0; i < matcher.Nodes.Length; i++)
            {
                var node = matcher.Nodes[i];
                if (node.Result.HasValue)
                {
                    var entryName = entries[node.Result.Value];
                    entryName = char.ToUpperInvariant(entryName[0]) + entryName.Substring(1);
                    Console.WriteLine($"/* {i:D3} */ new TextMatcher<TokenType>.CharNode('{node.c}', {node.Offset}, {node.Count}, TokenType.{entryName}),");
                }
                else
                {
                    var cAsText = node.c == 0 ? "\\0" : node.c.ToString();
                    Console.WriteLine($"/* {i:D3} */ new TextMatcher<TokenType>.CharNode('{cAsText}', {node.Offset}, {node.Count}),");
                }
            }
            Console.WriteLine("}");
        }
    }
}
