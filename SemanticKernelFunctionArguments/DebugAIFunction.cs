
using Microsoft.Extensions.AI;
using System.Reflection;
using System.Text;
using System.Text.Json;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


public class DebugAIFunction(AIFunction wrapped) : AIFunction
{
    private readonly AIFunction wrapped = wrapped;

    public override IReadOnlyDictionary<string, object?> AdditionalProperties => wrapped.AdditionalProperties;
    public override string Description => wrapped.Description;
    public override JsonElement JsonSchema => wrapped.JsonSchema;
    public override JsonSerializerOptions JsonSerializerOptions => wrapped.JsonSerializerOptions;
    public override string Name => wrapped.Name;
    public override MethodInfo? UnderlyingMethod => wrapped.UnderlyingMethod;
    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        if (arguments.Keys != null && arguments.Values != null)
            foreach (var (k, v) in arguments.Keys.Zip(arguments.Values))
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(k);
                sb.Append(": ");
                sb.Append("(");
                sb.Append(v.GetType().Name);
                sb.Append(")");
                sb.Append(JsonSerializer.Serialize(v, JsonSerializerOptions));
            }
        Console.WriteLine($"  DBG: {wrapped.Name}({sb})");
        return await wrapped.InvokeAsync(arguments, cancellationToken);
    }

    public override bool Equals(object? obj) => obj is DebugAIFunction dbg && Equals(dbg);
    public bool Equals(DebugAIFunction other) => wrapped.Equals(other.wrapped);
    public override int GetHashCode() => wrapped.GetHashCode();
}