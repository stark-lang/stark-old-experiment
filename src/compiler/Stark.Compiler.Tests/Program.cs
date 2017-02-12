// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Stark.Compiler.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<BenchmarkTokenizer>();
            //var clock = Stopwatch.StartNew();
            //var tokenizer = new BenchmarkTokenizer();
            //for (int i = 0; i < 100000; i++)
            //{
            //    tokenizer.StarkTokenizer();
            //}
            //clock.Stop();
            //Console.WriteLine(clock.ElapsedMilliseconds);
        }
    }
}
