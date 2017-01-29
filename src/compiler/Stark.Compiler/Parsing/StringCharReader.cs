// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System.Runtime.CompilerServices;

namespace Stark.Compiler.Parsing
{
    public struct StringCharReader : ICharReader
    {
        private readonly string _text;
        private int _c;

        public StringCharReader(string text)
        {
            this._text = text;
            _c = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char32 NextChar(ref TextPosition position)
        {
            position.Offset++;
            if (position.Offset < _text.Length)
            {
                if (_c == '\n')
                {
                    position.Column = 0;
                    position.Line += 1;
                }
                else
                {
                    position.Column++;
                }

                var c1 = _text[position.Offset];

                // Handle surrogates
                _c = char.IsHighSurrogate(c1) ? NextCharWithSurrogate(ref position, c1) : c1;
            }
            else
            {
                position.Offset = _text.Length;
                _c = -1;
            }
            return _c;
        }

        private int NextCharWithSurrogate(ref TextPosition position, char c1)
        {
            position.Offset++;
            if (position.Offset < _text.Length)
            {
                var c2 = _text[position.Offset];
                if (char.IsLowSurrogate(c2))
                {
                    return char.ConvertToUtf32(c1, c2);
                }
                throw new CharReaderException("Unexpected character after high-surrogate char");
            }
            throw new CharReaderException("Unexpected EOF after high-surrogate char");
        }

        public char32 Reset()
        {
            _c = _text.Length > 0 ? _text[0] : -1;
            return _c;
        }
    }
}