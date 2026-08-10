namespace CadWithAi.AI;

public sealed class ToolServiceDescriptor
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Type ServiceInterface { get; init; }
    public required Type ImplementationType { get; init; }
    public required object Instance { get; init; }
    public required IReadOnlyList<ToolDescriptor> Tools { get; init; }
}
