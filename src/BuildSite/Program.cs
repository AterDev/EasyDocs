﻿using System.Text;
using Share;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

ShowLogo();
#if DEBUG
Debug();
#endif

string? command = args.FirstOrDefault();

switch (command)
{
    case "init":
        var path = args.Skip(1).FirstOrDefault() ?? Directory.GetCurrentDirectory();
        Command.Init(path);
        break;
    case "build":
        var configPath = args.Skip(1).FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(configPath))
        {
            Command.Build(configPath);
        }
        else
        {
            Command.LogError(Language.Get("buildRequired"));
        }
        break;
    default:
        ShowHelp();
        break;
}

static void ShowHelp()
{
    var helpContent = """
    {0}:
    init [path]
        {1}

    build [contentPath] [outputPath]
        {2}

    """;
    Console.Write(helpContent,
        Language.Get("Command"),
        Language.Get("init"),
        Language.Get("build")
        );
}
static void ShowLogo()
{
    var logo = """
            EasyDocs : The Static Web Builder!
               —→ for freedom 🗽 ←—

            """;

    Console.WriteLine(logo);
}

static void Debug()
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "webinfo.json");
    Command.Build(configPath);
}
