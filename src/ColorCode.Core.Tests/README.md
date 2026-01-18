# ColorCode.Core 单元测试指南

## 项目概述

`ColorCode.Core.Tests` 是一个基于 Microsoft Test Platform (MSTest) 的单元测试项目，用于验证各种编程语言的语法高亮解析功能。

## 测试覆盖范围

### 1. **MarkdownParsingTests** - Markdown 解析测试
- 标题、代码块（围栏和缩进）
- 加粗、斜体、内联代码
- 链接、列表、水平线
- 转义字符、HTML 标签、HTML 实体
- **关键**：未闭合代码块的 ReDoS 防护

### 2. **CSharpParsingTests** - C# 解析测试
- 简单类、命名空间、using 声明
- 单行和多行注释、XML 文档注释
- 字符串（普通、逐字）
- 泛型类型
- **关键**：未闭合注释的 ReDoS 防护

### 3. **JavaScriptParsingTests** - JavaScript 解析测试
- 函数声明、箭头函数
- 单行和多行注释
- 字符串（单引号、双引号、模板字面量）
- 关键字、对象字面量
- **关键**：未闭合注释的 ReDoS 防护

### 4. **HtmlParsingTests** - HTML 解析测试
- 基本文档结构、DOCTYPE
- 注释、属性
- Script 和 Style 标签
- 实体、嵌套标签、自闭合标签
- 复杂页面和现实世界 HTML

### 5. **RegexDenialOfServicePreventionTests** - ReDoS 防护测试
这是最关键的测试集，专门验证正则表达式的灾难性回溯防护：

- **Markdown_UnclosedCodeBlock_DoesNotHang** - 100KB 未闭合代码块
- **CSharp_UnclosedMultilineComment_DoesNotHang** - 100KB 未闭合注释
- **JavaScript_UnclosedMultilineComment_DoesNotHang** - 100KB 未闭合注释
- **Html_UnclosedScriptTag_DoesNotHang** - 100KB 未闭合脚本标签
- **Mixed_LargeDocumentWithManyLanguageConstructs_DoesNotHang** - 综合压力测试

每个 ReDoS 测试都有 `[Timeout(3000)]` 属性，确保在 3 秒内完成。

## 运行测试

### 使用 Visual Studio

1. 在 Visual Studio 中打开解决方案
2. 右键点击 `ColorCode.Core.Tests` 项目
3. 选择 **"Run Tests"** 或 **"Run Tests"** (Ctrl+R, A)
4. 查看 **Test Explorer** 窗口中的结果

### 使用 dotnet CLI

```bash
# 运行所有测试
dotnet test src/ColorCode.Core.Tests/ColorCode.Core.Tests.csproj

# 运行特定测试类
dotnet test src/ColorCode.Core.Tests/ColorCode.Core.Tests.csproj --filter "ClassName=ColorCode.Core.Tests.RegexDenialOfServicePreventionTests"

# 运行特定测试方法
dotnet test src/ColorCode.Core.Tests/ColorCode.Core.Tests.csproj --filter "Name=Markdown_UnclosedCodeBlock_DoesNotHang"

# 显示详细输出
dotnet test src/ColorCode.Core.Tests/ColorCode.Core.Tests.csproj --verbosity detailed

# 生成覆盖率报告
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test src/ColorCode.Core.Tests/ColorCode.Core.Tests.csproj
```

## 关键改进

### 1. 正则表达式超时保护

在 `LanguageCompiler.cs` 中添加了 5 秒的正则表达式超时：

```csharp
private const int RegexTimeoutMs = 5000; // 5 second timeout

private static void CompileRules(IList<LanguageRule> rules, out Regex regex, out IList<string> captures)
{
    // ...
    regex = new Regex(regexBuilder.ToString(), RegexOptions.Compiled, 
        TimeSpan.FromMilliseconds(RegexTimeoutMs));
}
```

这防止了灾难性回溯导致的应用程序挂起。

### 2. 修复的正则表达式

已修复以下文件中的 ReDoS 漏洞：

| 文件 | 修复 | 原因 |
|------|------|------|
| Markdown.cs | `((?:(?!^```+)[\s\S])*?)` | 避免无限回溯 |
| CSharp.cs | `/\*(?:[^*]\|\*(?!/))*\*/` | 移除嵌套量词 |
| JavaScript.cs | `/\*(?:[^*]\|\*(?!/))*\*/` | 移除嵌套量词 |
| Html.cs | `((?:(?!</script>).)*?)` | 使用前向断言终止 |

## 测试基础设施

### LanguageParsingTestBase

所有语言测试类继承自 `LanguageParsingTestBase`，提供：

- `LanguageCompiler` 和 `LanguageParser` 实例
- `Parse(sourceCode, language)` - 解析源代码
- `VerifyParsingCompletes(sourceCode, language, timeoutMs)` - 验证解析在指定时间内完成
- `GetLanguage(languageId)` - 获取语言实例

### 测试约定

```csharp
[TestClass]
public class LanguageXTests : LanguageParsingTestBase
{
    private LanguageX? _language;

    [TestInitialize]
    public new void Initialize()
    {
        base.Initialize();
        _language = new LanguageX();
    }

    [TestMethod]
    public void TestScenarioName()
    {
        var sourceCode = "...";
        VerifyParsingCompletes(sourceCode, _language!);
        
        var result = Parse(sourceCode, _language!);
        Assert.IsNotNull(result);
    }
}
```

## 性能基准

运行所有测试的预期时间（在标准硬件上）：

- **正常测试**: ~500-1000ms
- **ReDoS 防护测试**: ~2000-3000ms (由于超时属性)
- **总计**: ~3-5 秒

## 扩展测试

要添加新的语言测试：

1. 创建新文件 `LanguageXParsingTests.cs`
2. 继承 `LanguageParsingTestBase`
3. 添加 `[TestClass]` 属性
4. 实现构造和初始化
5. 添加测试方法：
   - 至少 5 个正常场景测试
   - 至少 1 个 ReDoS 防护测试
   - 1 个复杂综合测试

## 故障排查

### 测试超时

如果测试超时，可能表示：
- 正则表达式存在灾难性回溯
- 输入数据过大
- 系统资源不足

检查 `RegexMatchTimeoutException` 异常日志。

### 构建失败

确保：
- 已安装 .NET 10 SDK
- 所有依赖项已还原：`dotnet restore`
- ColorCode.Core 项目已编译

### 测试不稳定

如果测试在某些机器上失败而在其他机器上通过：
- 调整 `[Timeout]` 属性值
- 检查系统资源
- 查看 `timeoutMs` 参数

## 相关链接

- [MSTest 官方文档](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
- [.NET 测试最佳实践](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [ReDoS 防护指南](https://cheatsheetseries.owasp.org/cheatsheets/Regular_Expression_Denial_of_Service_RegEx_DoS.html)
