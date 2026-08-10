using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Reflection;

namespace CadWithAi.AI;

public static class ToolDiscovery
{
    public static IReadOnlyList<ToolServiceDescriptor> Discover(Assembly assembly)
    {
        var result = new List<ToolServiceDescriptor>();

        foreach (Type serviceInterface in assembly.GetTypes()
                     .Where(t => t.IsInterface && t.GetCustomAttribute<AIToolServiceAttribute>() is not null))
        {
            Type? implementationType = assembly.GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface && serviceInterface.IsAssignableFrom(t));

            if (implementationType is null)
                continue;

            object? instance = Activator.CreateInstance(implementationType);
            if (instance is null)
                continue;

            AIToolServiceAttribute serviceAttribute = serviceInterface.GetCustomAttribute<AIToolServiceAttribute>()!;

            var tools = new List<ToolDescriptor>();

            foreach (MethodInfo method in serviceInterface.GetMethods())
            {
                if (method.IsSpecialName || method.GetCustomAttribute<AIToolIgnoreAttribute>() is not null)
                    continue;

                DescriptionAttribute? description = method.GetCustomAttribute<DescriptionAttribute>();
                if (description is null)
                    continue;

                string toolName = method.Name.EndsWith("Async", StringComparison.Ordinal)
                    ? method.Name[..^5]
                    : method.Name;

                AIFunction function = AIFunctionFactory.Create(
                    method,
                    instance,
                    toolName,
                    description.Description);

                tools.Add(new ToolDescriptor
                {
                    Name = toolName,
                    Description = description.Description,
                    Method = method,
                    Target = instance,
                    Function = function
                });
            }

            if (tools.Count == 0)
                continue;

            result.Add(new ToolServiceDescriptor
            {
                Name = serviceInterface.Name.StartsWith("I", StringComparison.Ordinal) && serviceInterface.Name.Length > 1
                    ? serviceInterface.Name[1..]
                    : serviceInterface.Name,
                Description = serviceAttribute.Description,
                ServiceInterface = serviceInterface,
                ImplementationType = implementationType,
                Instance = instance,
                Tools = tools
            });
        }

        return result;
    }
}
