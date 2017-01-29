// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Collections
{
    /// <summary>
    /// Iterator ala Stark.
    /// </summary>
    /// <typeparam name="TElement">The type of an element of the iteration.</typeparam>
    /// <typeparam name="TState">The type of the state of the iteration.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    // ReSharper disable once InconsistentNaming
    public interface Iterator<TElement, TState> where TElement : struct
    {
        /// <summary>
        /// Gets the start state for the iteration.
        /// </summary>
        TState Start { get; }

        /// <summary>
        /// Tries to get the next element in the iteration.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>none if no element, or an element</returns>
        TElement? TryGetNext(ref TState state);
    }
}