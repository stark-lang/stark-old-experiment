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

// TODO: Usage of plural form is not always consistent: sometimes it is a * (0 or more) or a + (1 or more)
// TODO: Clarify where NEW_LINE / SEMI_COLON are relevant or should be ignored/skipped
// TODO: Add missing
//  - use module directive
//  - pattern matching
//  - if let / while let
//  - static members, inheritance and extensions
//  - overflow/checked/unchecked?
//  - transient semantic
//  - throws/try semantic
//  - OptionType notation with ?
//  - delegate lambda/closures
//  - scoped descructor/defer / single ownership?
//  - fixed array and raw arrays (non reference type)
//  - yield return / yield break ?
//  - annotations
//  - async / await
//  - custom new operator
//  - integrated tests
//  - pure functions / side-effect free
//  - constructor init semantic
//  - syntax for slices same or different than array? (& runtime implications)

parser grammar StarkParser;

options { tokenVocab=StarkLexer; }

// Language declarations
Declarations: Declaration*;

Declaration: Modules
           | OperatorDeclaration
           | Functions
           | Types
           | NEW_LINE+
           ;

// -------------------------------------------------------------------------
// Module
// -------------------------------------------------------------------------

Modules: ModuleDeclaration
       | ModuleDefinition
       ;

ModuleDefinition: Visibility? 'module' ModuleFullPath ModuleBody;

ModuleDeclaration: Visibility? 'module' ModuleFullPath Eod;

ModuleFullPath:  ModulePath? ModuleName;

ModulePath: COLON_COLON? (ModuleName COLON_COLON)+;

ModuleName: IDENTIFIER;

ModuleBody: OPEN_BRACE Declarations CLOSE_BRACE;

// -------------------------------------------------------------------------
// Type Reference
// -------------------------------------------------------------------------
Type: TypeModifier* TypePath STAR?;

TypeModifier: Permission
            | Transient;

TypePath: DelegateDefinition
        | ModulePath? TypePart;

TypePart: TypePart DOT IDENTIFIER TypeArguments?
        | TypeFinalPart;

TypeFinalPart: TypeFinalPart OPEN_BRACKET CLOSE_BRACKET
             | IDENTIFIER TypeArguments?;

TypeArguments: LESS Type (COMMA Type)* GREATER;


// -------------------------------------------------------------------------
// Template Parameters
// -------------------------------------------------------------------------
TemplateParameters: LESS TemplateParameter (COMMA TemplateParameter)* CLOSE_PAREN;

TemplateParameter: TemplateParameterName
                | TemplateParameterTyped
                | TemplateParameterHigherOrder
                ;

TemplateParameterName: IDENTIFIER;
TemplateParameterType: IDENTIFIER;

TemplateParameterHigherOrder: TemplateParameterName TemplateParameters+;
TemplateParameterTyped: TemplateParameterName COLON TemplateParameterType;

TemplateParameterTypeConstraints: ('where' TemplateParameterTypeConstraint)*;

TemplateParameterTypeConstraint: IDENTIFIER 'extends' TypePath
                               | IDENTIFIER 'implements' TypePath;

// -------------------------------------------------------------------------
// Modifiers
// -------------------------------------------------------------------------

Visibility: 'public'
          | 'internal' 
          | 'private'
          | 'protected'
          ;

Inherit: 'virtual'
       | 'abstract'
       | 'override'
       ;

Partial: 'partial';

Modifier: Visibility
        | Partial
        | Permission
        | Inherit
        | Permission
        | Unsafe
        ;

Access: Permission
      | Ownership
      ;

Permission: 'immutable'
          | 'readonly'
          ;

Unsafe: 'unsafe';

Transient: 'transient';

Ownership: 'isolated';

// -------------------------------------------------------------------------
// Contracts
//
// Used by functions
// 
// TODO: Do we really need more? 
// -------------------------------------------------------------------------

Contracts:  Contract*
         ;

Contract: Requires
        | Ensures;

Requires: 'requires' Expression;

Ensures: 'ensures' Expression;


// *************************************************************************
// -------------------------------------------------------------------------
// Function definitions
// -------------------------------------------------------------------------
// *************************************************************************

Functions: Function
         | Property
         | OperatorDefinition
         ;

// -------------------------------------------------------------------------
// Function
//
// They can be used in:
// - global functions
// - class/struct methods
// - class/struct static methods
// - trait methods
// -------------------------------------------------------------------------

VariableType: Access? Type;

Function: Modifier* IDENTIFIER TemplateParameters? OPEN_PAREN FunctionParameters? CLOSE_PAREN FunctionReturnType? Contracts? FunctionBody;

FunctionParameters: FunctionParameter (COMMA FunctionParameter)*;

FunctionParameter: 'mutable'? IDENTIFIER (COLON VariableType)?;

FunctionReturnType: '->'  VariableType; // TODO: Parsing MINUS GREATER


FunctionBody: StatementBlock
            | FunctionExpression Eod
            | Eod;

