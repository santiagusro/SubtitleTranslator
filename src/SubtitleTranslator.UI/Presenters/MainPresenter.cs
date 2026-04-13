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

    public bool IsRunning => _monitor.IsRunning;

    // Proxy del evento del monitor: el form no depende de SubtitleMonitor directamente.
    public event EventHandler<Exception>? MonitorErrorOccurred
    {
        add    => _monitor.ErrorOccurred += value;
        remove => _monitor.ErrorOccurred -= value;
    }

    public MainPresenter(
        SubtitleMonitor monitor,
        AppSettings settings,
        AppSettingsLoader settingsLoader,
        ITranslationHistoryRepository historyRepository)
    {
        _monitor          = monitor;
        _settings         = settings;
        _settingsLoader   = settingsLoader;
        _historyRepository = historyRepository;
    }

    public Task StartAsync(CancellationToken ct = default) => _monitor.StartAsync(ct);

    public void Stop() => _monitor.Stop();

    public void ClearHistory() => _historyRepository.Clear();

    public void SaveSettings() => _settingsLoader.Save(_settings);

    public void ConfigureRegion()
    {
        // TODO: abrir RegionSelectorForm, actualizar _settings.Region, llamar SaveSettings()
    }
}
