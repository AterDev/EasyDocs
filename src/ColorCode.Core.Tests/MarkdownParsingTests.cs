using ColorCode.Core;
using ColorCode.Core.Compilation.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ColorCode.Core.Tests;

/// <summary>
/// Test cases for Markdown language parsing
/// </summary>
[TestClass]
public class MarkdownParsingTests : LanguageParsingTestBase
{
    private Markdown? _markdown;

    [TestInitialize]
    public new void Initialize()
    {
        base.Initialize();
        _markdown = new Markdown();
    }

    [TestMethod]
    public void ParseSimpleHeading()
    {
        var sourceCode = "# Heading 1\n## Heading 2\n### Heading 3";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseCodeBlock_Fenced()
    {
        var sourceCode = @"```csharp
public class MyClass
{
    public void MyMethod() { }
}
```";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseCodeBlock_Indented()
    {
        var sourceCode = @"Some text:

    public class MyClass
    {
        public void MyMethod() { }
    }

More text";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseCodeBlock_UnclosedFenced_DoesNotHang()
    {
        // This is the critical test for ReDoS prevention
        var sourceCode = @"```csharp
public class MyClass
{
    public void MyMethod() { }
}
// Missing closing backticks - should not hang!

" + new string('x', 10000); // Large content to trigger backtracking

        VerifyParsingCompletes(sourceCode, _markdown!, timeoutMs: 2000);
    }

    [TestMethod]
    public void ParseBoldText()
    {
        var sourceCode = "This is **bold** text and *also bold* text.";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseEmphasisText()
    {
        var sourceCode = "This is _emphasized_ text and __strong emphasis__ text.";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseInlineCode()
    {
        var sourceCode = "Use `var x = 10;` for inline code or ``` ``nested `` ``` for nested code.";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseLinks()
    {
        var sourceCode = @"[Link text](http://example.com)
[Reference link][ref]
![Image alt](http://example.com/image.jpg)
[Image with reference][img-ref]";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseLists()
    {
        var sourceCode = @"* Item 1
* Item 2
* Item 3

+ Ordered 1
+ Ordered 2

- Dash 1
- Dash 2";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseHorizontalRules()
    {
        var sourceCode = @"---

***

===";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseEscapedCharacters()
    {
        var sourceCode = @"Escaped \* asterisk and \[ bracket \]";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseHtmlTags()
    {
        var sourceCode = @"<div>HTML content</div>
<p>Paragraph</p>
&nbsp; &lt; &gt;";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseComplexMarkdownDocument()
    {
        var sourceCode = @"# Main Title

This is a paragraph with **bold** and _italic_ text.

## Code Examples

```csharp
public class Example
{
    public void Method()
    {
        var x = 10;
    }
}
```

### Features

* Feature 1
* Feature 2
* Feature 3

[Visit our website](http://example.com)

---

End of document.";
        VerifyParsingCompletes(sourceCode, _markdown!);
        
        var result = Parse(sourceCode, _markdown!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void HasCorrectAlias()
    {
        Assert.IsTrue(_markdown!.HasAlias("md"));
        Assert.IsTrue(_markdown!.HasAlias("markdown"));
        Assert.IsTrue(_markdown!.HasAlias("MD"));
        Assert.IsTrue(_markdown!.HasAlias("MARKDOWN"));
        Assert.IsFalse(_markdown!.HasAlias("txt"));
    }
}
