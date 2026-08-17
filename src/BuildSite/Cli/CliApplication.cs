using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal static class CliApplication
{
    public static int Run(string[] args)
    {
        CliHeader.Write();

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("ezdoc");
            config.SetApplicationVersion(CliHeader.Version);
            config.SetApplicationCulture(Language.CurrentCulture);

            config.AddCommand<InitCommand>("init")
                .WithAlias("初始化")
                .WithDescription(Language.GetBilingual("initDescription"))
                .WithExample("init", "./site")
                .WithExample("初始化", "./site");

            config.AddCommand<BuildCommand>("build")
                .WithAlias("生成")
                .WithDescription(Language.GetBilingual("buildDescription"))
                .WithExample("build", "./webinfo.json")
                .WithExample("生成", "./webinfo.json");
        });

        return app.Run(args);
    }
}
