using System;

namespace CadWithAi.AI;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AIToolIgnoreAttribute : Attribute
{
}
