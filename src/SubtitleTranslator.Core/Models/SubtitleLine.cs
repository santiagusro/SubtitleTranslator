namespace SubtitleTranslator.Core.Models;

public sealed record SubtitleLine
{
    public string OriginalText { get; }
    public string TranslatedText { get; }
    public DateTime DetectedAt { get; }
    public string SourceLanguage { get; }
    public string TargetLanguage { get; }

    public SubtitleLine(
        string originalText,
        string translatedText,
        DateTime detectedAt,
        string sourceLanguage,
        string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(originalText))
            throw new ArgumentException("OriginalText cannot be empty.", nameof(originalText));

        OriginalText = originalText;
        TranslatedText = translatedText;
        DetectedAt = detectedAt;
        SourceLanguage = sourceLanguage;
        TargetLanguage = targetLanguage;
    }
}
