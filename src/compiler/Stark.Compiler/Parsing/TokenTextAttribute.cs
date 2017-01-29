// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;

namespace Stark.Compiler.Parsing
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class TokenTextAttribute : Attribute
    {
        public TokenTextAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}