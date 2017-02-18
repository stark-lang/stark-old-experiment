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

            BeginSkipNewLines();
            NextToken(); // skip import token
            EndSkipNewLines();

            // Extract the public modifier if any
            importDirective.Public = ExpectPublicPendingModifierOnly("import");

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
            if (!TryParseModulePath(true, out importPath))
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
                // Inside a {...}, all NewLines are ignored
                BeginSkipNewLines();
                NextToken(); // Skip OpenBrace

                if (!TryParseImportNameOrAlias(out importNameOrAlias))
                {
                    LogError($"Unexpected token [{ToPrintable(_token)}]. Expecting at least an identifier inside an import path list {{...}}");
                    EndSkipNewLines();
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

                        LogError($"Unexpected token [{ToPrintable(_token)}]. Expecting at least an identifier inside an import path list {{...}}");
                        EndSkipNewLines();
                        return false;
                    }
                }

                // Exit of the list, restore expecting newline
                EndSkipNewLines();
                if (_token.Type != TokenType.CloseBrace)
                {
                    LogError($"Unexpected token  [{ToPrintable(_token)}]. Expecting a closing }} for an import path list {{...}}");
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

            importNameOrAlias.Name = GetAsSyntaxValueNode(_token);

            bool isValid = true;

            // Skip the lines when parsing an as, as we can have any NL in between
            BeginSkipNewLines();

            // Check if we have an alias
            if (PreviewToken(1).Type == TokenType.As)
            {
                NextToken(); // Skip name
                NextToken(); // skip as keyword
                EndSkipNewLines();

                if (_token.Type != TokenType.Identifier)
                {
                    LogError(_token, $"Unexpected token [{ToPrintable(_token)}]. Expecting an identifier after an alias name");
                    isValid = false;
                }
                else
                {
                    importNameOrAlias.Alias = GetAsSyntaxValueNode(_token);
                    NextToken();
                }
            }
            else
            {
                // We don't have [as] keyword, so we restore NL parsing
                EndSkipNewLines();

                // Skip name
                NextToken();
            }

            return isValid;
        }

        private bool TryParseModuleFullName<T>(out T modulePath) where T : ModuleFullName, new()
        {
            // A ModuleFullName don't expect any this/base module prefix
            if (!TryParseModulePath(false, out modulePath))
            {
                return false;
            }

            if (_token.Type != TokenType.Identifier)
            {
                LogError($"Unexpected token [{ToPrintable(_token)}]. Expecting an identifier while parsing a full module name");
                return false;
            }

            // Fetch the module name
            modulePath.Name = GetAsSyntaxValueNode(_token);

            // Skip Identifier
            NextToken();

            Close(modulePath);

            return true;
        }

        private bool TryParseModulePath<T>(bool allowThisAndBase, out T modulePath) where T : ModulePath, new()
        {
            modulePath = Open<T>();

            // Skip any newlines when parsing a module path
            // as the module path is not used alone (it always expect a trailing ::)
            BeginSkipNewLines();

            bool isValidPath = true;
            if (allowThisAndBase)
            {
                if (_token.Type == TokenType.This)
                {
                    modulePath.Items.Add(new SyntaxValueNode<string>(_token, "this"));
                    NextToken();
                    isValidPath = ExpectDoubleColon("module path after 'this'");
                }
                else
                {
                    while (_token.Type == TokenType.Base)
                    {
                        modulePath.Items.Add(new SyntaxValueNode<string>(_token, "base"));
                        NextToken();
                        var isFollowedByColon = ExpectDoubleColon("module path after 'base'");
                        if (!isFollowedByColon)
                        {
                            isValidPath = false;
                            break;
                        }
                    }
                }
            }

            // Parse all identifiers that may composed the modulePath
            if (isValidPath)
            {
                while (_token.Type == TokenType.Identifier && PreviewToken(1).Type == TokenType.Colon && PreviewToken(2).Type == TokenType.Colon)
                {
                    var id = ToText(_token);
                    modulePath.Items.Add(new SyntaxValueNode<string>(_token, id));
                    NextToken(); // Skip identifier

                    // We verify that we have an exact :: following it (as the Preview may introduce spaces, we need to check that it is actually a plain ::
                    if (!ExpectDoubleColon($"module path after identifier [{id}]"))
                    {
                        isValidPath = false;
                        break;
                    }
                }
            }

            // Restore newlines parsing
            EndSkipNewLines();

            if (isValidPath)
            {
                Close(modulePath);
            }

            return isValidPath;
        }
    }
}