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

            BeginSkipNewLines();
            {
                NextToken(); // skip extern token
            }
            EndSkipNewLines();

            if (_token.Type == TokenType.Package)
            {
                var externPackageDirective = Open<ExternPackageDirective>(externToken);

                BeginSkipNewLines();
                {
                    NextToken(); // skip package token
                }
                EndSkipNewLines();

                if (TryParseModuleFullName(out externPackageDirective.PackageName))
                {
                    // Expect Eod but allow to continue (with an error) if we don't find any
                    syntaxNode = Close(externPackageDirective);

                    ExpectEod(externPackageDirective);
                    return true;
                }
            }
            else
            {
                LogError(HasEod()
                    ? $"Unexpected EOF, or EOL or ; after extern"
                    : $"Unexpected extern [{ToPrintable(_token)}]");
            }

            SkipAfterEod();
            return false;
        }

    }
}