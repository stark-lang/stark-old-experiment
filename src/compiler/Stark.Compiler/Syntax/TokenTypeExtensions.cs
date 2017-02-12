﻿// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stark.Compiler.Syntax
{
    internal static class TokenTypeExtensions
    {
        private static readonly Dictionary<TokenType, string> TokenTexts = new Dictionary<TokenType, string>();

        static TokenTypeExtensions()
        {
            foreach (var field in typeof(TokenType).GetTypeInfo().DeclaredFields.Where(field => field.IsPublic && field.IsStatic))
            {
                var tokenText = field.GetCustomAttribute<TokenTextAttribute>();
                if (tokenText != null)
                {
                    var type = (TokenType) field.GetValue(null);
                    TokenTexts.Add(type, tokenText.Text);
                }
            }
        }

        public static bool HasText(this TokenType type)
        {
            return TokenTexts.ContainsKey(type);
        }

        public static string ToText(this TokenType type)
        {
            string value;
            TokenTexts.TryGetValue(type, out value);
            return value;
        }
    }
}