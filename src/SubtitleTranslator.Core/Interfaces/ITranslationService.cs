using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Core.Interfaces;

public interface ITranslationService
{
    Task<TranslationResult> TranslateAsync(string text, string targetLanguage, CancellationToken ct = default);
    string ProviderName { get; }
}
