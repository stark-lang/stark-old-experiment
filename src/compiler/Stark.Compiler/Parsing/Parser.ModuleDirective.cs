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
            NextToken(); // skip module token

            // If there are any pending modifiers, we verify that we only have public
            for (var i = _pendingModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = _pendingModifiers[i];
                if ((modifier & ModifierFlags.Public) == 0)
                {
                    LogError(modifier, "Unexpected modifier for module. Only the public modifier is supported.");
                    _pendingModifiers.RemoveAt(i);
                }
            }

            // If we have any modifiers, the span of the module directive starts on the modifier
            if (_pendingModifiers.Count > 0)
            {
                moduleDirective.Span.Start = _pendingModifiers[0].Span.Start;

                // Important: Clear any pending modifiers 
                _pendingModifiers.Clear();
            }

            // We are expecting only a plain identifier for a module
            if (_token.Type != TokenType.Identifier)
            {
                LogError("Expecting a module identifier");
                SkipAfterEod();
                return false;
            }

            // Extract module name and span
            moduleDirective.Name.Span = CurrentSpan;
            moduleDirective.Name.Value = GetAsText(_token);

            // Expect Eod but allow to continue (with an error) if we don't find any
            ExpectEod(moduleDirective);

            Close(moduleDirective);
            return true;
        }
    }
}