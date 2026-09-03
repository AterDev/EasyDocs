using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Share.Builders;

namespace Share.Tests;

[TestClass]
public class RepositoryLinkTests
{
    [TestMethod]
    public void NormalizeRepositoryUrl_RemovesCredentialsAndGitSuffix()
    {
        var result = TestBuilder.Normalize("http://gitlab-ci-token:canary@172.16.234.154:10008/data-analysis/docs.git");

        Assert.AreEqual("http://172.16.234.154:10008/data-analysis/docs", result);
    }

    [TestMethod]
    public void NormalizeRepositoryUrl_ConvertsSshRemoteToWebUrl()
    {
        var result = TestBuilder.Normalize("git@gitlab.com:data-analysis/docs.git");

        Assert.AreEqual("https://gitlab.com/data-analysis/docs", result);
    }

    private sealed class TestBuilder : BaseBuilder
    {
        private TestBuilder() : base(new WebInfo())
        {
        }

        public static string? Normalize(string? repositoryUrl) => NormalizeRepositoryUrl(repositoryUrl);
    }
}
