// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System;
using Stark.Compiler.Text;

namespace Stark.Compiler.Syntax
{
    /// <summary>
    /// A lightweight token struct to avoid GC allocations.
    /// </summary>
    public struct SyntaxToken : IEquatable<SyntaxToken>
    {
        public static readonly SyntaxToken Eof = new SyntaxToken(TokenType.Eof, TextPosition.Eof, TextPosition.Eof);

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxToken"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public SyntaxToken(TokenType type, TextPosition start, TextPosition end)
        {
            if (start.Offset > end.Offset) throw new ArgumentOutOfRangeException(nameof(start), $"[{nameof(start)}] index must be <= to [{nameof(end)}]");
            Type = type;
            Start = start;
            End = end;
        }

        /// <summary>
        /// The type of token.
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The start position of this token.
        /// </summary>
        public readonly TextPosition Start;

        /// <summary>
        /// The end position of this token.
        /// </summary>
        public readonly TextPosition End;

        public override string ToString()
        {
            return $"{Type}({Start}:{End})";
        }


        public string GetText(string text)
        {
            if (Type == TokenType.Eof)
            {
                return "<eof>";
            }
            return End.Offset < text.Length ? text.Substring(Start.Offset, End.Offset - Start.Offset + 1) : null;
        }

        public string GetText<TTextView>(TTextView text) where TTextView : IStringView
        {
            if (Type == TokenType.Eof)
            {
                return "<eof>";
            }
            return text.GetString(Start.Offset, End.Offset - Start.Offset + 1);
        }

        public bool Equals(SyntaxToken other)
        {
            return Type == other.Type && Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SyntaxToken && Equals((SyntaxToken) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SyntaxToken left, SyntaxToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SyntaxToken left, SyntaxToken right)
        {
            return !left.Equals(right);
        }
    }
}