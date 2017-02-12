// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System.Text;

namespace Stark.Compiler.Text
{
    public struct StringUtf8SourceView: ISourceView<StringCharacterUtf8Iterator>
    {
        private readonly byte[] _text;

        public StringUtf8SourceView(byte[] text, string sourcePath)
        {
            this._text = text;
            SourcePath = sourcePath;
        }

        public string SourcePath { get; }

        public string GetString(int offset, int length)
        {
            if (offset + length <= _text.Length)
            {
                return Encoding.UTF8.GetString(_text, offset, length);
            }
            return null;
        }

        public StringCharacterUtf8Iterator GetIterator()
        {
            return new StringCharacterUtf8Iterator(_text);
        }

    }
}