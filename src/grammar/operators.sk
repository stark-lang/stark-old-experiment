// In Stark, I expect to be able to describe all operators from within the language itself
// and not from a static grammar so that custom operators and regular operators are integrated
// the same way in the parser.
//
// This file contains the declaration of all the operators used by Stark
//
// These declarations are used by the parser to correctly parse expressions with correct precedence
// Because new operators can be added, expressions are not hard-coded into the grammar
// but are mainly define through operators declaration (and actual definitions)
//
// Still, when an operator requires special handling from the compiler, it is marked as builtin
// A builtin operator cannot be overrider

// Example from Swift: Complete Euler operators
// https://github.com/mattt/Euler/blob/master/Euler.swift

// C/C++ Operator Precedence: https://en.wikipedia.org/wiki/Operators_in_C_and_C%2B%2B#Operator_precedence

// The precedence value defined here is from highest to lowest

// Precedence 190
// ==============

// The :: operator is not defined through the operator system
// The same applies to template parameters "<" _ ">" that would
// conflict with comparison operators "<" or ">" alone, so we 
// need to handle them before any operators here.

// // Module root
// operator "::" _
// {
//     id: "ModuleRoot"
//     builtin: true
//     precedence: 190
//     associativity: right
// }
// 
// // Module path
// operator _ "::" _
// {
//     id: "ModulePath"
//     builtin: true
//     precedence: 190
//     associativity: left
// }
// 
// Precedence 180
// ==============

// The compiler will translate it to _ = _ "+" 1
operator _ "++"
{
    id: "IncrementPostFix"
    builtin: true
    assignment: true // ++ is an assignment operator
    precedence: 180
    associativity: left    
}

// The compiler will translate it to _ = _ "-" 1
operator _ "--"
{
    id: "DecrementPostFix"
    builtin: true
    assignment: true // ++ is an assignment operator
    precedence: 180
    associativity: left
}

operator _ "(" _ ")"
{
    id: "Call"
    builtin: true
    precedence: 180
    associativity: left
}

operator _ "[" _ "]"
{
    id: "ArrayIndexer"
    precedence: 180
    associativity: left
}

operator _ "." _
{
    id: "Member"
    builtin: true
    precedence: 180
    associativity: left
}

operator _ "->" _
{
    id: "MemberPointer"
    builtin: true
    precedence: 180
    associativity: left
}


operator "typeof" _
{
    id: "TypeOf"
    builtin: true
    precedence: 180
    associativity: left
}

operator "new" _
{
    id: "New"
    builtin: true
    precedence: 180
    associativity: right
}

// Precedence 170
// ==============

operator "+" _
{
    id: "UnaryPlus"
    precedence: 170
    associativity: right
}

operator "-" _
{
    id: "UnaryMinus"
    precedence: 170
    associativity: right
}

operator "!" _
{
    id: "LogicalNot"
    precedence: 170
    associativity: right
}

operator "~" _
{
    id: "LogicalNot"
    precedence: 170
    associativity: right
}

operator "*" _
{
    id: "PointerIndirection"
    builtin: true
    precedence: 170
    associativity: right
}

operator "&" _
{
    id: "AddressOf"
    builtin: true
    precedence: 170
    associativity: right
}

operator "sizeof" _
{
    id: "SizeOf"
    builtin: true
    precedence: 170
    associativity: right
}

public operator "throw" _
{
    id: "Throw"
    builtin: true
    precedence: 170
    associativity: right
}

public operator "ref" _
{
    id: "ref"
    builtin: true
    precedence: 170
    associativity: right
}

public operator _ "is" _
{
    id: "is"
    precedence: 170
    associativity: left
}

public operator _ "as" _
{
    id: "as"
    precedence: 170
    associativity: left
}

public operator _ "as?" _
{
    id: "as?"
    precedence: 170
    associativity: left
}

// Precedence 150
// ==============

public operator _ "*" _
{
    id: "Multiply"
    precedence: 150
    associativity: left
}

public operator _ "/" _
{
    id: "Divide"
    precedence: 150
    associativity: left
}

public operator _ "%" _
{
    id: "Modulo"
    precedence: 150
    associativity: left
}

// Precedence 140
// ==============

public operator _ "+" _
{
    id: "Add"
    precedence: 140
    associativity: left
}

public operator _ "-" _
{
    id: "Substract"
    precedence: 140
    associativity: left
}

public operator _ "&" _
{
    id: "BitAnd"
    precedence: 140
    associativity: left
}

public operator _ "^" _
{
    id: "BitXor"
    precedence: 140
    associativity: left
}

public operator _ "|" _
{
    id: "BitOr"
    precedence: 140
    associativity: left
}

// Precedence 130
// ==============

public operator _ "<<" _
{
    id: "BitShiftLeft"
    precedence: 130
    associativity: left
}

public operator _ ">>" _
{
    id: "BitShiftRight"
    precedence: 130
    associativity: left
}

// Precedence 120
// ==============

public operator _ "<" _
{
    id: "LessThan"
    precedence: 120
    associativity: left
}

public operator _ "<=" _
{
    id: "LessThanOrEqual"
    precedence: 120
    associativity: left
}

public operator _ ">" _
{
    id: "GreaterThan"
    precedence: 120
    associativity: left
}

public operator _ ">=" _
{
    id: "GreaterThanOrEqual"
    precedence: 120
    associativity: left
}

// Precedence 110
// ==============

public operator _ "==" _
{
    id: "Equal"
    precedence: 110
    associativity: left
}

public operator _ "!=" _
{
    id: "NotEqual"
    precedence: 110
    associativity: left
}

// Precedence 100 - 50
// ===================


public operator _ "&&" _
{
    id: "And"
    precedence: 70
    associativity: left
}

public operator _ "||" _
{
    id: "Or"
    precedence: 60
    associativity: left
}

public operator _ "?" _ ":" _
{
    id: "Conditional"
    precedence: 50
    associativity: right
}

// Precedence 40
// ===================

public operator _ ".." _
{
    id: "Range"
    builtin: true
    precedence: 40
}

public operator _ "..<" _
{
    id: "RangeLess"
    builtin: true
    precedence: 40
}

// all assignment operators using a binary (_ "XXX" _) are candidate to "XXX" "=" assignment operators (apart the "=" itself)

// Precedence 30
// ===================

// Group  (could be a tuple or closure...etc.)
// TODO: Check where exactly this operator should be defined in terms of precedence
operator "(" _ ")"
{
    id: "Group"
    builtin: true
    precedence: 30
}

public operator _ "," _
{
    id: "List"
    builtin: true
    precedence: 30
    associativity: left
}

// Precedence 20
// ===================

public operator _ "=" _
{
    id: "DirectAssignment"
    builtin: true
    assignment: true
    precedence: 20
    associativity: right
}
