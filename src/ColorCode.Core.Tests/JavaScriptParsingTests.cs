using ColorCode.Core;
using ColorCode.Core.Compilation.Languages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ColorCode.Core.Tests;

/// <summary>
/// Test cases for JavaScript language parsing
/// </summary>
[TestClass]
public class JavaScriptParsingTests : LanguageParsingTestBase
{
    private JavaScript? _javaScript;

    [TestInitialize]
    public new void Initialize()
    {
        base.Initialize();
        _javaScript = new JavaScript();
    }

    [TestMethod]
    public void ParseSimpleFunction()
    {
        var sourceCode = @"function greet(name) {
    return 'Hello, ' + name;
}";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseSingleLineComment()
    {
        var sourceCode = "// This is a comment\nvar x = 10; // inline comment";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseMultiLineComment()
    {
        var sourceCode = @"/* This is a
           multiline
           comment */
var x = 10;";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseUnclosedMultiLineComment_DoesNotHang()
    {
        // Critical test for ReDoS prevention
        var sourceCode = @"/* This comment is not closed
var x = 10;
" + new string('x', 10000); // Large content

        VerifyParsingCompletes(sourceCode, _javaScript!, timeoutMs: 2000);
    }

    [TestMethod]
    public void ParseStrings()
    {
        var sourceCode = @"var str1 = 'Single quotes';
var str2 = ""Double quotes"";
var str3 = `Template literal`;";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseKeywords()
    {
        var sourceCode = @"const x = 10;
let y = 20;
var z = 30;
if (x > 5) {
    for (let i = 0; i < 10; i++) {
        console.log(i);
    }
}";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseArrowFunctions()
    {
        var sourceCode = @"const add = (a, b) => a + b;
const greet = name => 'Hello, ' + name;
const log = () => console.log('test');";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseObjectLiterals()
    {
        var sourceCode = @"const obj = {
    name: 'John',
    age: 30,
    city: 'New York',
    greet: function() {
        return 'Hello ' + this.name;
    }
};";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ParseComplexJavaScript()
    {
        var sourceCode = @"// Module for handling data processing
class DataProcessor {
    /* Initialize the processor */
    constructor(config) {
        this.config = config;
    }

    // Process data
    process(data) {
        return data.map(item => {
            // Transform each item
            return {
                id: item.id,
                value: item.value * 2
            };
        });
    }
}

// Export the class
module.exports = DataProcessor;";
        VerifyParsingCompletes(sourceCode, _javaScript!);
        
        var result = Parse(sourceCode, _javaScript!);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void HasCorrectAlias()
    {
        Assert.IsTrue(_javaScript!.HasAlias("js"));
        Assert.IsTrue(_javaScript!.HasAlias("JS"));
        Assert.IsFalse(_javaScript!.HasAlias("javascript"));
    }
}