FunctionExpression: '=>' Expression; // TODO: Parsing EQUAL GREATER

// -------------------------------------------------------------------------
// Property 
//
// A property is a special function that provides a getter / setter
//
// They can be used in:
// - global properties
// - class/struct properties
// - class/struct static properties
// - trait properties
// -------------------------------------------------------------------------

Property: Modifier* IDENTIFIER COLON Type PropertyBody;

PropertyBody: OPEN_BRACE PropertyGetter? PropertySetter? CLOSE_BRACE 
            | FunctionExpression Eod;

PropertyGetter: 'get' Permission? Contracts? (StatementBlock | Eod);
PropertySetter: 'set' Permission? Contracts? (StatementBlock | Eod);


// -------------------------------------------------------------------------
// Operator
//
// Similar to functions using customized symbol operators.
//
// They can be used in:
// - global operators
// - class/struct operators
// - class/struct static operators
// - trait operators
// -------------------------------------------------------------------------
// NOTE: An operator definition starts exactly like a function, but the IDENTIFIER is `operator`
// The main difference after is for the parameters (that accept string/chars to define the operator)

// An OperatorDeclaration must happen before any usage of the Operator definition
// We don't strictly define how the operator strings are defined here
// This will be parsed and validated by the handwritten parser
OperatorDeclaration: Visibility? 'operator' (CHAR|STRING|STRING_RAW|UNDERSCORES)+ OperatorDescription;

OperatorDescription: OPEN_BRACE OperatorHint* CLOSE_BRACE;

OperatorHint: 'precedence' COLON INTEGER Eod
            | 'associativity' COLON ('right' | 'left') Eod
            | 'builtin' COLON ExpressionLiteralBool Eod
            | 'overridable' COLON ExpressionLiteralBool Eod
            | 'assignment' COLON ExpressionLiteralBool Eod
            | 'id' COLON STRING Eod
            ;

OperatorDefinition: Modifier* 'operator' TemplateParameters? OPEN_PAREN OperatorParameters CLOSE_PAREN FunctionReturnType? Contracts? FunctionBody;


OperatorParameters: OperatorParametersMember
                  | OperatorParametersUnary
                  | OperatorParametersBinary
                  | OperatorParametersIndexer
                  ;

OperatorParametersMember: (CHAR|STRING|STRING_RAW);
OperatorParametersIndexer: (CHAR|STRING|STRING_RAW) FunctionParameter (COMMA FunctionParameter)* (CHAR|STRING|STRING_RAW);

OperatorParametersUnary: (CHAR|STRING|STRING_RAW) FunctionParameter;

OperatorParametersBinary: FunctionParameter (CHAR|STRING|STRING_RAW) FunctionParameter;

// -------------------------------------------------------------------------
// Delegate
// -------------------------------------------------------------------------

DelegateDefinition: 'delegate' TypeConstructor FunctionReturnType?; 

Delegate: Visibility? 'delegate' IDENTIFIER TypeConstructor FunctionReturnType? Eod; 

// *************************************************************************
// -------------------------------------------------------------------------
// Type
// -------------------------------------------------------------------------
// *************************************************************************

Types: Class 
     | Trait
     | Enum
     | Extension
     | Delegate
     ;

TypeConstructor: OPEN_PAREN FunctionParameters CLOSE_PAREN;

// -------------------------------------------------------------------------
// Struct/Class
// -------------------------------------------------------------------------

Class: Modifier* ('struct'|'class') ClassIdentifier TypeConstructor? Extends? Implements? TemplateParameterTypeConstraints? ClassBody;

Extends: 'extends' TypePath;

Implements: 'implements' TypePath (COMMA TypePath )+;

ClassIdentifier: IDENTIFIER TemplateParameters?;

ClassBody: OPEN_BRACE ClassMember* CLOSE_BRACE;

ClassMember: ClassField
           | Functions
           ;

ClassField: Visibility? IDENTIFIER COLON Type Eod;

// -------------------------------------------------------------------------
// Trait
// -------------------------------------------------------------------------

Trait: Modifier* 'trait' TraitIdentifier TypeConstructor? Extends? TemplateParameterTypeConstraints? TraitBody;

TraitIdentifier: IDENTIFIER TemplateParameters?;

TraitBody: OPEN_BRACE TraitMember* CLOSE_BRACE;

TraitMember: Functions
           ;
// -------------------------------------------------------------------------
// Enum
// -------------------------------------------------------------------------

Enum: Modifier* 'enum' EnumIdentifier (COLON IDENTIFIER)? TemplateParameterTypeConstraints? EnumBody;

EnumIdentifier: IDENTIFIER TemplateParameters?;

EnumBody: OPEN_BRACE EnumMembers? CLOSE_BRACE;

EnumMembers: EnumMember (COMMA EnumMember)* COMMA?;

EnumMember: IDENTIFIER
          | IDENTIFIER TypeConstructor
          | IDENTIFIER EQUAL Expression;

