// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using Stark.Compiler.Syntax;
using Stark.Compiler.Text;

namespace Stark.Compiler.Parsing
{
    public partial class Parser<TSourceView> where TSourceView : ISourceView
    {
        private bool TryParseModule(out ModuleDirective moduleDirective)
        {
            // ModuleDirective: Visibility? 'module' ModuleName Eod;
            // ModuleName: IDENTIFIER;

            moduleDirective = Open<ModuleDirective>();

            // We allow to have new lines with the module and module id
            BeginSkipNewLines();
            {
                NextToken(); // skip module token
            }
            EndSkipNewLines();

            // If there are any pending modifiers, we verify that we only have public
            var publicModifier = ExpectPublicPendingModifierOnly("module");
            if (publicModifier.HasValue)
            {
                var modifier = publicModifier.Value;
                moduleDirective.Span.Start = modifier.Token.Start;
                moduleDirective.Modifiers.Add(modifier);
            }

            // We are expecting only a plain identifier for a module
            if (_token.Type != TokenType.Identifier)
            {
                LogError("Expecting a module identifier");
                SkipAfterEod();
                return false;
            }

            // Extract module name and span
            moduleDirective.Name = GetAsSyntaxValueNode(_token);

            // Skip module name
            NextToken();

            Close(moduleDirective);

            // Expect Eod but allow to continue (with an error) if we don't find any
            ExpectEod(moduleDirective);
            return true;
        }
    }
}