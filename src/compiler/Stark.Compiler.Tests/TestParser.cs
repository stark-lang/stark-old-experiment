// Copyright (c) The Stark Programming Language Contributors. All rights reserved.
// Licensed under the MIT license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Stark.Compiler.Parsing;
using Stark.Compiler.Syntax;
using Stark.Compiler.Text;

namespace Stark.Compiler.Tests
{
    [TestFixture]
    public class TestParser
    {
        private const string RelativeBasePath = @"..\..\grammar";
        private const string InputFilePattern = "*.sk";
        private const string OutputEndFileExtension = ".txt";

        [TestCaseSource("TestFiles")]
        public void Tests(TestFilePath testFilePath)
        {
            var inputName = testFilePath.FilePath;
            var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));

            var inputFile = Path.Combine(baseDir, inputName);
            var inputText = File.ReadAllText(inputFile);

            var expectedOutputFile = Path.ChangeExtension(inputFile, OutputEndFileExtension);
            Assert.True(File.Exists(expectedOutputFile), $"Expecting output result file [{expectedOutputFile}] for input file [{inputName}]");
            var expectedOutputText = File.ReadAllText(expectedOutputFile, Encoding.UTF8);

            var lexer = new Lexer<StringSourceView, StringCharacterIterator>(new StringSourceView(inputText, "<input>"));
            var parser = new Parser<StringSourceView>(lexer);

            var directives = parser.Run();

            var stringWriter = new StringWriter() {NewLine = "\n"};
            var printer = new SyntaxPrettyPrinter(stringWriter);
            directives.Visit(printer);
            var result = stringWriter.ToString();

            foreach (var message in parser.Messages)
            {
                result += message + "\n";
            }

            // Only display results if they are different
            if (expectedOutputText != result)
            {
                Console.WriteLine("Result");
                Console.WriteLine("======================================");
                Console.WriteLine(result);
                Console.WriteLine("Expected");
                Console.WriteLine("======================================");
                Console.WriteLine(expectedOutputText);
            }

            TextAssert.AreEqual(expectedOutputText, result);
        }

        public static IEnumerable<object[]> TestFiles
        {
            get
            {
                var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));
                return
                    Directory.EnumerateFiles(baseDir, InputFilePattern, SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(OutputEndFileExtension))
                        .Select(f => f.StartsWith(baseDir) ? f.Substring(baseDir.Length + 1) : f)
                        .OrderBy(f => f)
                        .Select(x => new object[]
                        {
                            new TestFilePath(x)
                        });
            }
        }

        /// <summary>
        /// Use an internal class to have a better display of the filename in Resharper Unit Tests runner.
        /// </summary>
        public struct TestFilePath
        {
            public TestFilePath(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }

            public override string ToString()
            {
                return FilePath;
            }
        }

        private static string BaseDirectory
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var codebase = new Uri(assembly.CodeBase);
                var path = codebase.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }
    }

    public class SyntaxPrettyPrinter : ISyntaxVisitor
    {
        private readonly TextWriter writer;

        public SyntaxPrettyPrinter(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            this.writer = writer;
        }

        public void Accept(SyntaxNode node)
        {
            Write(node);
            WriteLine();
        }

        public void Accept(ModuleDirective module)
        {
            Write(module);
            Write(", name", module.Name);
            WriteLine();
        }

        public void Accept(ExternPackageDirective externPackage)
        {
            Write(externPackage);
            if (externPackage.PackageName != null)
            {
                writer.Write(", ");
                Write(externPackage.PackageName);
            }
            WriteLine();
        }

        public void Accept(ImportDirective import)
        {
            Write(import);
            if (import.ImportPath != null)
            {
                writer.Write(", ");
                Write(import.ImportPath);
            }
            WriteLine();
        }

        private void Write(ModulePath path)
        {
            Write((SyntaxNodeBase)path);
            for (var i = 0; i < path.Items.Count; i++)
            {
                var item = path.Items[i];
                Write($", [{i}]", item);
            }
        }

        private void Write(ModuleFullName path)
        {
            Write((ModulePath) path);
            Write(", name", path.Name);
        }

        private void Write(ImportPath path)
        {
            Write((ModulePath)path);

            if (path.Import.HasValue)
            {
                writer.Write(" :: ");
                Write(path.Import.Value);
            }

            if (path.ImportAll)
            {
                writer.Write(" :: *");
            }

            if (path.ImportList.Count > 0)
            {
                writer.Write(" :: { ");
                for (var i = 0; i < path.ImportList.Count; i++)
                {
                    if (i > 0)
                    {
                        writer.Write(" , ");
                    }
                    var importNameOrAlias = path.ImportList[i];
                    Write(importNameOrAlias);
                }
                writer.Write(" }");
            }
        }

        private void Write(ImportNameOrAlias nameOrAlias)
        {
            Write("name", nameOrAlias.Name);
            if (nameOrAlias.Alias.HasValue)
            {
                Write(" as ", nameOrAlias.Alias.Value);
            }
        }

        private void Write(SyntaxNodeBase node)
        {
            writer.Write(node.SyntaxName);
            writer.Write(" (");
            writer.Write(node.Span.Start.ToStringSimple());
            writer.Write(")-(");
            writer.Write(node.Span.End.ToStringSimple());
            writer.Write(")");

            if (node is Declaration)
            {
                var declaration = (Declaration)node;

                for (var i = 0; i < declaration.Modifiers.Count; i++)
                {
                    var modifier = declaration.Modifiers[i];
                    Write($", modifier{i}", modifier);
                }
            }
        }

        private void Write<T>(string key, SyntaxValueNode<T> valueNode)
        {
            writer.Write(key);
            writer.Write(" (");
            writer.Write(valueNode.Token.Start.ToStringSimple());
            writer.Write(")-(");
            writer.Write(valueNode.Token.End.ToStringSimple());
            writer.Write(") = ");
            writer.Write(valueNode.Value);
        }

        private void WriteLine()
        {
            writer.WriteLine();
        }
    }
}