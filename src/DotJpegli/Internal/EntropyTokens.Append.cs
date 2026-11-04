namespace DotJpegli.Internal;

internal sealed partial class EntropyTokens
{
    // Worker-local baseline token streams already contain their exact code
    // contexts and restart records. Only the ordered caller touches the shared
    // destination budget; appending can abandon it under the normal cap.
    internal void AppendTo(EntropyTokens destination, CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(destination);
        if (ReferenceEquals(this, destination))
        {
            throw new ArgumentException("A token stream cannot append itself.", nameof(destination));
        }
        if (!this.IsComplete)
        {
            throw new InvalidOperationException("An abandoned token stream cannot be appended.");
        }
        cancellationToken.ThrowIfCancellationRequested();
        for (int segmentIndex = 0; segmentIndex < this.segments.Count && destination.IsComplete; segmentIndex++)
        {
            uint[] segment = this.segments[segmentIndex];
            int length = segmentIndex == this.segments.Count - 1 ? this.currentCount : segment.Length;
            for (int index = 0; index < length && destination.IsComplete; index++)
            {
                if ((index & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                uint token = segment[index];
                if (((token >> ContextShift) & 7) == 7)
                {
                    destination.hasRestart = true;
                }
                destination.Add(token);
            }
        }
    }
}
