// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Stark.Compiler.Text
{
    /// <summary>
    /// Allows to match multiple strings from an input character stream, 
    /// and associate a return value if a match is found.
    /// This is typically used to match keywords while decoding characters
    /// and generate a proper <see cref="TokenType"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the t result.</typeparam>
    /// <remarks>
    /// This class is optimized to build a lookup table that is cache friendly (array of struct, sequential access, no reference types).
    /// The representation is basically a tree of nodes, each node is a character
    /// that is either associated to a TResult and can have many "next" character nodes.
    /// We are using a structure to store a node, and we are making sure that child
    /// nodes are consecutive so we only need to store offset+count on each node.
    /// The offset is relative to the index of the current node.
    /// Internally it is using a binary search.
    /// The first node (_nodes[0]) doesn't have a character, but contains the child
    /// nodes for the first characters.
    /// </remarks>
    internal struct TextMatcher<TResult> where TResult : struct
    {
        private readonly CharNode[] _nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMatcher{TResult}"/> struct.
        /// </summary>
        /// <param name="mapToResult">The map between strings to match and their associated result.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        /// This method is usually not used, as we prefer to precalculate the internal CharNode[] as an offline process
        /// to avoid the calculation of the nodes...etc. at runtime.
        /// </remarks>
        public TextMatcher(Dictionary<string, TResult> mapToResult)
        {
            if (mapToResult == null) throw new ArgumentNullException(nameof(mapToResult));
            var nodeList = new List<CharNode> {new CharNode()};

            var nextList = new List<KeyValuePair<string, TResult>>(mapToResult);
            nextList.Sort((pair, valuePair) => string.Compare(pair.Key, valuePair.Key, StringComparison.Ordinal));

            BuildMap(0, 0, nodeList, nextList);
            _nodes = nodeList.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMatcher{TResult}"/> struct.
        /// </summary>
        /// <param name="nodes">The pre-calculated nodes.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">nodes are not correctly initialized</exception>
        public TextMatcher(CharNode[] nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length <= 1) throw new ArgumentException("nodes are not correctly initialized", nameof(nodes));
            _nodes = nodes;
        }

        /// <summary>
        /// Gets the internal matching nodes. This array must be readonly and must not be modified.
        /// </summary>
        /// <value>The nodes.</value>
        public CharNode[] Nodes => _nodes;

        /// <summary>
        /// Match a full string.
        /// </summary>
        /// <param name="str">The string to match.</param>
        /// <returns>The result or null if not matched.</returns>
        /// <exception cref="ArgumentNullException">if str is null</exception>
        public TResult? TryMatch(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));

            int i = 0;
            int nodeIndex = 0;
            TResult? value = null;
            while (i < str.Length && TryMatch(ref nodeIndex, str[i], out value))
            {
                i++;
            }
            return value;
        }

        /// <summary>
        /// Match a single character and advance to the next char if matched.
        /// </summary>
        /// <param name="nodeIndex">Index of the node (first match must use 0).</param>
        /// <param name="c">The character to match.</param>
        /// <param name="result">The result if matched for this character.</param>
        /// <returns><c>true</c> if c was matched, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryMatch(ref int nodeIndex, char c, out TResult? result)
        {
            result = null;
            return TryMatchByRef(ref nodeIndex, c, ref result);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryMatchByRef(ref int nodeIndex, char c, ref TResult? result)
        {
            if (nodeIndex < 0 || nodeIndex >= _nodes.Length)
            {
                return false;
            }
            return TryMatch(ref nodeIndex, ref _nodes[nodeIndex], c, ref result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryMatch(ref int mid, ref CharNode node, char c, ref TResult? result)
        {
            // the current node gives an access to the next character nodes
            // the first node (Nodes[0]) correspond to the first characters to match
            if (node.Count > 0)
            {
                var lo = (int)node.Offset;
                var hi = lo + node.Count - 1;

                while (lo <= hi)
                {
                    mid = lo + ((hi - lo) >> 1);
                    var dir = _nodes[mid].c - c;
                    if (dir == 0)
                    {
                        result = _nodes[mid].Result;
                        return true;
                    }
                    if (dir < 0)
                    {
                        lo = mid + 1;
                    }
                    else
                    {
                        hi = mid - 1;
                    }
                }
            }
            mid = -1;
            return false;
        }

        static void BuildMap(int nodeIndex, int charIndex, List<CharNode> nodes, List<KeyValuePair<string, TResult>> keyValues)
        {
            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
            var node = nodes[nodeIndex];

            if (keyValues.Count == 0)
            {
                return;
            }

            foreach (var match in keyValues)
            {
                var str = match.Key;
                var c = str[charIndex];

                var charFound = false;
                for (int i = 0; i < node.Count; i++)
                {
                    var nextNodeIndex = node.Offset + i;
                    if (nodes[nextNodeIndex].c == c)
                    {
                        charFound = true;
                        break;
                    }
                }
                if (!charFound)
                {
                    var nextNode = new CharNode(c);
                    if (charIndex + 1 == str.Length)
                    {
                        nextNode.Result = match.Value;
                    }
                    if (node.Count == 0)
                    {
                        node.Offset = (short)nodes.Count;
                    }
                    nodes.Add(nextNode);
                    node.Count++;
                }
            }
            nodes[nodeIndex] = node;

            var nextList = new List<KeyValuePair<string, TResult>>();
            for (int i = 0; i < node.Count; i++)
            {
                nextList.Clear();
                var nextNodeIndex = node.Offset + i;
                var nextNode = nodes[nextNodeIndex];
                // Not hyper optimized (loop of loop), but after the first level, we get only a few elements in keyValues (going mostly log(n))
                foreach (var match in keyValues)
                {
                    if (nextNode.c == match.Key[charIndex] && charIndex + 1 < match.Key.Length)
                    {
                        nextList.Add(match);
                    }
                }

                if (nextList.Count > 0)
                {
                    BuildMap(nextNodeIndex, charIndex + 1, nodes, nextList);
                }
                else
                {
                    // A terminal node must have a Result
                    Debug.Assert(nodes[nextNodeIndex].Result.HasValue);
                }
            }
        }

        public struct CharNode
        {
            public CharNode(char c, short offset = 0, byte count = 0, TResult? result = null)
            {
                this.c = c;
                this.Offset = offset;
                this.Count = count;
                Result = result;
            }

            internal readonly char c;

            internal short Offset;

            internal byte Count;

            public TResult? Result;

            public override string ToString()
            {
                var cText = c == 0 ? ("(root)") : c.ToString();
                return Result != null
                    ? $"{nameof(c)}: {cText}, offset: {Offset}, count: {Count}, result: {Result}"
                    : $"{nameof(c)}: {cText}, offset: {Offset}, count: {Count}";
            }
        }
    }
}