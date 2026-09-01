#:sdk Microsoft.NET.Sdk.Web
// 解析参数：根目录和端口
var baseDir = Directory.GetCurrentDirectory();
var root = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
    ? Path.GetFullPath(args[0])
    : Path.Combine(baseDir, "WebSite");

var port = args.Length > 1 && int.TryParse(args[1], out var p) ? p : 5200;

// 创建并配置应用
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = root,
    WebRootPath = root
});

builder.WebHost.UseUrls($"http://localhost:{port}");

var app = builder.Build();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

await app.RunAsync();