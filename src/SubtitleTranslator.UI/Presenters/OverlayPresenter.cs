using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Services;

namespace SubtitleTranslator.UI.Presenters;


public sealed class OverlayPresenter
{
    private readonly SubtitleMonitor _monitor;
    private readonly ITranslationHistoryRepository _historyRepository;

    public OverlayPresenter(
        SubtitleMonitor monitor,
        ITranslationHistoryRepository historyRepository)
    {
        _monitor = monitor;
        _historyRepository = historyRepository;
    }
}
