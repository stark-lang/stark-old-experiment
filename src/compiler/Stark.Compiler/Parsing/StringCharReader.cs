// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System.Runtime.CompilerServices;

namespace Stark.Compiler.Parsing
{
    public struct StringCharReader : ICharReader
    {
        private readonly string text;
        private int c;

        public StringCharReader(string text)
        {
            this.text = text;
            c = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextChar(ref TextPosition position)
        {
            position.Offset++;
            if (position.Offset < text.Length)
            {
                if (c == '\n')
                {
                    position.Column = 0;
                    position.Line += 1;
                }
                else
                {
                    position.Column++;
                }

                var c1 = text[position.Offset];

                // Handle surrogates
                c = char.IsHighSurrogate(c1) ? NextCharWithSurrogate(ref position, c1) : c1;
            }
            else
            {
                position.Offset = text.Length;
                c = -1;
            }
            return c;
        }

        private int NextCharWithSurrogate(ref TextPosition position, char c1)
        {
            position.Offset++;
            if (position.Offset < text.Length)
            {
                var c2 = text[position.Offset];
                if (char.IsLowSurrogate(c2))
                {
                    return char.ConvertToUtf32(c1, c2);
                }
                throw new CharReaderException("Unexpected character after high-surrogate char");
            }
            throw new CharReaderException("Unexpected EOF after high-surrogate char");
        }

        public int Reset()
        {
            c = text.Length > 0 ? text[0] : -1;
            return c;
        }
    }
}