// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;

namespace Stark.Compiler.Parsing
{
    static partial class CharHelper
    {
        public static readonly Func<char32, bool> IsHexa = IsHexaFunction;
        public static readonly Func<char32, bool> IsOctal = IsOctalFunction;
        public static readonly Func<char32, bool> IsBinary = IsBinaryFunction;

        public static bool IsIdentifierStart(char32 c)
        {
            // Extracted from http://unicode.org/Public/UNIDATA/DerivedCoreProperties.txt with XIDStartContinueGen
            // Test regular ASCII characters first before going to a more costly binary search on XID_Start
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return true;
            }
            return CharacterRangeContains(XID_Start_CharacterRanges, c);
        }


        public static bool IsIdentifierContinue(char32 c)
        {
            // Test regular ASCII characters first before going to a more costly binary search on XID_Continue
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            {
                return true;
            }
            return CharacterRangeContains(XID_Continue_CharacterRanges, c);
        }

        public static bool IsDigit(char32 c)
        {
            return (c >= '0' && c <= '9');
        }

        public static bool IsWhiteSpace(char32 c)
        {
            // http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5B%3APattern_White_Space%3A%5D&g=&i=
            // Pattern_White_Space except \r and \n and newlines

            // ZS characters http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5B%3AZS%3A%5D&g=&i=
            return c == ' ' || // space
                   c == '\t' || // horizontal tab
                   c == '\u000b' || // vertical tab
                   c == '\u000c' || // form feed
                   c == '\u200e' || // left-to-right mark
                   c == '\u200f' || // right-to-left-mark
                   c == '\u00A0' || // NO_BREAK SPACE
                   c == '\u1680' || // OGHAM SPACE MARK
                   (c >= '\u2000' && c <= '\u200A') ||
                   //c == '\u2000' || // EN QUAD
                   //c == '\u2001' || // EM QUAD
                   //c == '\u2002' || // EN SPACE
                   //c == '\u2003' || // EM SPACE
                   //c == '\u2004' || // THREE_PER_EM SPACE
                   //c == '\u2005' || // FOUR_PER_EM SPACE
                   //c == '\u2006' || // SIX_PER_EM SPACE
                   //c == '\u2007' || // FIGURE SPACE
                   //c == '\u2008' || // PUNCTUATION SPACE
                   //c == '\u2009' || // THIN SPACE
                   //c == '\u200A' || // HAIR SPACE
                   c == '\u202F' || // NARROW NO_BREAK SPACE
                   c == '\u205F' || // MEDIUM MATHEMATICAL SPACE
                   c == '\u3000'; // IDEOGRAPHIC SPACE
        }

        private static bool IsHexaFunction(char32 c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
        private static bool IsOctalFunction(char32 c)
        {
            return (c >= '0' && c <= '7');
        }
        private static bool IsBinaryFunction(char32 c)
        {
            return (c == '0' || c == '1');
        }

        private static bool CharacterRangeContains(CharacterRange[] range, char32 c)
        {
            int lo = 0;
            int hi = range.Length - 1;
            while (lo <= hi)
            {
                
                int mid = lo + ((hi - lo) >> 1);
                var dir = range[mid].CompareTo(c);
                if (dir == 0)
                {
                    return true;
                }
                if (dir < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }
            return false;
        }
    }
}