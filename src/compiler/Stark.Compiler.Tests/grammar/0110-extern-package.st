﻿// Simple extern package
extern package test1
// Simple extern with subpath
extern package test1::test2
// Multiple extern ;
extern package testa; extern package testb;
// Package declaration on multiline
extern
package
test3
// Module path with new lines and spaces
extern
package
test4
 ::
  test5
// Invalid token for package
extern package 5
// Invalid extern (expecting only package)
extern blabla
// Invalid subpath
extern package test1::6
// Invalid separator
extern package test1<test2
// Invalid token following path
extern package test1::test2 boooo
// Invalid visibility token
public static extern package test6
// Check Eod via Eof
extern package test7