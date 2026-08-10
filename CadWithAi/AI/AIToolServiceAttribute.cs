using System;

namespace CadWithAi.AI;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class AIToolServiceAttribute : Attribute
{
    public AIToolServiceAttribute(string description)
    {
        Description = description;
    }

    public string Description { get; }
}
