// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System.Text;
using NUnit.Framework;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Tests
{
    [TestFixture]
    public class TestCharHelper
    {
        [Test]
        public void TestUtf8()
        {
            // A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).
            for (int i = 1; i <= 0x10ffff; i++)
            {
                if (i == 0x00d800)
                {
                    i = 0x00dfff;
                    continue;
                }

                var bytes = Encoding.UTF8.GetBytes(char.ConvertFromUtf32(i));

                int position = 0;
                var ch = CharHelper.ToUtf8(bytes, ref position);
                Assert.True(ch.HasValue);
                Assert.AreEqual(i, (int)ch.Value);
            }
        }
    }
}