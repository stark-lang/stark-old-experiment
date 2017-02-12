// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using NUnit.Framework;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Tests
{
    [TestFixture]
    public class TestMatcher
    {
        [Test]
        public void TestSimpleMatch()
        {
            var dict = new Dictionary<string, int>()
            {
                {"abstract", 1},
                {"alias", 2},
                {"as", 3},
                {"async", 4},
                {"base", 5},
                {"break", 6},
                {"const", 7},
                {"constructor", 8},
                {"default", 9},
                {"else", 10},
                {"extends", 11},
                {"extern", 12},
                {"fatal", 13},
                {"fixed", 14},
                {"for", 15},
                {"from", 16},
                {"func", 17},
                {"get", 18},
                {"if", 19},
                {"immutable", 20},
                {"implements", 21},
                {"import", 22},
                {"in", 23},
                {"internal", 24},
                {"is", 25},
                {"isolated", 26},
                {"let", 27},
                {"match", 28},
                {"meta", 29},
                {"module", 30},
                {"mutable", 31},
                {"new", 32},
                {"operator", 33},
                {"out", 34},
                {"override", 35},
                {"package", 36},
                {"params", 37},
                {"partial", 38},
                {"private", 39},
                {"protected", 40},
                {"public", 41},
                {"readonly", 42},
                {"ref", 43},
                {"requires", 44},
                {"return", 45},
                {"scoped", 46},
                {"sealed", 47},
                {"set", 48},
                {"sizeof", 49},
                {"static", 50},
                {"this", 51},
                {"throw", 52},
                {"transient", 53},
                {"typeof", 54},
                {"unsafe", 55},
                {"value", 56},
                {"virtual", 57},
                {"volatile", 58},
                {"where", 59},
                {"while", 60},
                {"with", 61},
                {"false", 62},
                {"true", 63},
            };

            var textMatcher = new TextMatcher<int>(dict);

            var result = textMatcher.TryMatch("for");
            Assert.True(result.HasValue);
            Assert.AreEqual(15, result.Value);

            result = textMatcher.TryMatch("mutable");
            Assert.True(result.HasValue);
            Assert.AreEqual(31, result.Value);

            result = textMatcher.TryMatch("while");
            Assert.True(result.HasValue);
            Assert.AreEqual(60, result.Value);

            result = textMatcher.TryMatch("transient");
            Assert.True(result.HasValue);
            Assert.AreEqual(53, result.Value);

            result = textMatcher.TryMatch("foo");
            Assert.False(result.HasValue);

            result = textMatcher.TryMatch("modules");
            Assert.False(result.HasValue);

            result = textMatcher.TryMatch("a");
            Assert.False(result.HasValue);

            result = textMatcher.TryMatch("á");
            Assert.False(result.HasValue);

            result = textMatcher.TryMatch("_");
            Assert.False(result.HasValue);

            result = textMatcher.TryMatch("for_");
            Assert.False(result.HasValue);
        }
    }
}