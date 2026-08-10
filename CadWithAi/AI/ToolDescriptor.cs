using Microsoft.Extensions.AI;
using System.Reflection;

namespace CadWithAi.AI;

public sealed class ToolDescriptor
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required MethodInfo Method { get; init; }
    public required object Target { get; init; }
    public required AIFunction Function { get; init; }
}
