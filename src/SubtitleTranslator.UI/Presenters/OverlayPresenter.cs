using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;
using SubtitleTranslator.Core.Services;

namespace SubtitleTranslator.UI.Presenters;

public sealed class OverlayPresenter
{
    private readonly SubtitleMonitor _monitor;
    private readonly ITranslationHistoryRepository _historyRepository;

    // El form se suscribe a este evento para actualizar el RichTextBox en el UI thread.
    public event EventHandler<SubtitleLine>? TranslationReceived;

    public OverlayPresenter(
        SubtitleMonitor monitor,
        ITranslationHistoryRepository historyRepository)
    {
        _monitor           = monitor;
        _historyRepository = historyRepository;
        _monitor.NewTranslationAvailable += OnNewTranslationAvailable;
    }

    public IReadOnlyList<SubtitleLine> GetHistory() => _historyRepository.GetAll();

    public void Unsubscribe() =>
        _monitor.NewTranslationAvailable -= OnNewTranslationAvailable;

    private void OnNewTranslationAvailable(object? sender, SubtitleLine line) =>
        TranslationReceived?.Invoke(this, line);
}
