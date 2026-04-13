namespace SubtitleTranslator.Core.Models;

public sealed class AppSettings
{
    public CaptureRegion? Region { get; set; }
    public string TargetLanguage { get; set; } = "ES";
    public int CaptureIntervalMs { get; set; } = 500;
    public string TranslationProviderName { get; set; } = string.Empty;
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}
