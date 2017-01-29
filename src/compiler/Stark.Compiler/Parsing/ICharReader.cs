// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Parsing
{
    public interface ICharReader
    {
        int Reset();

        int NextChar(ref TextPosition position);
    }
}