// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Stark.Compiler.Collections;
using Stark.Compiler.Syntax;
using Stark.Compiler.Text;

namespace Stark.Compiler.Parsing
{
    /// <summary>
    /// The parser.
    /// </summary>
    public partial class Parser<TSourceView> where TSourceView : ISourceView
    {
        private readonly ITokenProvider<TSourceView> _lexer;
        private IEnumerator<SyntaxToken>  _tokenIt;
        private readonly QueueArray<SyntaxToken> _tokensPreview;
        private SyntaxToken _previousToken;
        private SyntaxToken _token;
        private int _allowNewLineLevel;
        private readonly List<SyntaxValueNode<ModifierFlags>> _pendingModifiers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Parser(ITokenProvider<TSourceView> lexer)
        {
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            this._lexer = lexer;
            _tokensPreview = new QueueArray<SyntaxToken>();
            Messages = new List<LogMessage>();

            _pendingModifiers = new List<SyntaxValueNode<ModifierFlags>>();

            // Initialize the iterator
            _tokenIt = lexer.GetEnumerator();
            NextToken();
        }

        public List<LogMessage> Messages { get; private set; }

        public bool HasErrors { get; private set; }

        // private Stack<ScriptNode> Blocks { get; }

        private SourceSpan CurrentSpan => GetSpanForToken(_token);

        public Directives Run()
        {
            Messages = new List<LogMessage>();
            HasErrors = false;
            //Blocks.Clear();

            var result = new Directives();
            SyntaxNode node;
            while (TryParseDirective(out node))
            {
                if (node != null)
                {
                    result.Nodes.Add(node);
                }
            }

            if (_lexer.HasErrors)
            {
                foreach (var lexerError in _lexer.Errors)
                {
                    Log(lexerError);
                }
            }

            return result;
        }

        private bool TryParseDirective(out SyntaxNode nextNode)
        {
            bool hasNextNode = true;
            nextNode = null;

            bool continueParsing = true;
            while (continueParsing)
            {
                switch (_token.Type)
                {
                    case TokenType.Eof:
                        // Exit, check that we don't have any pending modifiers
                        CheckNoModifiers("end of file");
                        hasNextNode = false;
                        continueParsing = false;
                        break;
                    case TokenType.NewLine:
                        NextToken();
                        break;
                    case TokenType.Public:
                        AddPendingModifier(ModifierFlags.Public);
                        break;
                    case TokenType.Internal:
                        AddPendingModifier(ModifierFlags.Internal);
                        break;
                    case TokenType.Protected:
                        AddPendingModifier(ModifierFlags.Protected);
                        break;

                    case TokenType.Override:
                        AddPendingModifier(ModifierFlags.Override);
                        break;
                    case TokenType.Virtual:
                        AddPendingModifier(ModifierFlags.Virtual);
                        break;
                    case TokenType.Abstract:
                        AddPendingModifier(ModifierFlags.Abstract);
                        break;

                    case TokenType.Immutable:
                        AddPendingModifier(ModifierFlags.Immutable);
                        break;
                    case TokenType.Readonly:
                        AddPendingModifier(ModifierFlags.Readonly);
                        break;

                    case TokenType.Unsafe:
                        AddPendingModifier(ModifierFlags.Readonly);
                        break;

                    case TokenType.Partial:
                        AddPendingModifier(ModifierFlags.Partial);
                        break;

                    case TokenType.Static:
                        AddPendingModifier(ModifierFlags.Static);
                        break;

                    case TokenType.Module:
                        ModuleDirective moduleDirective;
                        continueParsing = !TryParseModule(out moduleDirective);
                        nextNode = moduleDirective;
                        break;

                    case TokenType.Import:
                        ImportDirective importDirective;
                        continueParsing = !TryParseImport(out importDirective);
                        nextNode = importDirective;
                        break;

                    case TokenType.Let:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Func:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Operator:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Extern:
                        continueParsing = !TryParseExtern(out nextNode);
                        break;

                    case TokenType.Struct:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Class:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Trait:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Enum:
                        throw new NotImplementedException();
                        break;

                    case TokenType.Extends:
                        throw new NotImplementedException();
                        break;
                    default:
                        LogError($"Unexpected token [{ToPrintable(_token)}] found");
                        NextToken();
                        break;
                }
            }

            return hasNextNode;
        }

