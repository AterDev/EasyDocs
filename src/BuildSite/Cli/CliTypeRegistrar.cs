using Spectre.Console.Cli;

namespace BuildSite.Cli;

internal sealed class CliTypeRegistrar(IServiceProvider serviceProvider) : ITypeRegistrar
{
    public void Register(Type service, Type implementation)
    {
    }

    public void RegisterInstance(Type service, object implementation)
    {
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
    }

    public ITypeResolver Build()
    {
        return new CliTypeResolver(serviceProvider);
    }
}

internal sealed class CliTypeResolver(IServiceProvider serviceProvider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        return type == null ? null : serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
