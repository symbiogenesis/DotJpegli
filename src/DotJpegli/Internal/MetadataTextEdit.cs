namespace DotJpegli.Internal;

// Interpret the public nullable-string convention once, before updating metadata.
internal readonly union MetadataTextEdit(PreserveText, RemoveText, SetText)
{
    public static MetadataTextEdit FromNullable(string? text) => text switch
    {
        null => PreserveText.Instance,
        "" => RemoveText.Instance,
        _ => new SetText(text)
    };

    // IPTC accepts a list: no list preserves the field, an empty list removes it,
    // and a singleton replaces it. Exif and XMP share this same edit projection.
    public string[]? ToValues() => this switch
    {
        PreserveText => null,
        RemoveText => [],
        SetText text => [text.Value],
        null => throw new InvalidOperationException("A metadata text edit must be initialized before use.")
    };
}

internal sealed class PreserveText
{
    public static PreserveText Instance { get; } = new();

    private PreserveText() { }
}

internal sealed class RemoveText
{
    public static RemoveText Instance { get; } = new();

    private RemoveText() { }
}

internal sealed record SetText(string Value);
