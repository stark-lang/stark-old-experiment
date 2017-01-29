// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;

namespace Stark.Compiler.Parsing
{
    public sealed class CharReaderException : Exception
    {
        public CharReaderException(string message) : base(message)
        {
        }
    }
}