// -------------------------------------------------------------------------
// Extension
// -------------------------------------------------------------------------
Extension: Visibility? Extends Implements? TemplateParameterTypeConstraints? ExtensionBody;

ExtensionBody: OPEN_BRACE ExtensionMember* CLOSE_BRACE;

ExtensionMember: Functions
               ;

// *************************************************************************
// -------------------------------------------------------------------------
// Statements
// -------------------------------------------------------------------------
// *************************************************************************

// End of statement
Eod: NEW_LINE | SEMI_COLON;


Statement: StatementFor
         | StatementLoop
         | StatementLet
         | StatementIf
         | StatementBreak
         | StatementContinue
         | StatementReturn
         | StatementBlock
         | StatementAssign
         | StatementUnsafe
         | StatementExpression
         | StatementEmpty
         ;

StatementFor: LoopLabel? 'for' ForVariable 'in' Expression StatementBlock;

LoopLabel: IDENTIFIER COLON;

ForVariable: IDENTIFIER
           | OPEN_PAREN IDENTIFIER COMMA IDENTIFIER CLOSE_PAREN;

StatementLoop: LoopLabel? 'loop' StatementBlock;

StatementBreak: 'break' IDENTIFIER? Eod;

StatementContinue: 'continue' IDENTIFIER? Eod;

StatementReturn: 'return' Expression? Eod;

StatementExpression: Expression Eod;

StatementIf: 'if' Statement StatementBlock StatementElseIf* StatementElse*;

StatementElseIf: 'else' 'if' Statement StatementBlock;

StatementElse: 'else' StatementBlock;


StatementLet: 'let' 'mutable'? IDENTIFIER (COLON VariableType)? (EQUAL Expression) Eod;

StatementUnsafe: 'unsafe' StatementBlock;


StatementBlock: OPEN_BRACE Statement* CLOSE_BRACE;

// All assign expressions are actually not allowed in expressions but only
// from a statement. Yet custom expression operators can define assign
// operators
StatementAssign:  Expression '=' Expression Eod
               ;

StatementEmpty : Eod;                        

// *************************************************************************
// -------------------------------------------------------------------------
// Expressions
// -------------------------------------------------------------------------
// *************************************************************************

// NOTE: Expression are partially defined here, as most of the expressions
// will be defined through operator declarations to allow custom and builtin
// operators to be added to the parsing without changing the grammar

// Expressions are defined first by two builtins expression: 
// - literals
// - full identifiers: module_prefix (using :: separator) + IDENTIFIER + template_parameters (embraced by '<' '>')

// The handwritten parsing of the template parameters '<' '>' requires special treatment in order to 
// separate it from regular compare operators like '<' or '>'
// If the parsing of a template doesn't succeed (for any reasons like non expecting characters inside the < >)
// the parser will have to rollback to the initial '<' and let parsing operators occuring

Expression: ExpressionIdentifier
          | ExpressionLiteral          
          ;
// The content of the Expression are dynamically created with operators declarations
// Usually, expressions are followed and defined statically, e.g like this:

        //   | Expression DOT Expression                          // #ExpressionMember
        //   | Expression MINUS_GREATER Expression                         // #ExpressionMemberPointer
        //   | OPEN_PAREN Expression (COMMA Expression)* CLOSE_PAREN               // #ExpressionTuple
        //   | Expression OPEN_BRACKET Expression (COMMA Expression)* CLOSE_BRACKET    // #ExpressionIndexer
        //   | Expression OPEN_PAREN Expression (COMMA Expression)* CLOSE_PAREN    // #ExpressionInvoke
        //   | 'typeof' OPEN_PAREN Expression CLOSE_PAREN
        //   | ('throw'|'new'|'ref'|'out') Expression             // #ExpressionUnaryAction
        //   | AND Expression                                     // #ExpressionAddressOf
        //   | (TILDE|NOT|PLUS|MINUS) Expression                       // #ExpressionUnaryOperator
        //   | Expression (STAR|DIVIDE|MODULUS) Expression                // #ExpressionBinary
        //   | Expression (PLUS|MINUS) Expression                    // #ExpressionBinary
        //   | Expression (LESS_LESS | GREATER_GREATER) Expression                // #ExpressionBinary
        //   | Expression ('as' | 'is' | 'as?') TypePath          // #ExpressionAsIs
        //   | Expression (LESS_EQUAL | GREATER_EQUAL | LESS | GREATER) Expression    // #ExpressionBinary
        //   | Expression (EQUAL_EQUAL | NOT_EQUAL) Expression                // #ExpressionBinary
        //   | Expression AND Expression                          // #ExpressionBinary
        //   | Expression EXPONENT Expression                          // #ExpressionBinary
        //   | Expression PIPE Expression                          // #ExpressionBinary
        //   | Expression AND_AND Expression                         // #ExpressionBinary
        //   | Expression PIPE_PIPE Expression                         // #ExpressionBinary
        //   | Expression QUESTION Expression COLON Expression           // #ExpressionIf

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
