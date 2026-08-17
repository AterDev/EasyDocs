using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share;
using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal static class CliApplication
{
    public static int Run(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLocalization();
        builder.Services.AddScoped<Localizer>();
        builder.Services.AddScoped<InitCommand>();
        builder.Services.AddScoped<BuildCommand>();

        using var host = builder.Build();
        var localizer = host.Services.GetRequiredService<Localizer>();

        CliHeader.Write();

        var app = new CommandApp(new CliTypeRegistrar(host.Services));
        app.Configure(config =>
        {
            config.SetApplicationName("ezdoc");
            config.SetApplicationVersion(CliHeader.Version);
            config.SetApplicationCulture(CultureInfo.CurrentUICulture);

            config.AddCommand<InitCommand>("init")
                .WithDescription(localizer.Get(Localizer.InitDescription))
                .WithExample("init", "./site");

            config.AddCommand<BuildCommand>("build")
                .WithDescription(localizer.Get(Localizer.BuildDescription))
                .WithExample("build", "./webinfo.json");
        });

        return app.Run(args);
    }
}
