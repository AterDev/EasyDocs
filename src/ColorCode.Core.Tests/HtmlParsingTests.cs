using ColorCode.Core;
using ColorCode.Core.Compilation.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ColorCode.Core.Tests;

/// <summary>
/// Test cases for HTML language parsing
/// </summary>
[TestClass]
public class HtmlParsingTests : LanguageParsingTestBase
{
    private Html? _html;

    [TestInitialize]
    public new void Initialize()
    {
        base.Initialize();
        _html = new Html();
    }

    [TestMethod]
    public void ParseBasicHtmlDocument()
    {
        var sourceCode = @"<!DOCTYPE html>
<html>
<head>
    <title>Test Page</title>
</head>
<body>
    <h1>Hello World</h1>
    <p>This is a test page.</p>
</body>
</html>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseHtmlComment()
    {
        var sourceCode = @"<!-- This is a comment -->
<div>Content</div>
<!-- Multi-line comment
     with more text -->";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseHtmlAttributes()
    {
        var sourceCode = @"<div class=""container"" id=""main"" data-value=""test"">
    <a href=""http://example.com"" title=""Example"">Link</a>
    <img src=""image.jpg"" alt=""An image"" width=""100"" height=""100"">
</div>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseScriptTag()
    {
        var sourceCode = @"<script type=""text/javascript"">
    function myFunction() {
        console.log('Hello');
    }
    var x = 10;
</script>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseInlineStyle()
    {
        var sourceCode = @"<style>
    .container { 
        width: 100%;
        background-color: #fff;
    }
    body { font-family: Arial; }
</style>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseHtmlEntities()
    {
        var sourceCode = @"<p>
    Special characters: &lt; &gt; &amp; &quot; &#39; &nbsp;
    Math symbols: &#8800; &#8804; &#8805;
</p>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseNestedTags()
    {
        var sourceCode = @"<div class=""wrapper"">
    <section>
        <article>
            <header>
                <h1>Title</h1>
            </header>
            <main>
                <p>Content with <strong>bold</strong> and <em>italic</em>.</p>
            </main>
        </article>
    </section>
</div>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseSelfClosingTags()
    {
        var sourceCode = @"<div>
    <br/>
    <hr />
    <img src=""image.jpg"" />
    <input type=""text"" placeholder=""Enter text"">
    <meta charset=""UTF-8"">
</div>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseComplexHtmlPage()
    {
        var sourceCode = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>EasyDocs</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
        .header { background-color: #333; color: white; padding: 10px; }
    </style>
</head>
<body>
    <!-- Main Header -->
    <header class=""header"">
        <h1>Welcome to EasyDocs</h1>
        <nav>
            <a href=""/"">Home</a> | 
            <a href=""/docs"">Documentation</a>
        </nav>
    </header>

    <!-- Main Content -->
    <main>
        <article>
            <h2>Getting Started</h2>
            <p>This is a <strong>documentation</strong> generator.</p>
        </article>
    </main>

    <!-- Footer -->
    <footer>
        <p>&copy; 2024 EasyDocs. All rights reserved.</p>
    </footer>

    <script>
        console.log('Page loaded');
    </script>
</body>
</html>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseInvalidButCommonHtml()
    {
        // Real-world HTML is often not perfectly formed
        var sourceCode = @"<div>
    <p>Unclosed paragraph
    <div>
        <p>Nested paragraph</p>
    </div>
    <br>
    <img src=""test.jpg"">
</div>";
        VerifyParsingCompletes(sourceCode, _html!);
        
        var result = Parse(sourceCode, _html!);
        Assert.IsNotNull(result);
    }
}
