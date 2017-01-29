// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using Stark.Compiler.Collections;

namespace Stark.Compiler.Parsing
{
    /// <summary>
    /// (trait) CharacterIterator ala Stark
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface CharacterIterator : Iterator<char32, int>
    {
    }
}