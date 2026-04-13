using System.Drawing;
using SubtitleTranslator.Core.Exceptions;
using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Core.Services;

public sealed class SubtitleMonitor
{
    private readonly IScreenCapturer _screenCapturer;
    private readonly IOcrEngine _ocrEngine;
    private readonly ITranslationService _translationService;
    private readonly ISubtitleChangeDetector _changeDetector;
    private readonly ITranslationHistoryRepository _historyRepository;
    private readonly AppSettings _settings;

    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private volatile bool _isRunning;

    public event EventHandler<SubtitleLine>? NewTranslationAvailable;
    public event EventHandler<Exception>? ErrorOccurred;

    public bool IsRunning => _isRunning;

    public SubtitleMonitor(
        IScreenCapturer screenCapturer,
        IOcrEngine ocrEngine,
        ITranslationService translationService,
        ISubtitleChangeDetector changeDetector,
        ITranslationHistoryRepository historyRepository,
        AppSettings settings)
    {
        _screenCapturer = screenCapturer;
        _ocrEngine = ocrEngine;
        _translationService = translationService;
        _changeDetector = changeDetector;
        _historyRepository = historyRepository;
        _settings = settings;
    }

    public Task StartAsync(CancellationToken ct)
    {
        if (_isRunning)
            return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _isRunning = true;
        _loopTask = Task.Run(() => RunLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    public void Stop()
    {
        _cts?.Cancel();
        _changeDetector.Reset();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        string previousText = string.Empty;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                previousText = await ProcessFrameAsync(previousText, ct);
                await Task.Delay(_settings.CaptureIntervalMs, ct);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async Task<string> ProcessFrameAsync(string previousText, CancellationToken ct)
    {
        try
        {
            if (_settings.Region is null)
                return previousText;

            using Bitmap bitmap = _screenCapturer.CaptureRegion(_settings.Region);
            string currentText = await _ocrEngine.ExtractTextAsync(bitmap);

            if (string.IsNullOrWhiteSpace(currentText) || !_changeDetector.HasChanged(previousText, currentText))
                return previousText;

            ct.ThrowIfCancellationRequested();
            await HandleTranslationAsync(currentText, ct);
            return currentText;
        }
        catch (OperationCanceledException)
        {
            throw; 
        }
        catch (OcrException ex)
        {
            OnErrorOccurred(ex);
            return previousText;
        }
        catch (TranslationException ex)
        {
            OnErrorOccurred(ex);
            return previousText;
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
            return previousText;
        }
    }

    private async Task HandleTranslationAsync(string currentText, CancellationToken ct)
    {
        TranslationResult result = await _translationService.TranslateAsync(
            currentText, _settings.TargetLanguage, ct);

        if (!result.IsSuccess)
        {
            OnErrorOccurred(new TranslationException(result.ErrorMessage ?? "Translation failed."));
            return;
        }

        SubtitleLine line = new(
            originalText: currentText,
            translatedText: result.TranslatedText,
            detectedAt: DateTime.UtcNow,
            sourceLanguage: string.Empty,
            targetLanguage: _settings.TargetLanguage);

        _historyRepository.Add(line);
        OnNewTranslationAvailable(line);
    }

    private void OnNewTranslationAvailable(SubtitleLine line) =>
        NewTranslationAvailable?.Invoke(this, line);

    private void OnErrorOccurred(Exception ex) =>
        ErrorOccurred?.Invoke(this, ex);
}
