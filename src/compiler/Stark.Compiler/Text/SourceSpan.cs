// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Text
{
    public struct SourceSpan
    {
        public SourceSpan(string fileName, TextPosition start, TextPosition end)
        {
            FileName = fileName;
            Start = start;
            End = end;
        }

        public string FileName;

        public int Offset => Start.Offset;

        public int Length => End.Offset - Start.Offset + 1;


        public TextPosition Start;

        public TextPosition End;

        public override string ToString()
        {
            return $"{FileName}({Start})-({End})";
        }

        public string ToStringSimple()
        {
            return $"{FileName}({Start.ToStringSimple()})";
        }
    }
}