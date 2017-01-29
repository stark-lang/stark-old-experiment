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

        Integer,
        IntegerHexa,
        IntegerOctal,
        IntegerBinary,

        Float,

        Char,

        String,

        Underscores, // at least one _

        [TokenText("`")]
        GraveAccent,

        [TokenText("~")]
        Tilde,

        [TokenText(";")]
        SemiColon,

        [TokenText("$")]
        Dollar,

        [TokenText("@")]
        At,

        [TokenText("#")]
        Hash,

        [TokenText("^")]
        Exponent,

        [TokenText(":")]
        Colon,

        [TokenText("::")]
        ColonColon,

        [TokenText("=")]
        Equal,

        [TokenText("|")]
        Pipe,

        [TokenText("!")]
        Not, 

        [TokenText("&")]
        And,

        [TokenText("&&")]
        AndAnd,

        [TokenText("||")]
        PipePipe,

        [TokenText("?")]
        Question,

        [TokenText("??")]
        QuestionQuestion,

        [TokenText("==")]
        EqualEqual,

        [TokenText("!=")]
        NotEqual,

        [TokenText("<")]
        Less,

        [TokenText("<<")]
        LessLess,

        [TokenText(">")]
        Greater,

        [TokenText(">>")]
        GreaterGreater,

        [TokenText("<=")]
        LessEqual,

        [TokenText(">=")]
        GreaterEqual,

        [TokenText("=>")]
        EqualGreater,

        [TokenText("/")]
        Divide,

        [TokenText("\\")]
        Backslash,

        [TokenText("*")]
        Star,

        [TokenText("+")]
        Plus,

        [TokenText("++")]
        PlusPlus,

        [TokenText("-")]
        Minus,

        [TokenText("->")]
        MinusGreater,

        [TokenText("--")]
        MinusMinus,

        [TokenText("%")]
        Modulus,

        [TokenText(",")]
        Comma,

        [TokenText(".")]
        Dot,

        [TokenText("..")]
        DotDot,

        [TokenText("...")]
        DotDotDot,

        [TokenText("..<")]
        DotDotLess,

        [TokenText("(")]
        OpenParent,
        [TokenText(")")]
        CloseParent,

        [TokenText("{")]
        OpenBrace,
        [TokenText("}")]
        CloseBrace,

        [TokenText("[")]
        OpenBracket,
        [TokenText("]")]
        CloseBracket,

        Eof,
    }
}