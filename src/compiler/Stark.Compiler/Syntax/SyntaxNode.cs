using System;
using System.Collections.Generic;
using Stark.Compiler.Parsing;
using Stark.Compiler.Text;

namespace Stark.Compiler.Syntax
{
    public abstract class SyntaxNode
    {
        public SourceSpan Span;
    }

    public abstract class SyntaxNodeContainer : SyntaxNode
    {
        protected SyntaxNodeContainer()
        {
            Nodes = new List<SyntaxNode>();
        }

        public List<SyntaxNode> Nodes { get; }
    }

    public class Directives : SyntaxNodeContainer
    {
    }

    public class ModuleDirective : SyntaxNode
    {
        public Visibility Visibility { get; set; }

        public ModuleName Name { get; set; }
    }

    public class Visibility : SyntaxNode
    {
        public VisibilityFlags Flags { get; set; }
    }

    public class ModuleName : SyntaxNode
    {
        public string Name { get; set; }
    }

    public abstract class TypeDeclaration : SyntaxNodeContainer
    {
    }

    public class StructDeclaration : TypeDeclaration
    {
    }

    public class ClassDeclaration : TypeDeclaration
    {
    }

    public class EnumDeclaration : TypeDeclaration
    {
    }

    public class TraitDeclaration : TypeDeclaration 
    {
    }

    public class VariableDeclaration : SyntaxNode
    {
    }

    public class FunctionDeclaration : SyntaxNode
    {
    }

    public abstract class Statement : SyntaxNode
    {
    }

    public abstract class Expression : SyntaxNode
    {
    }

    [Flags]
    public enum VisibilityFlags
    {
        Private = 0,
        Public = 1 << 0,
        Protected = 1 << 1,
        Internal = 1 << 2
    }

    [Flags]
    public enum ClassFlags
    {
        None = 0,
        Class = 1 << 0,
        Struct = 1 << 1,

        Abstract = 1 << 2,
        Virtual = 1 << 3,
        Immutable = 1 << 4,
    }

    [Flags]
    public enum PermissionFlags
    {
        Mutable = 0,
        Immutable = 1 << 0,
        Readonly = 1 << 1
    }



}