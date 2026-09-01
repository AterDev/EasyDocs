using Microsoft.Extensions.Localization;

namespace Share;

/// <summary>
/// CLI 本地化资源访问器。
/// </summary>
public partial class Localizer(IStringLocalizer<Localizer> localizer)
{
    public const string InitDescription = nameof(InitDescription);
    public const string BuildDescription = nameof(BuildDescription);
    public const string PathArgument = nameof(PathArgument);
    public const string ConfigPathArgument = nameof(ConfigPathArgument);
    public const string BuildRequired = nameof(BuildRequired);
    public const string InitSuccess = nameof(InitSuccess);
    public const string PreviewSuccess = nameof(PreviewSuccess);
    public const string ConfigExists = nameof(ConfigExists);
    public const string PreviewExists = nameof(PreviewExists);
    public const string InitNextSteps = nameof(InitNextSteps);
    public const string NotExistWebInfo = nameof(NotExistWebInfo);

    public string Get(string key, params object[] arguments)
    {
        return localizer[key, arguments];
    }
}
