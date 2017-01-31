// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace XIDGenApp
{
    /// <summary>
    /// Extract from unicode.org the XID_Start and XID_Continue range characters
    /// </summary>
    class Program
    {
        struct CharRange
        {
            public CharRange(int start, int end)
            {
                Start = start;
                End = end;
            }

            public int Start;

            public int End;
        }

        static void Main(string[] args)
        {
            const bool GenerateANTLR = true;

            var httpClient = new WebClient();
            var data = httpClient.DownloadString("http://unicode.org/Public/UNIDATA/DerivedCoreProperties.txt");

            var stringReader = new StringReader(data);

            var sep = new char[] { ';' };
            string line;
            var XID_Start_ranges = new List<CharRange>();
            var XID_Continue_ranges = new List<CharRange>();
            while ((line = stringReader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.StartsWith("@"))
                {
                    continue;
                }
                var commentIndex = line.IndexOf('#');
                var dataLine = commentIndex > 0 ? line.Substring(0, commentIndex) : line;

                var columns = dataLine.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length != 2)
                {
                    continue;
                }

                var rangeStr = columns[0].Trim();
                var category = columns[1].Trim();

                if (category != "XID_Start" && category != "XID_Continue")
                {
                    continue;
                }

                string fromStr = rangeStr;
                string toStr = fromStr;

                var splitRangeIndex = rangeStr.IndexOf("..", StringComparison.Ordinal);
                if (splitRangeIndex > 0)
                {
                    fromStr = rangeStr.Substring(0, splitRangeIndex);
                    toStr = rangeStr.Substring(splitRangeIndex + 2);
                }


                var from = Convert.ToInt32(fromStr, 16);
                var to = Convert.ToInt32(toStr, 16);
                var range = new CharRange(from, to);

                if (category == "XID_Start")
                {
                    XID_Start_ranges.Add(range);
                }
                else
                {
                    XID_Continue_ranges.Add(range);
                }
            }

            if (GenerateANTLR)
            {
                var dumpRange = new Action<int, CharRange>((i, range) =>
                {
                    var prefix = i > 0 ? "   | " : "     ";
                    if (range.Start > 0xFFFF || range.End > 0xFFFF)
                    {
                        // ANTLR doesn't support UTF32 ranges (or even UTF16 ranges)
                        // So we skip them for the XID_Start
                        // https://github.com/antlr/antlr4/issues/276
                        //
                        //var startRange = char.ConvertFromUtf32(range.Start);
                        //var endRange = char.ConvertFromUtf32(range.End);
                        //Debug.Assert(startRange.Length == 2);
                        //Debug.Assert(endRange.Length == 2);

                        //if (range.Start == range.End)
                        //{
                        //    Console.WriteLine($"{prefix}'\\u{(int) startRange[0]:X4}' '\\u{(int) startRange[1]:X4}'");
                        //}
                        //else
                        //{
                        //    // Because ANTLR doesn't support yet UTF-32 \U00000000 we need to convert a UTF-32 range
                        //    // to multiple UTF16 ranges
                        //    if (startRange[0] == endRange[0])
                        //    {
                        //        Console.WriteLine($"{prefix}'\\u{(int) startRange[0]:X4}' '\\u{(int) startRange[1]:X4}'..'\\u{(int) endRange[1]:X4}'");
                        //    }
                        //    else
                        //    {
                        //        for (int j = startRange[0]; j <= endRange[0]; j++)
                        //        {
                        //            int fromRange = j == startRange[0] ? startRange[1] : 0xDC00;
                        //            int toRange = j < endRange[0] ? 0xDFFF : endRange[1];

                        //            Console.WriteLine($"{prefix}'\\u{j:X4}' '\\u{fromRange:X4}'..'\\u{toRange:X4}'");
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        Console.WriteLine(range.Start == range.End
                            ? $"{prefix}[\\u{range.Start:X4}]"
                            : $"{prefix}[\\u{range.Start:X4}-\\u{range.End:X4}]");
                    }
                });

                Console.WriteLine("lexer grammar XID_Start_Continue;");
                Console.WriteLine("// NOTE: Because ANTLR doesn't support yet UTF-32 they are not included in the following set");
                Console.WriteLine("// See https://github.com/antlr/antlr4/issues/276");
                Console.WriteLine();

                // We add the character _ for XID_Start
                XID_Start_ranges.Insert(1, new CharRange(0x5F, 0x5F));

                Console.WriteLine("XID_Start: ");
                for (var i = 0; i < XID_Start_ranges.Count; i++)
                {
                    var range = XID_Start_ranges[i];
                    dumpRange(i, range);
                }
                Console.WriteLine("   ;");

                Console.WriteLine("XID_Continue: ");
                for (var i = 0; i < XID_Continue_ranges.Count; i++)
                {
                    var range = XID_Continue_ranges[i];
                    dumpRange(i, range);
                }
                Console.WriteLine("   ;");
            }
            else
            {
                Console.WriteLine("private static readonly CharacterRange[] XID_Start_CharacterRanges = new CharacterRange[] {");
                foreach (var range in XID_Start_ranges)
                {
                    Console.WriteLine("new CharacterRange(0x" + range.Start.ToString("X") + ", 0x" + range.End.ToString("X") + "),");
                }
                Console.WriteLine("};");

                Console.WriteLine("private static readonly CharacterRange[] XID_Continue_CharacterRanges = new CharacterRange[] {");
                foreach (var range in XID_Continue_ranges)
                {
                    Console.WriteLine("new CharacterRange(0x" + range.Start.ToString("X") + ", 0x" + range.End.ToString("X") + "),");
                }
                Console.WriteLine("};");

            }
        }

    }
}
