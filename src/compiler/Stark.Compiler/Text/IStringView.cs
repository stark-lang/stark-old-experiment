// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Text
{
    public interface IStringView
    {
        string GetString(int offset, int length);
    }

    public interface IStringView<out TCharIterator> : IStringView where TCharIterator : struct, CharacterIterator
    {
        TCharIterator GetIterator();
    }

}