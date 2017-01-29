// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stark.Compiler.Parsing;

namespace Stark.Compiler.Tests
{
    /// <summary>
    /// Tests for the <see cref="Tokenizer{TReader}"/>.
    /// </summary>
    [TestFixture]
    public class TestTokenizer
    {
        [Test]
        public void ParseSimpleTokens()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"@", TokenType.At},
                {"$", TokenType.Dollar},
                {"~", TokenType.Tilde},
                {"\\", TokenType.Backslash},
                {"`", TokenType.GraveAccent},
                {"^", TokenType.Exponent},
                {";", TokenType.SemiColon},
                {":", TokenType.Colon},
                {"::", TokenType.ColonColon},
                {"*", TokenType.Star},
                {"+", TokenType.Plus},
                {"-", TokenType.Minus},
                {"++", TokenType.PlusPlus},
                {"--", TokenType.MinusMinus},
                {"/", TokenType.Divide},
                {"#", TokenType.Hash},
                {"%", TokenType.Modulus},
                {"=", TokenType.Equal},
                {"!", TokenType.Not},
                {"|", TokenType.Pipe},
                {",", TokenType.Comma},
                {".", TokenType.Dot},
                {"(", TokenType.OpenParent},
                {")", TokenType.CloseParent},
                {"[", TokenType.OpenBracket},
                {"]", TokenType.CloseBracket},
                {"{", TokenType.OpenBrace},
                {"}", TokenType.CloseBrace},
                {"<", TokenType.Less},
                {"<<", TokenType.LessLess},
                {">", TokenType.Greater},
                {">>", TokenType.GreaterGreater},
                {"!=", TokenType.NotEqual},
                {"==", TokenType.EqualEqual},
                {">=", TokenType.GreaterEqual},
                {"=>", TokenType.EqualGreater},
                {"->", TokenType.MinusGreater},
                {"<=", TokenType.LessEqual},
                {"&", TokenType.And},
                {"&&", TokenType.AndAnd},
                {"?", TokenType.Question},
                {"??", TokenType.QuestionQuestion},
                {"||", TokenType.PipePipe},
                {"..", TokenType.DotDot},
                {"..<", TokenType.DotDotLess},
                {"...", TokenType.DotDotDot},
                {"\r", TokenType.NewLine},
                {"\r\n", TokenType.NewLine},
                {"\n", TokenType.NewLine},
                {" ", TokenType.Spaces},
                {"\t", TokenType.Spaces},
                {" \t", TokenType.Spaces},
                {"\b", TokenType.Invalid},
            });
        }

        [Test]
        public void ParseIdentifier()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"_", TokenType.Underscores},
                {"____", TokenType.Underscores},
                {"t_st", TokenType.Identifier},
                {"test", TokenType.Identifier},
                {"t999", TokenType.Identifier},
                {"_est", TokenType.Identifier},
                {"_999", TokenType.Identifier},
                {"_a", TokenType.Identifier},
                {"_B", TokenType.Identifier},
                {"_9", TokenType.Identifier},
                {"áê", TokenType.Identifier},
                {"_áê", TokenType.Identifier},
            });
        }

        [Test]
        public void ParseNumbers()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"1", TokenType.Integer},
                {"10", TokenType.Integer},
                {"100000", TokenType.Integer},
                {"123456789", TokenType.Integer},
                {"100_200", TokenType.Integer},
                {"100_200___", TokenType.Integer},

                {"0x01", TokenType.IntegerHexa},
                {"0X01", TokenType.IntegerHexa},
                {"0x123456789ABCDEF", TokenType.IntegerHexa},
                {"0x01_02", TokenType.IntegerHexa},

                {"0o777", TokenType.IntegerOctal},
                {"0O777", TokenType.IntegerOctal},
                {"0o01234567", TokenType.IntegerOctal},
                {"0o77_77", TokenType.IntegerOctal},

                {"0b1001", TokenType.IntegerBinary},
                {"0B1010", TokenType.IntegerBinary},
                {"0b10_01", TokenType.IntegerBinary},

                {"1e1", TokenType.Float},
                {"1.0", TokenType.Float},
                {"1.0e1", TokenType.Float},
                {"10.1", TokenType.Float},
                {"2.01235", TokenType.Float},
                {"3.01235e1", TokenType.Float},
                {"4.01235e-1", TokenType.Float},
                {"5.01235e+1", TokenType.Float},
            });
        }

        [Test]
        public void ParseIntegerFollowedByIdentifier()
        {
            var list = ParseTokens("0a", false);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Integer, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(1, 0, 1), new TextPosition(1, 0, 1)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseIntegerFollowedByRange()
        {
            var list = ParseTokens("1..");
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Integer, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)), list[0]);
            Assert.AreEqual(new Token(TokenType.DotDot, new TextPosition(1, 0, 1), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseInvalidHexa()
        {
            // Expect numbers after exponent
            var list = ParseTokens("0xz", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseInvalidNumberWithExponent1()
        {
            // Expect numbers after exponent
            var list = ParseTokens("1ex", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseInvalidNumberWithExponent2()
        {
            // Expect numbers after exponent
            var list = ParseTokens("1e+x", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }
        
        [Test]
        public void ParseInvalidNumberWithDot()
        {
            // Expect digits after dot .
            var list = ParseTokens("1.a", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseCommentSingleLine()
        {
            var comment = "// This is a comment";
            VerifyCodeBlock(comment, new Token(TokenType.Comment, new TextPosition(0, 0, 0), new TextPosition(comment.Length - 1, 0, comment.Length - 1)) );
        }

        [Test]
        public void ParseCommentMultiLineOnSingleLine()
        {
            {
                var comment = @"/* This a multi-line comment on a single line */";
                VerifyCodeBlock(comment, new Token(TokenType.CommentMultiLine, new TextPosition(0, 0, 0), new TextPosition(comment.Length - 1, 0, comment.Length - 1)));
            }

            {
                var comment = @"/* This a multi-line comment on a single line without a ending";
                VerifyCodeBlockWithErrors(comment, new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(comment.Length - 1, 0, comment.Length - 1)));
            }
        }

        [Test]
        public void ParseCommentMultiLine()
        {
            var text = @"/* This a multi-line 
comment on a 
multi-line 
*/";
            VerifyCodeBlock(text, new Token(TokenType.CommentMultiLine, new TextPosition(0, 0, 0), new TextPosition(text.Length - 1, 3, 1)));
        }


        [Test]
        public void ParseCommentMultiLineNested()
        {
            var text = @"/* This a multi-line /*
comment nested on a */
multi-line 
*/";
            VerifyCodeBlock(text, new Token(TokenType.CommentMultiLine, new TextPosition(0, 0, 0), new TextPosition(text.Length - 1, 3, 1)));
        }

        [Test]
        public void ParseStringSingleLine()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {@"""This a string on a single line""", TokenType.String},
                {@"""This a string with an escape \"" and escape \\ """, TokenType.String},
                {@"""This a string with \0 \b \n \u0000 \uFFFF \x00 \xff""", TokenType.String},
            });
        }

        [Test]
        public void ParseChar()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {@"'a'", TokenType.Char},
                {@"'\''", TokenType.Char},
                {@"'\r'", TokenType.Char},
                {@"'\n'", TokenType.Char},
                {@"'\t'", TokenType.Char},
                {@"'\u0000'", TokenType.Char},
                {@"'\u12ab'", TokenType.Char},
                {@"'\uabcd'", TokenType.Char},
                {@"'\uFFFF'", TokenType.Char},
                {@"'\x00'", TokenType.Char},
                {@"'\x1a'", TokenType.Char},
                {@"'\xab'", TokenType.Char},
                {@"'\xFF'", TokenType.Char},
            });
        }

        [Test]
        public void ParseInvalidChar()
        {
            var list = ParseTokens("'", true);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)), list[0]);
            Assert.AreEqual(Token.Eof, list[1]);
        }

        [Test]
        public void ParseInvalidChar1()
        {
            var list = ParseTokens("'a", true);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(Token.Eof, list[1]);
        }

        [Test]
        public void ParseInvalidChar2()
        {
            var list = ParseTokens("'ab'", true);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[2]);
            Assert.AreEqual(Token.Eof, list[3]);
        }

        [Test]
        public void ParseInvalidCharEscape_u()
        {
            var list = ParseTokens("'\\u'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);

            list = ParseTokens("'\\u0'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(3, 0, 3)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(4, 0, 4), new TextPosition(4, 0, 4)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);

            list = ParseTokens("'\\u00'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(4, 0, 4)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(5, 0, 5), new TextPosition(5, 0, 5)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);

            list = ParseTokens("'\\u000'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(5, 0, 5)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(6, 0, 6), new TextPosition(6, 0, 6)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseInvalidCharEscape_x()
        {
            var list = ParseTokens("'\\x'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);

            list = ParseTokens("'\\x0'", true);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(3, 0, 3)), list[0]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(4, 0, 4), new TextPosition(4, 0, 4)), list[1]);
            Assert.AreEqual(Token.Eof, list[2]);
        }

        [Test]
        public void ParseInvalidCharEscape_z()
        {
            var list = ParseTokens("'\\z'", true);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[2]);
            Assert.AreEqual(Token.Eof, list[3]);
        }

        [Test]
        public void ParseStringWithInvalidEscape()
        {
            var list = ParseTokens("\"\\z\"", true);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), list[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)), list[1]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)), list[2]);
            Assert.AreEqual(Token.Eof, list[3]);
        }

        [Test]
        public void ParseStringWithInvalidEOL()
        {
            var list = ParseTokens("\"\n\"", true);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)), list[0]);
            Assert.AreEqual(new Token(TokenType.NewLine, new TextPosition(1, 0, 1), new TextPosition(1, 0, 1)), list[1]);
            Assert.AreEqual(new Token(TokenType.Invalid, new TextPosition(2, 1, 0), new TextPosition(2, 1, 0)), list[2]);
            Assert.AreEqual(Token.Eof, list[3]);
        }

        [Test]
        public void ParseStringMultiLine()
        {
            var text = @"@""This a string on """"
a single line
""";
            VerifyCodeBlock(text, new Token(TokenType.String, new TextPosition(0, 0, 0), new TextPosition(text.Length - 1, 2, 0)));
        }

        [Test]
        public void ParseMultiples()
        {
            //           0123456 7
            var text = "10ab /\n{";
            var lexer = new Tokenizer<StringCharacterIterator>(new StringCharacterIterator(text));
            Assert.False((bool)lexer.HasErrors);
            var tokens = lexer.ToList<Token>();
            Assert.AreEqual(7, tokens.Count);
            Assert.AreEqual(new Token(TokenType.Integer, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)), tokens[0]);
            Assert.AreEqual(new Token(TokenType.Identifier, new TextPosition(2, 0, 2), new TextPosition(3, 0, 3)), tokens[1]);
            Assert.AreEqual(new Token(TokenType.Spaces, new TextPosition(4, 0, 4), new TextPosition(4, 0, 4)), tokens[2]);
            Assert.AreEqual(new Token(TokenType.Divide, new TextPosition(5, 0, 5), new TextPosition(5, 0, 5)), tokens[3]);
            Assert.AreEqual(new Token(TokenType.NewLine, new TextPosition(6, 0, 6), new TextPosition(6, 0, 6)), tokens[4]);
            Assert.AreEqual(new Token(TokenType.OpenBrace, new TextPosition(7, 1, 0), new TextPosition(7, 1, 0)), tokens[5]);
            Assert.AreEqual(new Token(TokenType.Eof, TextPosition.Eof, TextPosition.Eof), tokens[6]);
        }

        private void VerifySimpleTokens(Dictionary<string, TokenType> simpleTokens)
        {
            foreach (var token in simpleTokens)
            {
                var text = token.Key;
                VerifyCodeBlock(text, new Token(token.Value, new TextPosition(0, 0, 0), new TextPosition(token.Key.Length - 1, 0, token.Key.Length - 1)) );
            }
        }

        private List<Token> ParseTokens(string text, bool hasErrors = false)
        {
            var lexer = new Tokenizer<StringCharacterIterator>(new StringCharacterIterator(text));
            var tokens = lexer.ToList(); // Force to parse all tokens
            foreach (var error in lexer.Errors)
            {
                Console.WriteLine(error);
            }
            Assert.AreEqual(hasErrors, lexer.HasErrors, "Expecting errors");
            return tokens;
        }


        private void VerifyCodeBlockWithErrors(string text, params Token[] expectedTokens)
        {
            VerifyCodeBlock(text, true, expectedTokens);
        }

        private void VerifyCodeBlock(string text, params Token[] expectedTokens)
        {
            VerifyCodeBlock(text, false, expectedTokens);
        }

        private void VerifyCodeBlock(string text, bool expectErrors, params Token[] expectedTokens)
        {
            var expectedTokenList = new List<Token>();
            expectedTokenList.AddRange(expectedTokens);
            expectedTokenList.Add(Token.Eof);

            // Verify tokens
            var tokens = ParseTokens(text, expectErrors);
            Assert.AreEqual(expectedTokenList, tokens, $"Unexpected error while parsing: {text}");
            VerifyTokenGetText(tokens, text);

            // Parse all symbols with spaces before and after
            if (!char.IsWhiteSpace(text[0]) && !tokens.Any(token => token.Type == TokenType.Comment || token.Type == TokenType.Invalid))
            {
                var newText = " " + text + " ";
                var newTokens = ParseTokens(newText);
                Assert.AreEqual(expectedTokenList.Count, newTokens.Count - 2);
                Assert.AreEqual(newTokens[0], new Token(TokenType.Spaces, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)));
                Assert.AreEqual(newTokens[newTokens.Count - 1], Token.Eof);
                if (tokens[0].End.Line > 0)
                {
                    // We don't check newlines when adding space
                    Assert.AreEqual(newTokens[newTokens.Count - 2].Type, TokenType.Spaces);
                }
                else
                {
                    Assert.AreEqual(newTokens[newTokens.Count - 2], new Token(TokenType.Spaces, new TextPosition(newText.Length - 1, 0, newText.Length - 1), new TextPosition(newText.Length - 1, 0, newText.Length - 1)));
                }

                Assert.AreEqual(expectedTokenList, tokens, $"Unexpected error while parsing: {text}");
                VerifyTokenGetText(tokens, text);
            }
        }

        private static void VerifyTokenGetText(List<Token> tokens, string text)
        {
            foreach (var token in tokens)
            {
                var tokenText = token.GetText(text);
                if (token.Type.HasText())
                {
                    Assert.AreEqual(token.Type.ToText(), tokenText, $"Invalid captured text found for standard token [{token.Type}] while parsing: {text}");
                }
            }
        }

    }
}