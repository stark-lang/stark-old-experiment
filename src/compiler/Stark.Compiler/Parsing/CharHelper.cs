// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;

namespace Stark.Compiler.Parsing
{
    static partial class CharHelper
    {
        public static readonly Func<int, bool> IsHexa = IsHexaFunction;
        public static readonly Func<int, bool> IsOctal = IsOctalFunction;
        public static readonly Func<int, bool> IsBinary = IsBinaryFunction;

        public static bool IsIdentifierStart(int c)
        {
            // Extracted from http://unicode.org/Public/UNIDATA/DerivedCoreProperties.txt with XIDStartContinueGen
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return true;
            }
            return CharacterRangeContains(XID_Start_CharacterRanges, c);
        }


        public static bool IsIdentifierContinue(int c)
        {
            if (c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            {
                return true;
            }
            return CharacterRangeContains(XID_Continue_CharacterRanges, c);
        }

        public static bool IsDigit(int c)
        {
            return (c >= '0' && c <= '9');
        }

        public static bool IsWhiteSpace(int c)
        {
            // http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5B%3APattern_White_Space%3A%5D&g=&i=
            // Pattern_White_Space except \r and \n

            return c == ' ' || // space
                   c == '\t' || // horizontal tab
                   c == '\u000b' || // vertical tab
                   c == '\u000c' || // form feed
                   c == '\u0085' || // next line
                   c == '\u200e' || // left-to-right mark
                   c == '\u200f' || // right-to-left-mark
                   c == '\u2028' || // line separator
                   c == '\u2029';   // paragraph separator
        }

        private static bool IsHexaFunction(int c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
        private static bool IsOctalFunction(int c)
        {
            return (c >= '0' && c <= '7');
        }
        private static bool IsBinaryFunction(int c)
        {
            return (c == '0' || c == '1');
        }

        private static bool CharacterRangeContains(CharacterRange[] range, int c)
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