using System.Text.RegularExpressions;
using SubtitleTranslator.Core.Interfaces;

namespace SubtitleTranslator.Core.Services;

public sealed class SubtitleChangeDetector : ISubtitleChangeDetector
{
    private static readonly Regex WhitespacePattern = new(@"\s+", RegexOptions.Compiled);

    public bool HasChanged(string previousText, string currentText)
    {
        string normalizedPrevious = Normalize(previousText);
        string normalizedCurrent = Normalize(currentText);
        return !string.Equals(normalizedPrevious, normalizedCurrent, StringComparison.Ordinal);
    }

    public void Reset() { }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return WhitespacePattern.Replace(text.Trim(), " ");
    }
}
