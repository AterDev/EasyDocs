using ColorCode.Core;
using ColorCode.Core.Compilation;
using ColorCode.Core.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ColorCode.Core.Tests;

/// <summary>
/// Base test class for language parsing tests
/// </summary>
[TestClass]
public abstract class LanguageParsingTestBase
{
    protected ILanguageCompiler LanguageCompiler { get; private set; } = null!;
    protected ILanguageParser LanguageParser { get; private set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Access internal static fields via reflection
        var languagesType = typeof(Languages);
        var compiledLanguagesField = languagesType.GetField("CompiledLanguages", 
            BindingFlags.Static | BindingFlags.NonPublic);
        var compileLockField = languagesType.GetField("CompileLock", 
            BindingFlags.Static | BindingFlags.NonPublic);
        var languageRepositoryField = languagesType.GetField("LanguageRepository",
            BindingFlags.Static | BindingFlags.NonPublic);

        var compiledLanguages = (Dictionary<string, CompiledLanguage>)compiledLanguagesField!.GetValue(null)!;
        var compileLock = (System.Threading.ReaderWriterLockSlim)compileLockField!.GetValue(null)!;
        var languageRepository = (Common.ILanguageRepository)languageRepositoryField!.GetValue(null)!;

        LanguageCompiler = new LanguageCompiler(compiledLanguages, compileLock);
        LanguageParser = new LanguageParser(LanguageCompiler, languageRepository);
    }

    /// <summary>
    /// Parses the given source code with the specified language
    /// </summary>
    protected List<Scope> Parse(string sourceCode, ILanguage language)
    {
        ArgumentNullException.ThrowIfNull(sourceCode);
        ArgumentNullException.ThrowIfNull(language);

        var scopes = new List<Scope>();

        LanguageParser.Parse(sourceCode, language, (text, scopeList) =>
        {
            if (scopeList != null)
                scopes.AddRange(scopeList);
        });

        return scopes;
    }

    /// <summary>
    /// Verifies that parsing completes within a reasonable time
    /// </summary>
    protected void VerifyParsingCompletes(string sourceCode, ILanguage language, int timeoutMs = 5000)
    {
        ArgumentNullException.ThrowIfNull(sourceCode);
        ArgumentNullException.ThrowIfNull(language);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = Parse(sourceCode, language);
            sw.Stop();

            Assert.IsNotNull(result, "Parsing should return a result");
            Assert.IsTrue(sw.ElapsedMilliseconds < timeoutMs,
                $"Parsing took {sw.ElapsedMilliseconds}ms, expected < {timeoutMs}ms");
        }
        catch (RegexMatchTimeoutException ex)
        {
            Assert.Fail($"Regex timeout after {sw.ElapsedMilliseconds}ms: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the language instance by ID
    /// </summary>
    protected ILanguage GetLanguage(string languageId)
    {
        var language = Languages.FindById(languageId);
        Assert.IsNotNull(language, $"Language with ID '{languageId}' not found");
        return language;
    }
}
