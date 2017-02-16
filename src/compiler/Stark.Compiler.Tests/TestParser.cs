// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using NUnit.Framework;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Tests
{
    [TestFixture]
    public class TestParser
    {
        [Test]
        public void TestModule()
        {
            // TODO: Not a test, just a playground for testing the parser
            var input = "public module Test";

            var lexer = new Lexer<StringSourceView, StringCharacterIterator>(new StringSourceView(input, "<input>"));
            var parser = new Parser<StringSourceView>(lexer);

            var directives = parser.Run();
        }
    }
}