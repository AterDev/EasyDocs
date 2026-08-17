using BuildSite.Cli;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var cultureName = Environment.GetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE");
var systemCulture = !string.IsNullOrWhiteSpace(cultureName)
    ? new CultureInfo(cultureName)
    : CultureInfo.CurrentUICulture;

CultureInfo.DefaultThreadCurrentCulture = systemCulture;
CultureInfo.DefaultThreadCurrentUICulture = systemCulture;
CultureInfo.CurrentCulture = systemCulture;
CultureInfo.CurrentUICulture = systemCulture;

return CliApplication.Run(args);
