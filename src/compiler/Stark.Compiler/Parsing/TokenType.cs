// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
namespace Stark.Compiler.Parsing
{
    /// <summary>
    /// An enumeration to categorize tokens.
    /// </summary>
    public enum TokenType
    {
        Invalid,

        Spaces,

        NewLine,

        Comment,

        CommentDoc,

        CommentMultiLine,

        Identifier,

        Underscores, // at least one _

        Integer,
        IntegerHexa,
        IntegerOctal,
        IntegerBinary,

        Float,

        Char,

        String,

        StringRaw,

        [TokenText("!")]
        Exclamation,

        [TokenText("#")]
        Number,

        [TokenText("$")]
        Dollar,

        [TokenText("%")]
        Percent,

        [TokenText("&")]
        Ampersand,

        [TokenText("(")]
        OpenParenthesis,

        [TokenText(")")]
        CloseParenthesis,



        [TokenText("*")]
        Asterisk,

        [TokenText("+")]
        Plus,

        [TokenText(",")]
        Comma,

        [TokenText("-")]
        Minus,

        [TokenText(".")]
        Dot,

        [TokenText("/")]
        Slash,

        [TokenText(":")]
        Colon,

        [TokenText(";")]
        SemiColon,

        [TokenText("<")]
        LessThan,

        [TokenText("=")]
        Equal,

        [TokenText(">")]
        GreaterThan,

        [TokenText("?")]
        Question,

        [TokenText("@")]
        At,

        [TokenText("[")]
        OpenBracket,
        [TokenText("\\")]
        Backslash,
        [TokenText("]")]
        CloseBracket,

        [TokenText("^")]
        Caret,
        [TokenText("`")]
        GraveAccent,

        [TokenText("{")]
        OpenBrace,
        [TokenText("|")]
        Pipe,
        [TokenText("}")]
        CloseBrace,

        [TokenText("~")]
        Tilde,

        Eof,
    }
}