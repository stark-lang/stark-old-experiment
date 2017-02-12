// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.
using System;
using System.Text;
using Stark.Compiler.Text;

namespace Stark.Compiler.Parsing
{
    public class LogMessage
    {
        // TODO: Use an ERROR_CODE instead

        public LogMessage(ParserMessageType type, SourceSpan span, string message)
        {
            Type = type;
            Span = span;
            Message = message;
        }

        public ParserMessageType Type { get; set; }

        public SourceSpan Span { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Span.ToStringSimple());
            builder.Append(" : ");
            switch (Type)
            {
                case ParserMessageType.Error:
                    builder.Append("error");
                    break;
                case ParserMessageType.Warning:
                    builder.Append("warning");
                    break;
                default:
                    throw new InvalidOperationException($"Message type [{Type}] not supported");
            }
            builder.Append(" : ");
            if (Message != null)
            {
                builder.Append(Message);
            }
            return builder.ToString();
        }
    }

    public enum ParserMessageType
    {
        Error,

        Warning,
    }
}