// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

// *************************************************************************
// This is the ANTLR grammar for Stark
//
// NOTE: This is a work in progress.
//
// This grammar is not used for parsing, as we are using a handwritten 
// recursive descent parser, but should help to describe the overall
// syntax and spot early in the process potential parsing issues.
//
// The grammar doesn't reflect exactly how whitespaces are handled by the 
// parser (indicated as a comment in the grammar if there is anything special)
// *************************************************************************

parser grammar StarkParser;

options { tokenVocab=StarkLexer; }

// Top level grammar declarations
Declaration: ModuleDeclaration
           | TypeDeclaration
           | BlockDeclaration
           ;

// -------------------------------------------------------------------------
// Module
// -------------------------------------------------------------------------

// Defines Modules and ModuleDeclaration
ModuleName: IDENTIFIER;
ModulePath: COLON_COLON? (ModuleName COLON_COLON)+;

ModuleFullPath:  ModulePath? ModuleName;

ModuleDeclaration: 'module' ModuleFullPath Eos;

// BlockDeclaration

BlockDeclaration: OPEN_BRACE Declaration* CLOSE_BRACE;

// -------------------------------------------------------------------------
// Statements
// -------------------------------------------------------------------------

Statement: StatementFor
         | StatementLoop
         | StatementLet
         | StatementIf
         | StatementBreak
         | StatementContinue
         | StatementReturn
         | StatementBlock
         | StatementAssign
         | StatementExpression
         | StatementEmpty
         ;

StatementFor: LoopLabel? 'for' ForVariable 'in' Expression StatementBlock;

LoopLabel: IDENTIFIER COLON;

ForVariable: IDENTIFIER
           | OPEN_PAREN IDENTIFIER COMMA IDENTIFIER CLOSE_PAREN;

StatementLoop: LoopLabel? 'loop' StatementBlock;

StatementBreak: 'break' IDENTIFIER? Eos;

StatementContinue: 'continue' IDENTIFIER? Eos;

StatementReturn: 'return' Expression? Eos;

StatementExpression: Expression Eos;

StatementIf: 'if' Statement StatementBlock StatementElseIf* StatementElse*;

StatementElseIf: 'else' 'if' Statement StatementBlock;

StatementElse: 'else' StatementBlock;


StatementLet: 'let' 'mutable'? IDENTIFIER (COLON VariableType)? (EQUAL Expression) Eos;


StatementBlock: OPEN_BRACE Statement* CLOSE_BRACE;

StatementAssign: Expression StatementAssignOperators Expression Eos
               | Expression (PLUS_PLUS | MINUS_MINUS) Eos
               ;

// A variable path
ExpressionLValue: ModulePath? VariableTypePart;

VariableTypePart: VariableTypePart DOT IDENTIFIER TypeArguments?
                | VariableTypePartFinalPart;

VariableTypePartFinalPart: VariableTypePartFinalPart OPEN_BRACKET CLOSE_BRACKET
                         | IDENTIFIER TypeArguments?;

// x += 1 equivalent to x++
// x -= 1 equivalent to x--
StatementAssignOperators: EQUAL
                        | PLUS_EQUAL
                        | MINUS_EQUAL
                        | STAR_EQUAL 
                        | MODULUS_EQUAL
                        | DIVIDE_EQUAL 
                        | LESS_LESS_EQUAL 
                        | GREATER_GREATER_EQUAL 
                        | PIPE_EQUAL
                        | AND_EQUAL
                        | EXPONENT_EQUAL
                        | PIPE_PIPE_EQUAL
                        | AND_AND_EQUAL
                        ;

StatementEmpty : Eos;                        

// -------------------------------------------------------------------------
// Expressions
// -------------------------------------------------------------------------

// From higher to lower precedence
Expression: ExpressionIdentifier
          | ExpressionLiteral          
          | Expression DOT Expression                          // #ExpressionMember
          | Expression MINUS_GREATER Expression                         // #ExpressionMemberPointer
          | OPEN_PAREN Expression (COMMA Expression)* CLOSE_PAREN               // #ExpressionTuple
          | Expression OPEN_BRACKET Expression (COMMA Expression)* CLOSE_BRACKET    // #ExpressionIndexer
          | Expression OPEN_PAREN Expression (COMMA Expression)* CLOSE_PAREN    // #ExpressionInvoke
          | 'typeof' OPEN_PAREN Expression CLOSE_PAREN
          | ('throw'|'new'|'ref'|'out') Expression             // #ExpressionUnaryAction
          | AND Expression                                     // #ExpressionAddressOf
          | (TILDE|NOT|PLUS|MINUS) Expression                       // #ExpressionUnaryOperator
          | Expression (STAR|DIVIDE|MODULUS) Expression                // #ExpressionBinary
          | Expression (PLUS|MINUS) Expression                    // #ExpressionBinary
          | Expression (LESS_LESS | GREATER_GREATER) Expression                // #ExpressionBinary
          | Expression ('as' | 'is' | 'as?') TypePath          // #ExpressionAsIs
          | Expression (LESS_EQUAL | GREATER_EQUAL | LESS | GREATER) Expression    // #ExpressionBinary
          | Expression (EQUAL_EQUAL | NOT_EQUAL) Expression                // #ExpressionBinary
          | Expression AND Expression                          // #ExpressionBinary
          | Expression EXPONENT Expression                          // #ExpressionBinary
          | Expression PIPE Expression                          // #ExpressionBinary
          | Expression AND_AND Expression                         // #ExpressionBinary
          | Expression PIPE_PIPE Expression                         // #ExpressionBinary
          | Expression QUESTION Expression COLON Expression           // #ExpressionIf
          ;

