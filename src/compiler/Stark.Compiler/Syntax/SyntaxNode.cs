// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Syntax
{
    // WIP: Temporary SyntaxNodes

    // TODO: Implement as a struct instead
    public class ValueList<T> : List<T>
    {
    }

    public abstract class SyntaxNode
    {
        public SourceSpan Span;
    }

    public struct SyntaxValueNode<T>
    {
        public SyntaxValueNode(SourceSpan span, T value)
        {
            Span = span;
            Value = value;
        }

        public SourceSpan Span;

        public T Value;

        public static implicit operator T(SyntaxValueNode<T> node)
        {
            return node.Value;
        }
    }

    public abstract class ContainerDeclaration : Declaration
    {
        protected ContainerDeclaration()
        {
            Nodes = new ValueList<SyntaxNode>();
        }

        public ValueList<SyntaxNode> Nodes;
    }

    public class Directives : SyntaxNode
    {
        public Directives()
        {
            Nodes = new ValueList<SyntaxNode>();
        }

        public ValueList<SyntaxNode> Nodes;
    }

    public abstract class Declaration : SyntaxNode
    {
        public ValueList<SyntaxValueNode<ModifierFlags>> Modifiers;
    }

    public class ModuleDirective : Declaration
    {
        public SyntaxValueNode<string> Name;
    }

    public class ModulePath : SyntaxNode
    {
        public ModulePath()
        {
            Items = new ValueList<SyntaxValueNode<string>>();
        }

        public ValueList<SyntaxValueNode<string>> Items;
    }

    public class ModuleFullName : ModulePath
    {
        public ModuleFullName()
        {
        }

        public SyntaxValueNode<string> Name;
    }


    public class ImportPath : ModulePath
    {
        public ImportPath()
        {
            ImportList = new ValueList<ImportNameOrAlias>();
        }

        public ImportNameOrAlias? Import;

        public bool ImportAll;

        public ValueList<ImportNameOrAlias> ImportList;
    }

    public struct ImportNameOrAlias
    {
        public SyntaxValueNode<string> Name;

        public SyntaxValueNode<string>? Alias;
    }


    public class ImportDirective : SyntaxNode
    {
        public SyntaxValueNode<ModifierFlags>? Public;

        public ImportPath ImportPath;
    }

    public class ExternPackage : ModuleFullName
    {
    }

    public class TypeReference : SyntaxNode
    {
        
    }

    public abstract class TypeDeclaration : ContainerDeclaration
    {
        public SyntaxValueNode<string> Name;

        public ValueList<TemplateParameter> TemplateParameters;
    }

    public class ClassDeclaration : TypeDeclaration
    {
        public TypeReference Extends;

        public ValueList<TypeReference> Implements;
    }

    public class EnumDeclaration : TypeDeclaration
    {
    }

    public class TraitDeclaration : TypeDeclaration 
    {
    }

    public class VariableDeclaration : SyntaxNode
    {
        public TypeReference Type { get; set; }
    }

    public class TemplateParameter : SyntaxNode
    {
        public SyntaxValueNode<string> Name;
    }

    public class FunctionDeclaration : Declaration
    {
        public ValueList<TemplateParameter> TemplateParameters;

        public ValueList<VariableDeclaration> Parameters;

        public TypeReference ReturnType { get; set; }

        public ValueList<Statement> Statememts;
    }

    public abstract class Statement : SyntaxNode
    {
    }

    public abstract class Expression : SyntaxNode
    {
    }

    [Flags]
    public enum ModifierFlags
    {
        None = 0,

        // Visibility
        Public = 1 << 1,
        Protected = 1 << 2,
        Internal = 1 << 3,

        // Inheritance
        Virtual = 1 << 4,
        Abstract = 1 << 5,
        Override = 1 << 6,
        Static = 1 << 7,

        // Permission
        Immutable = 1 << 8,
        Readonly = 1 << 9,
          
        // Unsafe
        Unsafe = 1 << 10,

        // Partial
        Partial = 1 << 11,
    }

    public static class ModifierFlagsExtension
    {
        private const int VisibilityFlags = (int)(ModifierFlags.Public | ModifierFlags.Protected | ModifierFlags.Internal);
        private const int InheritanceFlags = (int)(ModifierFlags.Virtual | ModifierFlags.Abstract | ModifierFlags.Override | ModifierFlags.Static);
        private const int PermissionFlags = (int)(ModifierFlags.Immutable | ModifierFlags.Readonly);

        public static bool HasVisibility(this ModifierFlags flags)
        {
            return ((int)flags & VisibilityFlags) != 0;
        }

        public static bool HasVisibilityOnly(this ModifierFlags flags)
        {
            return ((int) flags & VisibilityFlags) != 0 && ((int) flags & ~VisibilityFlags) == 0;
        }

        public static bool HasInheritance(this ModifierFlags flags)
        {
            return ((int)flags & InheritanceFlags) != 0;
        }

        public static bool HasPermission(this ModifierFlags flags)
        {
            return ((int)flags & PermissionFlags) != 0;
        }

        public static bool HasUnsafe(this ModifierFlags flags)
        {
            return (flags & ModifierFlags.Unsafe) != 0;
        }

        public static bool HasPartial(this ModifierFlags flags)
        {
            return (flags & ModifierFlags.Partial) != 0;
        }
    }

    [Flags]
    public enum ClassFlags
    {
        None = 0,
        Class = 1 << 0,
        Struct = 1 << 1,
    }

    [Flags]
    public enum ThisFlags
    {
        None = 0,
        This = 1 << 0,
        Base = 1 << 1,
    }
}