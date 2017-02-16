// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Text
{

    public interface ISourceView : IStringView
    {
        string SourcePath { get; }
    }

    public interface ISourceView<out TCharIterator> : ISourceView, IStringView<TCharIterator> where TCharIterator : struct, CharacterIterator
    {
    }
}