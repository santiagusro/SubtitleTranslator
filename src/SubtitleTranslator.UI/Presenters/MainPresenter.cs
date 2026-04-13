using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;
using SubtitleTranslator.Core.Services;
using SubtitleTranslator.Infrastructure.Configuration;

namespace SubtitleTranslator.UI.Presenters;


public sealed class MainPresenter
{
    private readonly SubtitleMonitor _monitor;
    private readonly AppSettings _settings;
    private readonly AppSettingsLoader _settingsLoader;
    private readonly ITranslationHistoryRepository _historyRepository;

    public MainPresenter(
        SubtitleMonitor monitor,
        AppSettings settings,
        AppSettingsLoader settingsLoader,
        ITranslationHistoryRepository historyRepository)
    {
        _monitor = monitor;
        _settings = settings;
        _settingsLoader = settingsLoader;
        _historyRepository = historyRepository;
    }
}
