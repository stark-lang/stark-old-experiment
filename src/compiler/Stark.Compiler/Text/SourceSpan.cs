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

        public string FileName { get; set; }

        public TextPosition Start { get; set; }

        public TextPosition End { get; set; }

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