// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using Stark.Compiler.Syntax;
using Stark.Compiler.Text;

namespace Stark.Compiler.Parsing
{
    public partial class Parser<TSourceView> where TSourceView : ISourceView
    {
        private bool TryParseExtern(out SyntaxNode syntaxNode)
        {
            // ExternDirective: ExternPackageDirective
            //                ;
            //                
            // ExternPackageDirective: 'extern' 'package' Package Eod;
            // 
            // // note that this:: or base:: modules are not supported for a Package
            // Package: ModuleFullName;

            // We don't expect any modifiers for the extern directive
            syntaxNode = null;
            CheckNoModifiers("extern");
            var externToken = _token;
            NextToken(); // skip extern token

            if (_token.Type == TokenType.Package)
            {
                NextToken(); // skip package token

                ExternPackage externPackage;
                if (TryParseModuleFullName(out externPackage))
                {
                    externPackage.Span.Start = externToken.Start;

                    // Expect Eod but allow to continue (with an error) if we don't find any
                    ExpectEod(externPackage);

                    syntaxNode = Close(externPackage);
                    return true;
                }
            }
            else
            {
                LogError($"Unsupported extern [{GetAsText(_token)}");
            }

            SkipAfterEod();
            return false;
        }

    }
}