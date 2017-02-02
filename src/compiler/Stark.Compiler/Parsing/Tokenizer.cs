// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Stark.Compiler.Parsing
{
    /// <summary>
    /// Tokenizer enumerator that generates <see cref="Token"/>, to be used from a foreach.
    /// </summary>
    public class Tokenizer<TReader> : IEnumerable<Token> where TReader : struct, CharacterIterator
    {
        private Token _token;
        private TextPosition _position;
        private TextPosition _nextPosition;
        private char32 _c;
        private TextPosition? _peekPosition;
        private TextPosition? _peekNextPosition;
        private char32? _peekC;
        private List<LogMessage> _errors;
        private int _nestedMultilineCommentCount;
        private TReader _reader;
        private const int Eof = -1;

        /// <summary>
        /// Initialize a new instance of this <see cref="Tokenizer{TReader}" />.
        /// </summary>
        /// <param name="textReader">The text to analyze</param>
        /// <param name="sourcePath">The file path used for error reporting only.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentNullException">If text is null</exception>
        public Tokenizer(TReader textReader, string sourcePath = null)
        {
            _reader = textReader;
            SourceFilePath = sourcePath ?? "<input>";
        }

        /// <summary>
        /// The file path of the text (used for reporting errors)
        /// </summary>
        public string SourceFilePath { get; }

        /// <summary>
        /// Gets a boolean indicating whether this tokenizer has errors.
        /// </summary>
        public bool HasErrors => _errors != null && _errors.Count > 0;

        /// <summary>
        /// Gets error messages.
        /// </summary>
        public IEnumerable<LogMessage> Errors => _errors ?? Enumerable.Empty<LogMessage>();

        /// <summary>
        /// Enumerator. Use simply <code>foreach</code> on this instance to automatically trigger an enumeration of the tokens.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private bool MoveNext()
        {
            // If we have errors or we are already at the end of the file, we don't continue
            if (_token.Type == TokenType.Eof)
            {
                return false;
            }

            NextToken();
            return true;
        }

        private void NextToken()
        {
            var start = _position;
            switch (_c)
            {
                case '\u0085': // next line
                case '\u2028': // line separator
                case '\u2029': // paragraph separator
                case '\n':
                    _token = new Token(TokenType.NewLine, start, _position);
                    NextChar();
                    break;
                case ';':
                    _token = new Token(TokenType.SemiColon, start, _position);
                    NextChar();
                    break;
                case '\r':
                    NextChar();
                    // case of: \r\n
                    if (_c == '\n')
                    {
                        _token = new Token(TokenType.NewLine, start, _position);
                        NextChar();
                        break;
                    }
                    // case of \r
                    _token = new Token(TokenType.NewLine, start, start);
                    break;
                case ':':
                    NextChar();
                    if (_c == ':')
                    {
                        _token = new Token(TokenType.ColonColon, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Colon, start, start);
                    break;
                case '$':
                    _token = new Token(TokenType.Dollar, start, start);
                    NextChar();
                    break;
                case '#':
                    _token = new Token(TokenType.Hash, start, start);
                    NextChar();
                    break;
                case '~':
                    _token = new Token(TokenType.Tilde, start, start);
                    NextChar();
                    break;
                case '`':
                    _token = new Token(TokenType.GraveAccent, start, start);
                    NextChar();
                    break;
                case '\\':
                    _token = new Token(TokenType.Backslash, start, start);
                    NextChar();
                    break;
                case '@':
                    NextChar();
                    if (_c == '"')
                    {
                        ReadString(start, true);
                        break;
                    }
                    _token = new Token(TokenType.At, start, start);
                    break;
                case '^':
                    NextChar();
                    _token = new Token(TokenType.Exponent, start, start);
                    break;
                case '*':
                    NextChar();
                    _token = new Token(TokenType.Star, start, start);
                    break;
                case '/':
                    NextChar();
                    if (_c == '/' || _c == '*')
                    {
                        ReadComment(start);
                        break;
                    }
                    _token = new Token(TokenType.Divide, start, start);
                    break;
                case '+':
                    NextChar();
                    _token = new Token(TokenType.Plus, start, start);
                    break;
                case '-':
                    NextChar();
                    _token = new Token(TokenType.Minus, start, start);
                    break;
                case '%':
                    NextChar();
                    _token = new Token(TokenType.Modulus, start, start);
                    break;
                case ',':
                    _token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '&':
                    NextChar();
                    _token = new Token(TokenType.And, start, start);
                    break;
                case '?':
                    NextChar();
                    _token = new Token(TokenType.Question, start, start);
                    break;
                case '|':
                    NextChar();
                    _token = new Token(TokenType.Pipe, start, start);
                    break;
                case '.':
                    NextChar();
                    _token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    _token = new Token(TokenType.Not, start, start);
                    break;

                case '=':
                    NextChar();
                    _token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    _token = new Token(TokenType.Less, start, start);
                    break;
                case '>':
                    NextChar();
                    _token = new Token(TokenType.Greater, start, start);
                    break;
                case '(':
                    _token = new Token(TokenType.OpenParen, _position, _position);
                    NextChar();
                    break;
                case ')':
                    _token = new Token(TokenType.CloseParen, _position, _position);
                    NextChar();
                    break;
                case '[':
                    _token = new Token(TokenType.OpenBracket, _position, _position);
                    NextChar();
                    break;
                case ']':
                    _token = new Token(TokenType.CloseBracket, _position, _position);
                    NextChar();
                    break;
                case '{':
                    _token = new Token(TokenType.OpenBrace, _position, _position);
                    NextChar();
                    break;
                case '}':
                    _token = new Token(TokenType.CloseBrace, _position, _position);
                    NextChar();
                    break;
                case '"':
                    ReadString(start, false);
                    break;
                case '\'':
                    ReadChar();
                    break;
                case Eof:
                    _token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    if (ConsumeWhitespace())
                    {
                        break;
                    }

                    if (CharHelper.IsIdentifierStart(_c))
                    {
                        ReadIdentifier();
                        break;
                    }

                    if (CharHelper.IsDigit(_c))
                    {
                        ReadNumber();
                        break;
                    }

                    // invalid char
                    _token = new Token(TokenType.Invalid, _position, _position);
                    NextChar();
                    break;
            }
        }

        private bool ConsumeWhitespace()
        {
            var start = _position;
            var end = _position;
            while (CharHelper.IsWhiteSpace(_c))
            {
                end = _position;
                NextChar();
            }

            if (start != _position)
            {
                _token = new Token(TokenType.Spaces, start, end);
                return true;
            }

            return false;
        }

        private void ReadIdentifier()
        {
            var start = _position;

            var starsWithUnderscore = '_' == _c;
            TextPosition beforePosition;
            bool hasLetters = false;
            while(true)
            {
                beforePosition = _position;
                NextChar();

                if (_c == '_')
                {
                    continue;
                }
                if (CharHelper.IsIdentifierContinue(_c))
                {
                    hasLetters = true;
                }
                else
                {
                    break;
                }
            } 

            _token = new Token(starsWithUnderscore && !hasLetters ? TokenType.Underscores : TokenType.Identifier, start, beforePosition);
        }

        private void ReadNumber()
        {
            var start = _position;
            var end = _position;
            var isFloat = false;

            var startsWithZero = _c == '0';
            NextChar(); // Skip first digit character

            // If we start with 0, it might be an hexa, octal or binary literal
            if (startsWithZero)
            {
                if (_c == 'x' || _c == 'X' || _c == 'o' || _c == 'O' || _c == 'b' || _c == 'B')
                {
                    string name;
                    Func<char32, bool> match;
                    string range;
                    string prefix;
                    TokenType tokenType;
                    if (_c == 'x' || _c == 'X')
                    {
                        name = "hexadecimal";
                        range = "[0-9a-zA-Z]";
                        prefix = "0x";
                        match = CharHelper.IsHexa;
                        tokenType = TokenType.IntegerHexa;
                    }
                    else if (_c == 'o' || _c == 'O')
                    {
                        name = "octal";
                        range = "[0-7]";
                        prefix = "0o";
                        match = CharHelper.IsOctal;
                        tokenType = TokenType.IntegerOctal;
                    }
                    else
                    {
                        name = "binary";
                        range = "0 or 1";
                        prefix = "0b";
                        match = CharHelper.IsBinary;
                        tokenType = TokenType.IntegerBinary;
                    }

                    end = _position;
                    NextChar(); // skip x,X,o,O,b,B

                    bool hasCharInRange = false;
                    while (true)
                    {
                        bool hasLocalCharInRange = false;
                        if (_c == '_' || (hasLocalCharInRange = match(_c)))
                        {
                            if (hasLocalCharInRange)
                            {
                                hasCharInRange = true;
                            }
                            end = _position;
                            NextChar();
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!hasCharInRange)
                    {
                        AddError($"Invalid {name} integer. Expecting at least one {range} after {prefix}", start, start);
                        _token = new Token(TokenType.Invalid, start, end);
                    }
                    else
                    {
                        _token = new Token(tokenType, start, end);
                    }
                    return;
                }
            }

            // Parse all digits
            while (CharHelper.IsDigit(_c) || _c == '_') 
            {
                end = _position;
                NextChar();
            }

            // Read any number following
            var hasRange = false;
            if (_c == '.')
            {
                // If the next char is a '.' it means that we have a range iterator, so we don't touch it
                if (PeekChar() != '.')
                {
                    end = _position;
                    NextChar(); // Skip the dot .

                    // We expect at least a digit after .
                    if (!CharHelper.IsDigit(_c))
                    {
                        AddError("Expecting at least one digit after the float dot .", _position, _position);
                        _token = new Token(TokenType.Invalid, start, end);
                        return;
                    }

                    isFloat = true;
                    while (CharHelper.IsDigit(_c) || _c == '_')
                    {
                        end = _position;
                        NextChar();
                    }
                }
                else
                {
                    // If we have a range, we don't expect to parse anything after
                    hasRange = true;
                }
            }

            // Parse only the exponent if we don't have a range
            if (!hasRange && (_c == 'e' || _c == 'E'))
            {
                isFloat = true;

                end = _position;
                NextChar();
                if (_c == '+' || _c == '-')
                {
                    end = _position;
                    NextChar();
                }

                if (!CharHelper.IsDigit(_c))
                {
                    AddError("Expecting at least one digit after the exponent", _position, _position);
                    _token = new Token(TokenType.Invalid, start, end);
                    return;
                }

                while (CharHelper.IsDigit(_c) || _c == '_')
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(isFloat ? TokenType.Float : TokenType.Integer, start, end);
        }

        private void ReadChar()
        {
            var start = _position;
            var end = _position;
            char32 startChar = _c;
            NextChar(); // Skip '
            if (ReadChar(ref end, startChar, false))
            {
                if (_c == startChar)
                {
                    end = _position;
                    NextChar();
                    _token = new Token(TokenType.Char, start, end);
                    return;
                }
                AddError($"Unexpected end of file while parsing a character not terminated by a {startChar}", end, end);
            }

            _token = new Token(TokenType.Invalid, start, end);
        }

        private void ReadString(TextPosition start, bool isRawString)
        {
            var end = _position;
            char32 startChar = _c;
            NextChar(); // Skip "
            while (true)
            {
                if (ReadChar(ref end, startChar, isRawString))
                {
                    if (_c == startChar)
                    {
                        // If double ""  for raw string, we keep it
                        if (isRawString && _c == '"' && PeekChar() == '"')
                        {
                            NextChar();
                            end = _position;
                            NextChar();
                        }
                        else
                        {
                            end = _position;
                            NextChar();
                            break;
                        }
                    }
                }
                else
                {
                    _token = new Token(TokenType.Invalid, start, end);
                    return;
                }
            }

            _token = new Token(isRawString ? TokenType.StringRaw : TokenType.String, start, end);
        }

        private bool ReadChar(ref TextPosition end, char32 startChar, bool isRawString)
        {
            if (!isRawString && _c == '\\')
            {
                end = _position;
                NextChar();
                // 0 \ ' " a b f n r t v u0000-uFFFF x00-xFF
                switch (_c)
                {
                    case '0':
                    case '\\':
                    case '\'':
                    case '"':
                    case 'a':
                    case 'b':
                    case 'f':
                    case 'n':
                    case 'r':
                    case 't':
                    case 'v':
                        end = _position;
                        NextChar();
                        return true;
                    case 'u':
                    case 'U':
                        end = _position;
                        var maxCount = _c == 'u' ? 4 : 8;
                        NextChar();

                        // Must be followed 0 to 8 hex numbers (0-FFFFFFFF)
                        {
                            int i = 0;
                            for (; CharHelper.IsHexa(_c) && i < maxCount; i++)
                            {
                                end = _position;
                                NextChar();
                            }
                            if (i == maxCount)
                            {
                                return true;
                            }
                        }
                        break;
                    case 'x':
                        end = _position;
                        NextChar();
                        // Must be followed 0 to 4 hex numbers (0-FFFF)
                        {
                            int i = 0;
                            for (; CharHelper.IsHexa(_c) && i < 4; i++)
                            {
                                end = _position;
                                NextChar();
                            }
                            if (i > 0)
                            {
                                return true;
                            }
                        }
                        break;
                }
                AddError($"Unexpected escape character [{_c}] in string. Only 0 ' \\ \" a b f n r t v u0000-uFFFF U00000000-UFFFFFFFF x0-xFFFF are allowed", _position, _position);
                return false;
            }
            else if (_c == Eof)
            {
                AddError($"Unexpected end of file while parsing a string/character not terminated by a {startChar}", end, end);
                return false;
            }
            else
            {
                if (_c != startChar)
                {
                    if (!isRawString && (_c == '\r' || _c == '\n'))
                    {
                        AddError($"Unexpected end of line while parsing a string not terminated by a {startChar}", _position, _position);
                        return false;
                    }
                    else
                    {
                        end = _position;
                        NextChar();
                    }
                }
                return true;
            }
        }

        private void ReadComment(TextPosition start)
        {
            var end = _position;

            // Read single line comment
            if (_c == '/')
            {
                // Skip second /
                NextChar();

                // Check if we have a ///
                // TODO: Add support for //!
                bool isDocComment = _c == '/';

                // Read until the end of the line/file
                while (_c != Eof && _c != '\r' && _c != '\n')
                {
                    end = _position;
                    NextChar();
                }

                _token = new Token(isDocComment ? TokenType.CommentDoc : TokenType.Comment, start, end);
            }
            else
            {
                // Skip second *
                NextChar();
                _nestedMultilineCommentCount++;

                // Read until the end of the line/file
                while (_c != Eof)
                {
                    if (_c == '/')
                    {
                        NextChar();
                        if (_c == '*')
                        {
                            _nestedMultilineCommentCount++;
                            NextChar();
                        }
                    }
                    else if (_c == '*')
                    {
                        NextChar();
                        if (_c == '/')
                        {
                            _nestedMultilineCommentCount--;
                            if (_nestedMultilineCommentCount == 0)
                            {
                                end = _position;
                                NextChar(); // skip last /
                                _token = new Token(TokenType.CommentMultiLine, start, end);
                                return;
                            }
                            NextChar();
                        }
                    }
                    else { 
                        end = _position;
                        NextChar();
                    }
                }

                AddError("Invalid multi-line comment. No matching */ for start /*", start, start);
                _token = new Token(TokenType.Invalid, start, end);
            }
        }

        /// <summary>
        /// Peeks the character next to the current one.
        /// </summary>
        private char32 PeekChar()
        {
            if (_peekC.HasValue)
            {
                return _peekC.Value;
            }

            // Save the state of the position
            var savedPosition = _position;
            var savedNextPosition = _nextPosition;

            // Move to the next position
            _position = _nextPosition;
            _peekC = NextCharFromReader();

            // Save the peek position
            _peekPosition = _position;
            _peekNextPosition = _nextPosition;

            // Restore the position before the peek
            _position = savedPosition;
            _nextPosition = savedNextPosition;

            return _peekC.Value;
        }

        private void NextChar()
        {
            // If we have any pending peek position, use it
            if (_peekPosition.HasValue)
            {
                Debug.Assert(_peekC.HasValue);
                Debug.Assert(_peekNextPosition.HasValue);
                _position = _peekPosition.Value;
                _nextPosition = _peekNextPosition.Value;
                _c = _peekC.Value;
                _peekNextPosition = null;
                _peekPosition = null;
                _peekC = null;
            }
            else
            {
                // Else move to the next position
                _position = _nextPosition;
                _c = NextCharFromReader();
            }
        }

        private char32 NextCharFromReader()
        {
            try
            {
                int position = _position.Offset;
                var nextChar = _reader.TryGetNext(ref position);
                _nextPosition.Offset = position;

                if (nextChar.HasValue)
                {
                    var nextc = nextChar.Value;
                    if (nextc == '\n')
                    {
                        _nextPosition.Column = 0;
                        _nextPosition.Line += 1;
                    }
                    else
                    {
                        _nextPosition.Column++;
                    }
                    return nextc;
                }

                return Eof;
            }
            catch (CharReaderException ex)
            {
                AddError(ex.Message, _position, _position);
            }
            return 0;
        }

        IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void AddError(string message, TextPosition start, TextPosition end)
        {
            if (_errors == null)
            {
                _errors = new List<LogMessage>();
            }
            _errors.Add(new LogMessage(ParserMessageType.Error, new SourceSpan(SourceFilePath, start, end), message));
        }

        private void Reset()
        {
            // Initialize the position at -1 when starting
            _nextPosition = new TextPosition();
            _position = new TextPosition();
            _c = NextCharFromReader();

            _token = new Token();
            _peekNextPosition = null;
            _peekPosition = null;
            _peekC = null;
            _errors = null;
            _nestedMultilineCommentCount = 0;
        }

        /// <summary>
        /// Custom enumerator on <see cref="Token"/>
        /// </summary>
        public struct Enumerator : IEnumerator<Token>
        {
            private readonly Tokenizer<TReader> _tokenizer;

            public Enumerator(Tokenizer<TReader> tokenizer)
            {
                _tokenizer = tokenizer;
                tokenizer.Reset();
            }

            public bool MoveNext()
            {
                return _tokenizer.MoveNext();
            }

            public void Reset()
            {
                _tokenizer.Reset();
            }

            public Token Current => _tokenizer._token;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}