// Expression identifier (either a full type path with template arguments or a simple identifier)
ExpressionIdentifier: ModulePath? ExpressionIdentifierPath;

ExpressionTemplateArgument: ModulePath? ExpressionIdentifierSubPath STAR 
                          | ExpressionLiteralSimple
                          ;

ExpressionIdentifierPath: IDENTIFIER ExpressionTemplateArguments?;

ExpressionIdentifierSubPath: ExpressionIdentifierPath DOT ExpressionSimpleType
                           | ExpressionSimpleType;

ExpressionSimpleType: IDENTIFIER ExpressionTemplateArguments?;

ExpressionTemplateArguments:  LESS ExpressionTemplateArgument (COMMA ExpressionTemplateArgument)* GREATER;


// Literal Expressions
LiteralTypeSuffix: ExpressionIdentifier;

// In the custom parser, we don't expect any whitespace between the literal and its suffix
ExpressionLiteral: ExpressionLiteralSimple LiteralTypeSuffix?;


ExpressionLiteralSimple: ExpressionLiteralBool
                       | INTEGER
                       | INTEGER_HEXA
                       | INTEGER_OCTAL
                       | INTEGER_BINARY
                       | FLOAT
                       | STRING_RAW
                       | STRING
                       | CHAR
                       ;

ExpressionLiteralBool: 'true' | 'false';

// -------------------------------------------------------------------------
// Type Declaration (class, struct)
// -------------------------------------------------------------------------

TypeDeclaration: TypeStructClassDeclaration;

TypeStructClassDeclaration: TypeVisibility? TypePermission? TypeInheritability? ('struct'|'class') TypeDeclarationIdentifer TypeConstructor? (COLON TypePath (COMMA TypePath )+ )? ('where' TypeTemplateConstraint)+ Eos;


TypeVisibility: 'public'
              | 'internal' 
              ;

TypePermission: 'immutable'
              | 'readonly'
              ;

TypeOwnership: 'isolated';


TypeInheritability: 'virtual'
                  | 'abstract'
                  ;

TypeDeclarationIdentifer: IDENTIFIER TypeTemplateParameters?;


Constructor: OPEN_PAREN ParameterList CLOSE_PAREN;

ParameterList: IDENTIFIER COLON;



TypeTemplateParameters: LESS TypeTemplateParameter (COMMA TypeTemplateParameter)* CLOSE_PAREN;
TypeTemplateParameter: TypeTemplateParameterName
                | TypeTemplateParameterTyped
                | TypeTemplateParameterHigherOrder
                ;

TypeTemplateParameterName: IDENTIFIER;
TypeTemplateParameterType: IDENTIFIER;

TypeTemplateParameterHigherOrder: TypeTemplateParameterName TypeTemplateParameters+;
TypeTemplateParameterTyped: TypeTemplateParameterName COLON TypeTemplateParameterType;



TypeConstructor: OPEN_PAREN TypeConstructorParameters CLOSE_PAREN;

TypeConstructorParameters: TypeConstructorParameter (COMMA TypeConstructorParameters)*;

TypeConstructorParameter: 'mutable'? IDENTIFIER COMMA VariableType;

VariableType: TypeOwnership? Type;

// A Type Declaration
Type: TypePermission? TypePath STAR?;

TypePath: ModulePath? TypePart;

TypePart: TypePart DOT IDENTIFIER TypeArguments?
        | TypeFinalPart;

TypeFinalPart: TypeFinalPart OPEN_BRACKET CLOSE_BRACKET
             | IDENTIFIER TypeArguments?;

TypeArguments: LESS Type (COMMA Type)* GREATER;


TypeTemplateConstraint: IDENTIFIER 'is' Type;

Eos: NEW_LINE | SEMI_COLON;