﻿// import simple moduel
import test1
// Simple import with subpath
import test1::test2
// Multiple import ;
import testa; import testb;
// Package declaration on multiline
import
test3
// Module path with new lines and spaces
import
test4
 ::
  test5
// Public import
public import test6
// import star
import teststar::*
// import list 1
import testlist1::{a}
// import list 2
import testlist2::{a,b}
// import list 3
import testlist3::{a,b,c}
// import list multiline 
import testlist4:: {
    a,
    b,
    c
}
// import list trailing ,
import testlist5::{a,b,}
// import this
import this::testthis
// import base
import base::testbase1
// import base 2
import base::base::testbase2
// Invalid token for package
import 5
// Invalid subpath
import test1::6
// Invalid separator
import test1<test2
// Invalid token following path
import test1::test2 boooo
// Don't allow this if not first
import badthis::this
// Don't allow base if not base
import badbase::base
// import list with missing id
import testbadlist::{}
// import list with missing id
import testbadlist::{,}
// invalid modifier
static partial import testbadmodifiers
// Check Eod via Eof
import test7