using ColorCode.Core;
using ColorCode.Core.Compilation.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ColorCode.Core.Tests;

/// <summary>
/// Test cases for C# language parsing
/// </summary>
[TestClass]
public class CSharpParsingTests : LanguageParsingTestBase
{
    private CSharp? _csharp;

    [TestInitialize]
    public new void Initialize()
    {
        base.Initialize();
        _csharp = new CSharp();
    }

    [TestMethod]
    public void ParseSimpleClass()
    {
        var sourceCode = @"public class MyClass
{
    public void MyMethod()
    {
        var x = 10;
    }
}";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseSingleLineComment()
    {
        var sourceCode = "// This is a comment\nvar x = 10; // inline comment";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseMultiLineComment()
    {
        var sourceCode = @"/* This is a
           multiline
           comment */
var x = 10;";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseUnclosedMultiLineComment_DoesNotHang()
    {
        // This is critical for ReDoS prevention
        var sourceCode = @"/* This comment is not closed
var x = 10;
" + new string('x', 10000); // Large content

        VerifyParsingCompletes(sourceCode, _csharp!, timeoutMs: 2000);
    }

    [TestMethod]
    public void ParseXmlDocComment()
    {
        var sourceCode = @"/// <summary>
/// This is a method
/// </summary>
/// <param name=""value"">A value</param>
public void MyMethod(int value)
{
}";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseStrings()
    {
        var sourceCode = @"var str1 = ""Normal string"";
var str2 = @""Verbatim string"";
var str3 = ""String with \""escaped quotes\"""";";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseNamespaceAndUsings()
    {
        var sourceCode = @"using System;
using System.Collections.Generic;

namespace MyNamespace
{
    public class MyClass { }
}";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseGenericTypes()
    {
        var sourceCode = @"var list = new List<string>();
var dict = new Dictionary<string, int>();
public void Method<T>(T value) { }";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseComplexCSharpCode()
    {
        var sourceCode = @"using System;

namespace EasyDocs.Core
{
    /// <summary>
    /// Main application class
    /// </summary>
    public class Application
    {
        /* Initialize the application */
        public Application()
        {
            // Constructor code
        }

        /// <param name=""args"">Command line arguments</param>
        public void Run(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
        }
    }
}";
        VerifyParsingCompletes(sourceCode, _csharp!);
        
        var result = Parse(sourceCode, _csharp!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void HasCorrectAlias()
    {
        Assert.IsTrue(_csharp!.HasAlias("cs"));
        Assert.IsTrue(_csharp!.HasAlias("c#"));
        Assert.IsTrue(_csharp!.HasAlias("csharp"));
        Assert.IsTrue(_csharp!.HasAlias("cake"));
        Assert.IsFalse(_csharp!.HasAlias("java"));
    }
}
