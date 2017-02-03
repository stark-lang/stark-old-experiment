﻿using System;
using System.Globalization;
using System.IO;

namespace SpecsToAntlrApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage error: {0} source.md dest.g4", Environment.CommandLine);
                Environment.Exit(1);
            }

            // Check if we need to regenerate the file
            var thisAssemblyTime = File.GetLastWriteTime(typeof(Program).Assembly.Location);
            var srcFileTime = File.GetLastWriteTime(args[0]);
            var destFileTime = File.GetLastWriteTime(args[1]);
            if (srcFileTime < destFileTime && destFileTime > thisAssemblyTime)
            {
                return;
            }

            Console.WriteLine($"Update ANTLR [{args[1]}] from specs [{args[0]}]");

            var mdFile = File.ReadAllLines(args[0]);
            using (var stream = new FileStream(args[1], FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {            
                writer.WriteLine("// -----------------------------------------------------------------");
                writer.WriteLine("// This file was automatically generated from {0}", Path.GetFileName(args[0]));
                writer.WriteLine("// DO NOT EDIT THIS FILE MANUALLY");
                writer.WriteLine("// -----------------------------------------------------------------");

                writer.WriteLine();
                var isAntlr = false;
                foreach (var line in mdFile)
                {
                    if (line.StartsWith("```antlr", true, CultureInfo.InvariantCulture))
                    {
                        writer.WriteLine();
                        isAntlr = true;
                        continue;
                    }

                    if (isAntlr)
                    {
                        if (line.StartsWith("```"))
                        {
                            writer.WriteLine();
                            isAntlr = false;
                            continue;
                        }
                        writer.WriteLine(line);
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        writer.WriteLine("// " + line);
                    }
                }
            }
        }
    }
}
