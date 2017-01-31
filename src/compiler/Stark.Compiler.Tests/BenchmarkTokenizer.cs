// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using Antlr4.Runtime;
using BenchmarkDotNet.Attributes;
using Stark.Compiler.Parsing;

namespace Stark.Compiler.Tests
{
    /// <summary>
    /// Simple benchmark to compare the performance fo the Stark Tokenizer vs ANTLR generated lexer
    /// </summary>
    public class BenchmarkTokenizer
    {
        private readonly string text;
        public BenchmarkTokenizer()
        {
            text = TestTokenizer.LoadTestTokens();
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

        [Benchmark(Description = "Stark Tokenizer")]
        public void StarkTokenizer()
        {
            var tokenizer = new Tokenizer<StringCharacterIterator>(new StringCharacterIterator(text));
            foreach (var token in tokenizer)
            {
            }
        }
    }
}