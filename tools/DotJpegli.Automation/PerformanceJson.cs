namespace DotJpegli.Automation;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static class PerformanceJson
{
    internal static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    internal static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    internal static void Fields(JsonNode? value, params ReadOnlySpan<string> names)
    {
        Require(value is JsonObject, "Expected a JSON object");
        foreach (string name in names)
        {
            Require(((JsonObject)value!).ContainsKey(name), $"Missing evidence field: {name}");
        }
    }

    internal static bool Integer(JsonNode? value)
    {
        if (value?.GetValueKind() != JsonValueKind.Number)
        {
            return false;
        }

        string raw = value.ToJsonString();
        return !raw.Contains('.') && !raw.Contains('e') && !raw.Contains('E');
    }

    internal static double Number(JsonNode? value)
    {
        if (value?.GetValueKind() != JsonValueKind.Number ||
            !double.TryParse(value.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
        {
            throw new InvalidOperationException("Expected a JSON number");
        }

        return result;
    }

    internal static double Positive(JsonNode? value, string label)
    {
        Require(value?.GetValueKind() == JsonValueKind.Number, $"{label} must be finite and positive");
        double result = Number(value);
        Require(double.IsFinite(result) && result > 0, $"{label} must be finite and positive");
        return result;
    }

    internal static string Text(JsonNode? value)
    {
        Require(value?.GetValueKind() == JsonValueKind.String, "Expected a JSON string");
        return value!.GetValue<string>();
    }

    internal static bool True(JsonNode? value) => value?.GetValueKind() == JsonValueKind.True;

    internal static bool False(JsonNode? value) => value?.GetValueKind() == JsonValueKind.False;

    internal static bool Equal(JsonNode? left, JsonNode? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        if (left is JsonObject first)
        {
            return right is JsonObject second && first.Count == second.Count &&
                first.All(pair => second.TryGetPropertyValue(pair.Key, out JsonNode? value) && Equal(pair.Value, value));
        }

        if (left is JsonArray array)
        {
            return right is JsonArray other && array.Count == other.Count &&
                array.Zip(other).All(pair => Equal(pair.First, pair.Second));
        }

        return JsonNode.DeepEquals(left, right);
    }

    internal static JsonNode Read(string path) => JsonNode.Parse(File.ReadAllText(path)) ?? throw new InvalidOperationException($"Empty JSON document: {path}");

    internal static void Write(string path, JsonNode value) => File.WriteAllText(path, value.ToJsonString(Indented) + "\n");

    internal static void Rejected(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or JsonException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }
}
