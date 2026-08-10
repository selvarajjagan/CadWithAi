using System.Reflection;

namespace CadWithAi.AI;

public sealed class ToolRegistry
{
    private readonly IReadOnlyList<ToolServiceDescriptor> _services;

    public ToolRegistry(Assembly applicationAssembly)
    {
        _services = ToolDiscovery.Discover(applicationAssembly);
    }

    public IReadOnlyList<ToolServiceDescriptor> Services => _services;

    public ToolServiceDescriptor? FindService(string serviceName)
    {
        return _services.FirstOrDefault(x =>
            string.Equals(x.Name, serviceName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.ServiceInterface.Name, serviceName, StringComparison.OrdinalIgnoreCase));
    }

    public ToolServiceDescriptor? FindServiceByImplementation(Type implementationType)
    {
        return _services.FirstOrDefault(x => x.ImplementationType == implementationType);
    }
}
