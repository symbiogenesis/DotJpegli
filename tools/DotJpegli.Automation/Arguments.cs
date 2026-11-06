namespace DotJpegli.Automation;

using System.Runtime.InteropServices;

internal sealed class Arguments
{
    private readonly Dictionary<string, List<string>> values = [];

    public Arguments(ReadOnlySpan<string> tokens)
    {
        for (int index = 0; index < tokens.Length; index++)
        {
            string token = tokens[index];
            if (!token.StartsWith("--", StringComparison.Ordinal) || token.Length == 2)
            {
                throw new ArgumentException($"Expected an option, got: {token}");
            }

            string name = Name(token);
            List<string> items = CollectionsMarshal.GetValueRefOrAddDefault(this.values, name, out _) ??= [];
            if (index + 1 < tokens.Length && !tokens[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                items.Add(tokens[++index]);
            }
            else
            {
                items.Add(string.Empty);
            }
        }
    }

    public bool Has(string name) => this.values.ContainsKey(Name(name));

    public string Get(string name) => this.Optional(name) ?? throw new ArgumentException($"Missing --{Name(name)}.");

    public string? Optional(string name)
    {
        if (!this.values.TryGetValue(Name(name), out List<string>? items))
        {
            return null;
        }

        if (items.Count != 1 || string.IsNullOrWhiteSpace(items[0]))
        {
            throw new ArgumentException($"--{Name(name)} requires exactly one nonempty value.");
        }

        return items[0];
    }

    public IReadOnlyList<string> Values(string name) => this.values.TryGetValue(Name(name), out List<string>? items) ? items : [];

    public void Validate(ReadOnlySpan<string> valueOptions, ReadOnlySpan<string> switches, string? repeatedOption = null)
    {
        foreach ((string name, List<string> items) in this.values)
        {
            bool isSwitch = switches.Contains(name, StringComparer.Ordinal);
            if (!isSwitch && !valueOptions.Contains(name, StringComparer.Ordinal))
            {
                throw new ArgumentException($"Unknown option: --{name}");
            }

            if ((items.Count != 1 && !string.Equals(name, repeatedOption, StringComparison.Ordinal)) ||
                (isSwitch ? items.Any(value => value.Length != 0) : items.Any(string.IsNullOrWhiteSpace)))
            {
                throw new ArgumentException($"Invalid or repeated option: --{name}");
            }
        }
    }

    private static string Name(string name) => name.StartsWith("--", StringComparison.Ordinal) ? name[2..] : name;
}
