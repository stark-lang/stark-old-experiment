// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
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
