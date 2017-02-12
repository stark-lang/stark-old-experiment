// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System.Text;
using Antlr4.Runtime;
using BenchmarkDotNet.Attributes;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Tests
{
    /// <summary>
    /// Simple benchmark to compare the performance fo the Stark Lexer vs ANTLR generated lexer
    /// </summary>
    public class BenchmarkTokenizer
    {
        private readonly string text;
        private readonly byte[] textUTF8;
        public BenchmarkTokenizer()
        {
            text = TestLexer.LoadTestTokens();
            textUTF8 = Encoding.UTF8.GetBytes(text);
        }


        [Benchmark(Description = "Stark Lexer")]
        public void StarkTokenizer()
        {
            var tokenizer = new Lexer<StringSourceView, StringCharacterIterator>(new StringSourceView(text, "<input>"));
            foreach (var token in tokenizer)
            {
            }
        }

        [Benchmark(Description = "Stark UTF8 Lexer")]
        public void StarkTokenizerUTF8()
        {
            var tokenizer = new Lexer<StringUtf8SourceView, StringCharacterUtf8Iterator>(new StringUtf8SourceView(textUTF8, "<input>"));
            foreach (var token in tokenizer)
            {
            }
        }

        [Benchmark(Baseline = true, Description = "ANTLR Lexer")]
        public void ANTLRLexer()
        {
            var antlrLexer = new StarkLexer(new AntlrInputStream(text));
            while (true)
            {
                var nextToken = antlrLexer.NextToken();
                if (nextToken.Type == -1)
                {
                    break;
                }
            }
        }
    }
}