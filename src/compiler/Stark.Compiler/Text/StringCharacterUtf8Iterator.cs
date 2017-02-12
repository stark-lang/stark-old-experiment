// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System.Runtime.CompilerServices;

namespace Stark.Compiler.Text
{
    public struct StringCharacterUtf8Iterator : CharacterIterator
    {
        private readonly byte[] _text;

        public StringCharacterUtf8Iterator(byte[] text)
        {
            this._text = text;
        }

        public int Start => 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char32? TryGetNext(ref int position)
        {
            return CharHelper.ToUtf8(_text, ref position);
        }
    }
}