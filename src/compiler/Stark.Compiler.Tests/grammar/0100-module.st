// Simple module declaration
module test1
// Simple module declaration with public
public module test2
// Trailing semicolon
public module test3;public module test4;
// Newlines
public
 module
  test5
// Invalid token for module name
module 5
// Invalid duplicated visibility
public public module test_invalid1
// Invalid modifier
static partial module test_invalid2
// Invalid Eod
module test_invalid3 ::<=
// Check Eod via Eof
module test6