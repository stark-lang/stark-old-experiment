// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Syntax
{

    public interface ISyntaxVisitor
    {
        void Accept(SyntaxNode node);

        void Accept(ModuleDirective module);

        void Accept(ExternPackageDirective externPackage);

        void Accept(ImportDirective import);
    }

    // WIP: Temporary SyntaxNodes

    // TODO: Implement as a struct instead
    public class ValueList<T> : List<T>
    {
    }

    public abstract class SyntaxNodeBase
    {
        public SourceSpan Span;

        public virtual string SyntaxName => this.GetType().Name;
    }

    public abstract class SyntaxNode : SyntaxNodeBase
    {
        public abstract void Visit(ISyntaxVisitor visitor);
    }

    public struct SyntaxValueNode<T>
    {
        public SyntaxValueNode(SyntaxToken token, T value)
        {
            Token = token;
            Value = value;
        }

        public SyntaxToken Token;

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
        public override void Visit(ISyntaxVisitor visitor)
        {
            foreach (var directive in Nodes)
            {
                directive.Visit(visitor);
            }
        }
    }

    public abstract class Directive : SyntaxNode
    {
    }

    public abstract class Declaration : Directive
    {
        protected Declaration()
        {
            Modifiers = new ValueList<SyntaxValueNode<ModifierFlags>>();
        }

        public ValueList<SyntaxValueNode<ModifierFlags>> Modifiers;
    }

    public class ModuleDirective : Declaration
    {
        public SyntaxValueNode<string> Name;

        public override string SyntaxName => "module";

        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class ModulePath : SyntaxNodeBase
    {
        public ModulePath()
        {
            Items = new ValueList<SyntaxValueNode<string>>();
        }

        public ValueList<SyntaxValueNode<string>> Items;
    }

    public class ModuleFullName : ModulePath
    {
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


    public class ImportDirective : Directive
    {
        public SyntaxValueNode<ModifierFlags>? Public;

        public ImportPath ImportPath;

        public override string SyntaxName => "import";

        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class ExternPackageDirective : Directive
    {
        public ModuleFullName PackageName;

        public override string SyntaxName => "extern package";

        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class TypeReference : SyntaxNode
    {
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
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

        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class EnumDeclaration : TypeDeclaration
    {
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class TraitDeclaration : TypeDeclaration 
    {
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class VariableDeclaration : SyntaxNode
    {
        public TypeReference Type { get; set; }
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class TemplateParameter : SyntaxNode
    {
        public SyntaxValueNode<string> Name;
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
    }

    public class FunctionDeclaration : Declaration
    {
        public ValueList<TemplateParameter> TemplateParameters;

        public ValueList<VariableDeclaration> Parameters;

        public TypeReference ReturnType { get; set; }

        public ValueList<Statement> Statememts;
        public override void Visit(ISyntaxVisitor visitor)
        {
            visitor.Accept(this);
        }
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