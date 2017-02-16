```antlr
lexer grammar StarkLexer;
import XID_Start_Continue; // import the definition of XID_Start 
                           // and XID_Continue characters
```
# Tokens Specification

This document describes the tokens used for parsing the Stark Programming Language.

The syntax used to define the tokens is [ANTLR 4](http://www.antlr.org/)

The ANTLR syntax stored in this document is automatically extracted from this file to generate an ANTLR `g4` file and tested against the manual tokenizer developed to parse the language.

> Note that the tokens don't describe how the language is going to use or store them (e.g a string token might be stored/used in the context of a `UTF-8` or `UTF-16` string, a float literal might be used as a `float32` or a `float64`)

## Whitespaces and new line

We separate here new lines from whitespaces, as new lines have a special meaning in Stark, to enable ending a statement/declaration automatically.

See [Appendix: Fragments](#appendix-fragments) for the details about the `Whitespace` fragment.

```antlr
WHITESPACES: Whitespace+;

NEW_LINE
        : '\r\n' | '\r' | '\n'
        | '\u0085' // <Next Line CHARACTER (U+0085)>'
        | '\u2028' //'<Line Separator CHARACTER (U+2028)>'
        | '\u2029' //'<Paragraph Separator CHARACTER (U+2029)>'
        ;
```

## Comments

We allow single line comment:

```antlr
COMMENT:            '//' ( ~[/\r\n] ~[\r\n]* )?;
``` 

Comment used for documenting the following language element:

```antlr
COMMENT_DOC:        '///' ~[\r\n]*;
``` 

Multi-line comments with support for nested multi-line comments:

```antlr
COMMENT_MULTI_LINE: '/*' (COMMENT_MULTI_LINE | .)*? '*/';
``` 

> We may introduced another type of comment doc for documenting the parent language element.

## Keywords

A temporary list of keywords or contextual keywords (WIP)

```antlr
ABSTRACT: 'abstract';
ALIAS: 'alias';
AS: 'as';
ASYNC: 'async';
AWAIT: 'await';
BASE: 'base';
BREAK: 'break';
CLASS: 'class';
CONST: 'const';
CONSTRUCTOR: 'constructor';
DEFAULT: 'default';
ELSE: 'else';
ENUM: 'enum';
EXTENDS: 'extends';
EXTERN: 'extern';
FALSE: 'false';
FATAL: 'fatal';
FIXED: 'fixed';
FOR: 'for';
FROM: 'from';
FUNC: 'func';
GET: 'get';
IF: 'if';
IMMUTABLE: 'immutable';
IMPLEMENTS: 'implements';
IMPORT: 'import';
IN: 'in';
INTERNAL: 'internal';
IS: 'is';
ISOLATED: 'isolated';
LET: 'let';
MATCH: 'match';
META: 'meta';
MODULE: 'module';
MUTABLE: 'mutable';
NEW: 'new';
OPERATOR: 'operator';
OUT: 'out';
OVERRIDE: 'override';
PACKAGE: 'package';
PARAMS: 'params';
PARTIAL: 'partial';
PRIVATE: 'private';
PROTECTED: 'protected';
PUBLIC: 'public';
READONLY: 'readonly';
REF: 'ref';
REQUIRES: 'requires';
RETURN: 'return';
SCOPED: 'scoped';
SEALED: 'sealed';
SET: 'set';
SIZEOF: 'sizeof';
STATIC: 'static';
STRUCT: 'struct';
THIS: 'this';
THROW: 'throw';
TRAIT: 'trait';
TRANSIENT: 'transient';
TRUE: 'true';
TYPEOF: 'typeof';
UNSAFE: 'unsafe';
VALUE: 'value';
VIRTUAL: 'virtual';
VOLATILE: 'volatile';
WHERE: 'where';
WHILE: 'while';
WITH: 'with';
```

## Identifiers

An identifier is defined by using the ["Unicode Identifier and Pattern Syntax" (Unicode® Standard Annex #31)](http://www.unicode.org/reports/tr31/)

It is composed of characters `XID_Start` and `XID_Continue` that can be extracted from the [Unicode DerivedCoreProperties database](http://unicode.org/Public/UNIDATA/DerivedCoreProperties.txt).

The main difference here is that we allow the character underscore `_` to be used as a `XID_Start`

> Note that because of a restriction of ANTLR not supporting correctly `UTF-32`, the extracted `XID_Start` and `XID_Continue` does not contain `UTF-32` code points above `0xFFFF` (while the manual parser does)

```antlr
IDENTIFIER: ('_'|XID_Start) XID_Continue*;
```

Note that we are introducing also a special identifier to match an identifier composed only of the character underscore `_`

```antlr
UNDERSCORES: '_'+;
```

## Integer Literals

Integer digits `[0-9]` can be separated by an underscore `_` but they must contain at least one digit.


```antlr
INTEGER:        [0-9] [0-9_]*;
```

There are also dedicated tokens for other integer declarations:

- hexadecimal (e.g `0x1234FF00`)
- octal  (e.g `0o117`)
- binary (e.g `0b11110101`)

```antlr
INTEGER_HEXA:   '0' [xX] '_'* [0-9a-fA-F] [0-9a-fA-F_]*;
INTEGER_OCTAL:  '0' [oO] '_'* [0-7] [0-7_]*;
INTEGER_BINARY: '0' [bB] '_'* [0-1] [0-1_]*;
```

All digits can be separated by underscore `_`

## Float Literals

A floating-point number literal requires a digit followed at least by either:

- a `.` followed by one or more digits
- an exponent `e` followed by an optional `+|-` and one or more digits

All digits can be separated by underscore `_`

```antlr
FLOAT: [0-9][0-9_]* ( ([eE] [-+]? [0-9][0-9_]*) | '.' [0-9][0-9_]* ([eE] [-+]? [0-9][0-9_]*)?);
```

## Characters

A character is enclosed by a single quote `'`

```antlr
CHAR:       '\'' (~['\\\r\n\u0085\u2028\u2029] | CommonCharacter) '\'';

fragment CommonCharacter
    : SimpleEscapeSequence
    | HexEscapeSequence
    | UnicodeEscapeSequence
    ;
```

The character `\` is used for escaping the following control characters:

```antlr
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
```

The escape `\` character is also used to input special characters:

- Hexadecimal character (e.g `\xFF`)
- Unicode UTF-16 character (e.g `\u12AC`)
- Unicode UTF-32 character (e.g `\U00012345`)

```antlr
fragment HexEscapeSequence
    : '\\x' [0-9a-fA-F]
    | '\\x' [0-9a-fA-F][0-9a-fA-F]
    | '\\x' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
    | '\\x' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
    ;

fragment UnicodeEscapeSequence
    : '\\u' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
    | '\\U' [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
            [0-9a-fA-F][0-9a-fA-F][0-9a-fA-F][0-9a-fA-F]
    ;   
```

## Strings


```antlr
STRING_RAW: '@"' (~'"' | '""')* '"';
STRING:     '"'  (~["\\\r\n\u0085\u2028\u2029] | CommonCharacter)* '"';
```

## Symbols

The symbols are used by the grammar in different scenarios (e.g separating an identifier from its type or in expression with operators)

We do not define composed tokens here (e.g `>>` instead we have two consecutive `>` `>`) to allow the language parser to handle general and custom operators in an uniform manner.

We also don't attach a particular semantic to the symbols, as a symbol might be used in different scenarios (e.g `*` for operator multiplication but also for pointer declaration and dereferencing)

```antlr
EXCLAMATION: '!';
NUMBER: '#';
DOLLAR: '$';
PERCENT: '%';
AMPERSAND: '&';


OPEN_PARENTHESIS: '(';
CLOSE_PARENTHESIS: ')';

ASTERISK: '*';
PLUS: '+';
COMMA: ',';
MINUS: '-';
DOT: '.';
SLASH: '/';
COLON: ':';
SEMI_COLON: ';';
LESS_THAN: '<';
EQUAL: '=';
GREATER_THAN:'>';
QUESTION: '?';
AT: '@';

OPEN_BRACKET: '[';
BACKSLASH: '\\';
CLOSE_BRACKET: ']';

CARET: '^';
GRAVE_ACCENT: '`';

OPEN_BRACE: '{';
PIPE: '|';
CLOSE_BRACE: '}';

TILDE: '~';
```

## Appendix: Fragments

```antlr
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
```
