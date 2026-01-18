using ColorCode.Core;
using ColorCode.Core.Compilation.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ColorCode.Core.Tests;

/// <summary>
/// Specific tests for ReDoS (Regular Expression Denial of Service) prevention
/// These tests verify that malformed or unclosed constructs don't cause catastrophic backtracking
/// </summary>
[TestClass]
public class RegexDenialOfServicePreventionTests : LanguageParsingTestBase
{
    [TestMethod]
    [Timeout(3000)] // Should complete within 3 seconds
    public void Markdown_UnclosedCodeBlock_DoesNotHang()
    {
        var markdown = new Markdown();
        var sourceCode = "```\n" + new string('x', 100000); // 100KB unclosed code block

        VerifyParsingCompletes(sourceCode, markdown, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void CSharp_UnclosedMultilineComment_DoesNotHang()
    {
        var csharp = new CSharp();
        var sourceCode = "/* " + new string('x', 100000); // 100KB unclosed comment

        VerifyParsingCompletes(sourceCode, csharp, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void CSharp_DeepNesting_DoesNotHang()
    {
        var csharp = new CSharp();
        var sourceCode = @"class A { class B { class C { class D { class E { 
            public void Method() { 
                var x = /* comment
                " + new string('x', 50000) + @" */ 10; 
            } 
        } } } } }";

        VerifyParsingCompletes(sourceCode, csharp, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void JavaScript_UnclosedMultilineComment_DoesNotHang()
    {
        var js = new JavaScript();
        var sourceCode = "/* " + new string('x', 100000); // 100KB unclosed comment

        VerifyParsingCompletes(sourceCode, js, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void Html_UnclosedScriptTag_DoesNotHang()
    {
        var html = new Html();
        var sourceCode = "<script>\n" + new string('x', 100000); // 100KB unclosed script

        VerifyParsingCompletes(sourceCode, html, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void Markdown_MultipleUnclosedCodeBlocks_DoesNotHang()
    {
        var markdown = new Markdown();
        var sourceCode = @"```
unclosed 1
```
``` 
unclosed 2
" + new string('x', 50000) + @"
```
```
unclosed 3
" + new string('x', 50000);

        VerifyParsingCompletes(sourceCode, markdown, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void CSharp_VerbatimStringWithSpecialChars_DoesNotHang()
    {
        var csharp = new CSharp();
        // Verbatim strings can contain unescaped quotes if doubled
        var sourceCode = @"var str = @""Contains ""double"" quotes and 
" + new string('x', 50000) + @" 
more text"";";

        VerifyParsingCompletes(sourceCode, csharp, timeoutMs: 2000);
    }

    [TestMethod]
    [Timeout(3000)]
    public void Mixed_LargeDocumentWithManyLanguageConstructs_DoesNotHang()
    {
        var markdown = new Markdown();
        
        var sourceCode = @"# Large Document Test

```csharp
" + new string('x', 10000) + @"
```

**bold** _italic_ `code`

```javascript
" + new string('y', 10000) + @"
```

Regular paragraph text with /* comment-like */ content.

- List item 1
- List item 2

[Link](http://example.com)

Another code block:
```html
" + new string('z', 10000) + @"
```";

        VerifyParsingCompletes(sourceCode, markdown, timeoutMs: 3000);
    }

    [TestMethod]
    public void ParsingUnderNormalConditions_CompleteSuccessfully()
    {
        var markdown = new Markdown();
        var csharp = new CSharp();
        var js = new JavaScript();
        var html = new Html();

        var normalMarkdown = @"# Title
```csharp
public class Test { }
```
Some **bold** text.";

        var normalCSharp = @"public class MyClass
{
    // Comment
    public void Method() { }
}";

        var normalJs = @"function test() {
    /* Multi-line
       comment */
    const x = 10;
}";

        var normalHtml = @"<!DOCTYPE html>
<html>
<head><title>Test</title></head>
<body><p>Hello</p></body>
</html>";

        VerifyParsingCompletes(normalMarkdown, markdown);
        VerifyParsingCompletes(normalCSharp, csharp);
        VerifyParsingCompletes(normalJs, js);
        VerifyParsingCompletes(normalHtml, html);
    }
}
