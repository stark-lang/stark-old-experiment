// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

// *************************************************************************
// This is the ANTLR tokens for Stark
//
// This is a work in progress.
//
// This lexer grammar is not used for parsing, as we are using a handwritten 
// tokenizer. But the tests are checking the tokens against this file
// in order to verify that the handwritten tokenizer is correct.
// *************************************************************************

lexer grammar StarkLexer;

// -------------------------------------------------------------------------
// Tokens
// -------------------------------------------------------------------------

import XID_Start_Continue;

// Spaces and Newlines
SPACES: Whitespace+; // Note we separate NL from WHITESPACES

NEW_LINE: '\r\n' | '\r' | '\n'
	| '\u0085' // <Next Line CHARACTER (U+0085)>'
	| '\u2028' //'<Line Separator CHARACTER (U+2028)>'
	| '\u2029' //'<Paragraph Separator CHARACTER (U+2029)>'
	;

// Comments
COMMENT : '//' ( ~[/\r\n] ~[\r\n]* )?;
COMMENT_DOC : '///' ~[\r\n]*;
COMMENT_MULTI_LINE : '/*' (COMMENT_MULTI_LINE | .)*? '*/';

// Identifier
// Note that for XID_Start, XID_Continue they are not strictly equivalent
// to the manual parser, as ANTLR doesn't support UTF-32
// so UTF-32 chars are currently removed from the set 
// See issue: https://github.com/antlr/antlr4/issues/276
IDENTIFIER: XID_Start XID_Continue*;

UNDERSCORES: '_'+;

// Integer Literals
INTEGER:     [0-9] [0-9_]*;
INTEGER_HEXA: '0' [xX] '_'* [0-9a-fA-F] [0-9a-fA-F_]*;
INTEGER_OCTAL: '0' [oO] '_'* [0-7] [0-7_]*;
INTEGER_BINARY: '0' [bB] '_'* [0-1] [0-1_]*;

// Float Literals
FLOAT: [0-9][0-9_]* ( ([eE] [-+]? [0-9][0-9_]*) | '.' [0-9][0-9_]* ([eE] [-+]? [0-9][0-9_]*)?);

// Character and Strings
CHAR:       '\'' (~['\\\r\n\u0085\u2028\u2029] | CommonCharacter) '\'';
STRING_RAW: '@"' (~'"' | '""')* '"';
STRING:     '"'  (~["\\\r\n\u0085\u2028\u2029] | CommonCharacter)* '"';

TILDE: '~';
SEMI_COLON: ';';
EXPONENT: '^';
COLON: ':';
COLON_COLON: '::';
EQUAL: '=';
PIPE: '|';
NOT: '!';
AND: '&';
QUESTION: '?';
LESS: '<';
GREATER:'>';
DIVIDE: '/';
BACKSLASH: '\\';
STAR: '*';
PLUS: '+';
MINUS: '-';
MODULUS: '%';
COMMA: ',';
DOT: '.';
OPEN_PAREN: '(';
CLOSE_PAREN: ')';
OPEN_BRACE: '{';
CLOSE_BRACE: '}';
OPEN_BRACKET: '[';
CLOSE_BRACKET: ']';

// Space Fragments
fragment Whitespace
	: UnicodeClassZS //'<Any Character With Unicode Class Zs>'
	| '\u0009' //'<Horizontal Tab Character (U+0009)>'
	| '\u000B' //'<Vertical Tab Character (U+000B)>'
	| '\u000C' //'<Form Feed Character (U+000C)>'
	;

// http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5B%3AZS%3A%5D&g=&i=
fragment UnicodeClassZS
	: '\u0020' // SPACE
	| '\u00A0' // NO_BREAK SPACE
	| '\u1680' // OGHAM SPACE MARK
	| '\u2000' // EN QUAD
	| '\u2001' // EM QUAD
	| '\u2002' // EN SPACE
	| '\u2003' // EM SPACE
	| '\u2004' // THREE_PER_EM SPACE
	| '\u2005' // FOUR_PER_EM SPACE
	| '\u2006' // SIX_PER_EM SPACE
	| '\u2007' // FIGURE SPACE
	| '\u2008' // PUNCTUATION SPACE
	| '\u2009' // THIN SPACE
	| '\u200A' // HAIR SPACE
	| '\u202F' // NARROW NO_BREAK SPACE
	| '\u205F' // MEDIUM MATHEMATICAL SPACE
	| '\u3000' // IDEOGRAPHIC SPACE
	;

fragment CommonCharacter
	: SimpleEscapeSequence
	| HexEscapeSequence
	| UnicodeEscapeSequence
	;

fragment SimpleEscapeSequence
	: '\\\''
	| '\\"'
	| '\\\\'
	| '\\0'
	| '\\a'
	| '\\b'
	| '\\f'
	| '\\n'
	| '\\r'
	| '\\t'
	| '\\v'
	;

fragment HexEscapeSequence
	: '\\x' [0-9a-fA-F]
	| '\\x' [0-9a-fA-F][0-9a-fA-F]
	| '\\x' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
	| '\\x' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
	;

fragment UnicodeEscapeSequence
	: '\\u' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
	| '\\U' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
	;	