        private void AddPendingModifier(ModifierFlags visibilityFlags)
        {
            // We don't allow any duplicate modifiers
            // so we log an error if we find any
            foreach (var it in _pendingModifiers)
            {
                if ((it & visibilityFlags) != 0)
                {
                    LogError($"Unexpected duplicated modifier [{ToPrintable(_token)}]");
                    NextToken();
                    return;
                }
            }

            var pendingModifier = new SyntaxValueNode<ModifierFlags>(_token, visibilityFlags);
            _pendingModifiers.Add(pendingModifier);

            NextToken();
        }

        private SyntaxValueNode<string> GetAsSyntaxValueNode(SyntaxToken token)
        {
            return new SyntaxValueNode<string>(token, ToText(token));
        }

        private SyntaxValueNode<ModifierFlags>? ExpectPublicPendingModifierOnly(string context)
        {
            // If there are any pending modifiers, we verify that we only have public
            for (var i = _pendingModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = _pendingModifiers[i];
                if ((modifier & ModifierFlags.Public) == 0)
                {
                    LogError(modifier, $"Unexpected modifier [{ToPrintable(modifier.Token)}] for {context}. Only the [public] modifier is supported.");
                    _pendingModifiers.RemoveAt(i);
                }
            }

            if (_pendingModifiers.Count > 0)
            {
                var result = _pendingModifiers[0];
                _pendingModifiers.Clear();
                return result;
            }
            return null;
        }

        private bool CheckNoModifiers(string type)
        {
            for (var i = _pendingModifiers.Count - 1; i >= 0; i--)
            {
                var modifier = _pendingModifiers[i];
                LogError(modifier, $"Unexpected modifier before [{type}]");
            }
            var hasModifiers = _pendingModifiers.Count > 0;
            _pendingModifiers.Clear();
            return !hasModifiers;
        }

        private void SkipAfterEod()
        {
            while (!HasEod())
            {
                NextToken();
            }
            if (_token.Type != TokenType.Eof)
            {
                NextToken();
            }
        }

        private bool ExpectDoubleColon(string context)
        {
            return ExpectTokens("::", context, TokenType.Colon, TokenType.Colon);
        }

        private bool ExpectToken(string tokenName, string context, TokenType token1)
        {
            if (_token.Type == token1)
            {
                NextToken(); // skip token1
                return true;
            }
            LogError($"Expecting token [{tokenName}] but found [{ToPrintable(_token)} while parsing {context}");
            return false;
        }

        private bool ExpectTokens(string tokenName, string context, TokenType token1, TokenType token2)
        {
            if (_token.Type == token1 && PreviewToken(1, true).Type == token2)
            {
                NextToken(); // skip token1
                NextToken(); // skip token2
                return true;
            }

            var invalidToken = _token.Type == token1 ? PreviewToken(1, true) : _token;
            LogError(invalidToken, $"Expecting token [{tokenName}] but found [{ToPrintable(invalidToken)} while parsing {context}");
            return false;
        }

        private bool ExpectEod(SyntaxNode statement)
        {
            // If we don't have an End Of Declaration (EOF or EOL or ;) following this statement
            // Output an error
            var hasEod = HasEod();
            if (!hasEod)
            {
                var errorSpan = CurrentSpan;
                while (!HasEod())
                {
                    NextToken();
                }
                errorSpan.End = _previousToken.End;

                LogError(statement, errorSpan, $"Unexpected tokens [{ToPrintable(errorSpan)}]. Expecting EOF or EOL or ; after declaration");
                if (_token.Type != TokenType.Eof)
                {
                    NextToken();
                }
            }
            else
            {
                NextToken();
            }
            return hasEod;
        }

        private bool HasEod()
        {
            return _token.Type == TokenType.NewLine || _token.Type == TokenType.SemiColon || _token.Type == TokenType.Eof;
        }

        private T Open<T>() where T : SyntaxNodeBase, new()
        {
            return Open<T>(_token);
        }

