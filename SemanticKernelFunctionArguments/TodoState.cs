using Microsoft.SemanticKernel;

public class TodoState
{

    public Dictionary<string, List<string>> todos = new Dictionary<string, List<string>>()
    {
        ["shopping"] = ["bread", "milk", "eggs", "bananas"]
    };
    public string[] add_todos(string[] items, string scope)
    {
        if (!todos.TryGetValue(scope, out var result))
            todos[scope] = result = [];
        Console.WriteLine($"    @add_todos({string.Join(", ", items)}, {scope})");
        result.AddRange(items.Except(result));
        return [.. result];
    }
    public int remove_todos(string[] items, string scope)
    {
        if (todos.TryGetValue(scope, out var result))
            return result.RemoveAll(items.Contains);
        Console.WriteLine($"    @remove_todos({string.Join(", ", items)}, {scope})");
        return 0;
    }

    public string[] get_todos(string scope)
    {
        Console.WriteLine($"    @get_todos({scope})");
        if (todos.TryGetValue(scope, out var result))
            return [.. result];
        return [];
    }

    public string[] get_scopes()
    {
        Console.WriteLine($"    @get_scopes()");
        return [.. todos.Keys];
    }

}
