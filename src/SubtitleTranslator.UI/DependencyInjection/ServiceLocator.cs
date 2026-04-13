using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;
using SubtitleTranslator.Core.Services;
using SubtitleTranslator.Infrastructure.Capture;
using SubtitleTranslator.Infrastructure.Configuration;
using SubtitleTranslator.Infrastructure.Ocr;
using SubtitleTranslator.Infrastructure.Persistence;
using SubtitleTranslator.Infrastructure.Translation;

namespace SubtitleTranslator.UI.DependencyInjection;

/// <summary>
/// Composition root de la aplicación. Único lugar donde se instancian y cablean
/// las dependencias concretas. Los Form y Presenters nunca hacen new() de infraestructura.
/// </summary>
public static class ServiceLocator
{
    private static readonly AppSettingsLoader _settingsLoader = new();
    private static readonly AppSettings _settings;
    private static readonly InMemoryTranslationHistoryRepository _historyRepository;
    private static readonly SubtitleMonitor _subtitleMonitor;

    static ServiceLocator()
    {
        _settings = _settingsLoader.Load();
        _historyRepository = new InMemoryTranslationHistoryRepository();
        _subtitleMonitor = BuildSubtitleMonitor();
    }

    public static AppSettings Settings => _settings;

    public static AppSettingsLoader AppSettingsLoader => _settingsLoader;

    public static SubtitleMonitor SubtitleMonitor => _subtitleMonitor;

    public static ITranslationHistoryRepository HistoryRepository => _historyRepository;

    private static SubtitleMonitor BuildSubtitleMonitor()
    {
        return new SubtitleMonitor(
            screenCapturer:     new WindowsScreenCapturer(),
            ocrEngine:          new WindowsOcrEngine(),
            translationService: new DeepLTranslationService(BuildDeepLHttpClient()),
            changeDetector:     new SubtitleChangeDetector(),
            historyRepository:  _historyRepository,
            settings:           _settings);
    }

    private static HttpClient BuildDeepLHttpClient()
    {
        _settings.ApiKeys.TryGetValue("DeepL", out string? apiKey);

        HttpClient client = new()
        {
            BaseAddress = new Uri("https://api-free.deepl.com/v2/")
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
            client.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {apiKey}");

        return client;
    }
}