        private T Open<T>(SyntaxToken startToken) where T : SyntaxNodeBase, new()
        {
            return new T() { Span = { FileName = _lexer.Source.SourcePath, Start = startToken.Start } };
        }

        private T Close<T>(T statement) where T : SyntaxNodeBase
        {
            statement.Span.End = _previousToken.End;
            return statement;
        }

        private string ToPrintable(SyntaxToken localToken)
        {
            return CharHelper.PrintableString(ToText(localToken));
        }

        private string ToText(SyntaxToken localToken)
        {
            return localToken.GetText(_lexer.Source);
        }

        private string ToPrintable(SourceSpan span)
        {
            return CharHelper.PrintableString(_lexer.Source.GetString(span.Offset, span.Length));
        }

        private void NextToken()
        {
            _previousToken = _token;
            bool result;

            while (_tokensPreview.Count > 0)
            {
                var nextToken = _tokensPreview.Dequeue();
                if (!IsHidden(nextToken.Type))
                {
                    _token = nextToken;
                    return;
                }
            }

            // Skip Comments
            while ((result = _tokenIt.MoveNext()) && IsHidden(_tokenIt.Current.Type))
            {
            }

            _token = result ? _tokenIt.Current : SyntaxToken.Eof;
        }

        private SyntaxToken PreviewToken(int offset = 1, bool includeHiddenToken = false)
        {
            if (includeHiddenToken)
            {
                return FetchTokenAhead(offset);
            }

            // We are filtering hidden token
            while (true)
            {
                var nextToken = FetchTokenAhead(offset);
                if (!IsHidden(nextToken.Type))
                {
                    return nextToken;
                }
                offset++;
            }
        }

        private SyntaxToken FetchTokenAhead(int offset = 1)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Must be >= 0");

            if (offset == 0)
            {
                return _token;
            }

            for (int i = _tokensPreview.Count; i < offset; i++)
            {
                if (_tokenIt.MoveNext())
                {
                    _tokensPreview.Enqueue(_tokenIt.Current);
                }
                else
                {
                    break;
                }
            }

            if (_tokensPreview.Count == 0)
            {
                return SyntaxToken.Eof;
            }

            offset--;
            // optimized case for last element
            if (offset >= _tokensPreview.Count)
            {
                return _tokensPreview[_tokensPreview.Count - 1];
            }

            return _tokensPreview[offset];
        }

        private bool IsHidden(TokenType tokenType)
        {
            return tokenType == TokenType.Whitespaces || tokenType == TokenType.Comment || tokenType == TokenType.CommentMultiLine ||
                   (tokenType == TokenType.NewLine && _allowNewLineLevel > 0);
        }

        private void BeginSkipNewLines()
        {
            _allowNewLineLevel++;
            // If we have any newline on the current token, we can skip them
            while (_token.Type == TokenType.NewLine)
            {
                NextToken();
            }
        }

        private void EndSkipNewLines()
        {
            Debug.Assert(_allowNewLineLevel > 0);
            _allowNewLineLevel--;
        }

        private void LogError(string text)
        {
            LogError(_token, text);
        }

        private void LogError(SyntaxToken tokenArg, string text)
        {
            LogError(GetSpanForToken(tokenArg), text);
        }


        private void LogError<T>(SyntaxValueNode<T> tokenArg, string text)
        {
            LogError(tokenArg.Token, text);
        }

        private SourceSpan GetSpanForToken(SyntaxToken tokenArg)
        {
            return new SourceSpan(_lexer.Source.SourcePath, tokenArg.Start, tokenArg.End);
        }

        private void LogError(SourceSpan span, string text)
        {
            Log(new LogMessage(ParserMessageType.Error, span, text));
        }

        private void LogError(SyntaxNode node, string message)
        {
            LogError(node, node.Span, message);
        }

        private void LogError(SyntaxNode node, SourceSpan span, string message)
        {
            LogError(span, $"Error while parsing {node.SyntaxName}: {message}");
        }

        private void Log(LogMessage logMessage)
        {
            if (logMessage == null) throw new ArgumentNullException(nameof(logMessage));
            Messages.Add(logMessage);
            if (logMessage.Type == ParserMessageType.Error)
            {
                HasErrors = true;
            }
        }
    }
}