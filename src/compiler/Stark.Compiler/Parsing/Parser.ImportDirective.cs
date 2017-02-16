// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using Stark.Compiler.Syntax;
using Stark.Compiler.Text;

namespace Stark.Compiler.Parsing
{
    public partial class Parser<TSourceView> where TSourceView : ISourceView
    {
        private bool TryParseImport(out ImportDirective importDirective)
        {
            // ImportDirective: 'public'? 'import' ImportPath Eod;
            // 
            // ImportPath: ModulePath ASTERISK
            //           | ModulePath OPEN_BRACE ImportNameOrAlias (COMMA ImportNameOrAlias)* CLOSE_BRACE
            //           | ModulePath? ImportNameOrAlias
            //           ;
            // 
            // // The ImportName can either be a module name or a type name
            // ImportName: IDENTIFIER;
            // ImportNameOrAlias: ImportName ('as' ImportName)?;

            importDirective = Open<ImportDirective>();
            NextToken(); // skip import token

            // If there are any pending modifiers, we verify that we only have public
            for (var i = _pendingModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = _pendingModifiers[i];
                if ((modifier & ModifierFlags.Public) == 0)
                {
                    LogError(modifier, "Unexpected modifier for import. Only the public modifier is supported.");
                    _pendingModifiers.RemoveAt(i);
                }
            }

            // If we have any modifiers, the span of the module directive starts on the modifier
            if (_pendingModifiers.Count > 0)
            {
                importDirective.Span.Start = _pendingModifiers[0].Span.Start;
                importDirective.Public = new SyntaxValueNode<ModifierFlags>(_pendingModifiers[0].Span, ModifierFlags.Public);

                // Important: Clear any pending modifiers 
                _pendingModifiers.Clear();
            }

            // Parse the import path
            if (!TryParseImportPath(out importDirective.ImportPath))
            {
                SkipAfterEod();
                return false;
            }

            // Expect Eod but allow to continue (with an error) if we don't find any
            ExpectEod(importDirective);

            Close(importDirective);
            return true;
        }

        private bool TryParseImportPath(out ImportPath importPath)
        {
            if (!TryParseModulePath(out importPath))
            {
                return false;
            }

            ImportNameOrAlias importNameOrAlias;

            if (TryParseImportNameOrAlias(out importNameOrAlias))
            {
                importPath.Import = importNameOrAlias;
                Close(importPath);
                return true;
            }

            if (_token.Type == TokenType.Asterisk)
            {
                importPath.ImportAll = true;
                NextToken();
            }
            else if (_token.Type == TokenType.OpenBrace)
            {
                NextToken(); // Skip OpenBrace

                // Inside a {...}, we allow new lines
                _allowNewLineLevel++;

                if (!TryParseImportNameOrAlias(out importNameOrAlias))
                {
                    LogError($"Invalid token found [{GetAsText(_token)}]. Expecting at least an identifier inside an import path list {{...}}");
                    return false;
                }

                while (_token.Type == TokenType.Comma)
                {
                    NextToken(); // Skip Comma
                    if (TryParseImportNameOrAlias(out importNameOrAlias))
                    {
                        importPath.ImportList.Add(importNameOrAlias);
                    }
                    else
                    {
                        if (_token.Type == TokenType.CloseBrace)
                        {
                            break;
                        }

                        LogError($"Invalid token found [{GetAsText(_token)}]. Expecting at least an identifier inside an import path list {{...}}");
                        _allowNewLineLevel--;
                        return false;
                    }
                }

                // Exit of the list, restore expecting newline
                _allowNewLineLevel--;
                if (_token.Type != TokenType.CloseBrace)
                {
                    LogError($"Invalid token found [{GetAsText(_token)}]. Expecting a closing }} for an import path list {{...}}");
                    return false;
                }
                NextToken();
            }
            else
            {
                LogError("Expecting identifier, * or {...} for import name");
                return false;
            }

            Close(importPath);
            return true;
        }

        private bool TryParseImportNameOrAlias(out ImportNameOrAlias importNameOrAlias)
        {
            importNameOrAlias = default(ImportNameOrAlias);
            if (_token.Type != TokenType.Identifier)
            {
                return false;
            }

            importNameOrAlias.Name = new SyntaxValueNode<string>(CurrentSpan, GetAsText(_token));

            // Skip the name
            NextToken();

            // Check if we have an alias
            if (_token.Type == TokenType.As)
            {
                NextToken(); // skip as keyword

                var tokenFound = _token;
                if (!ExpectToken("identifier", $"alias for import name [{importNameOrAlias.Name.Value}]", TokenType.Identifier))
                {
                    return false;
                }
                importNameOrAlias.Alias = new SyntaxValueNode<string>(GetSpanForToken(tokenFound), GetAsText(tokenFound));
            }

            return true;
        }

        private bool TryParseModuleFullName<T>(out T modulePath) where T : ModuleFullName, new()
        {
            if (!TryParseModulePath(out modulePath))
            {
                return false;
            }

            if (_token.Type != TokenType.Identifier)
            {
                LogError("Expecting an identifier while parsing a full module name");
                return false;
            }

            // Fetch the module name
            modulePath.Name.Span = CurrentSpan;
            modulePath.Name.Value = GetAsText(_token);

            return true;
        }

        private bool TryParseModulePath<T>(out T modulePath) where T : ModulePath, new()
        {
            modulePath = Open<T>();
            bool isValidPath = true;
            if (_token.Type == TokenType.This)
            {
                modulePath.Items.Add(new SyntaxValueNode<string>(CurrentSpan, "this"));
                NextToken();
                isValidPath = ExpectDoubleColon("module path after 'this'");
            }
            else
            {
                while (_token.Type == TokenType.Base)
                {
                    modulePath.Items.Add(new SyntaxValueNode<string>(CurrentSpan, "base"));
                    NextToken();
                    var isFollowedByColon = ExpectDoubleColon("module path after 'base'");
                    if (!isFollowedByColon)
                    {
                        isValidPath = false;
                        break;
                    }
                }
            }

            // Parse all identifiers that may composed the modulePath
            if (isValidPath)
            {
                while (_token.Type == TokenType.Identifier && PreviewToken(1).Type == TokenType.Asterisk && PreviewToken(2).Type == TokenType.Asterisk)
                {
                    var id = GetAsText(_token);
                    modulePath.Items.Add(new SyntaxValueNode<string>(CurrentSpan, id));
                    NextToken(); // Skip identifier

                    // We verify that we have an exact :: following it (as the Preview may introduce spaces, we need to check that it is actually a plain ::
                    if (!ExpectDoubleColon($"module path after identifier [{id}]"))
                    {
                        isValidPath = false;
                        break;
                    }
                }
            }

            return isValidPath;
        }
    }
}