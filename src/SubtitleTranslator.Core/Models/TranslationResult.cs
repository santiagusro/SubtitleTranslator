namespace SubtitleTranslator.Core.Models;

public sealed class TranslationResult
{
    public bool IsSuccess { get; }
    public string TranslatedText { get; }
    public string? ErrorMessage { get; }

    private TranslationResult(bool isSuccess, string translatedText, string? errorMessage)
    {
        IsSuccess = isSuccess;
        TranslatedText = translatedText;
        ErrorMessage = errorMessage;
    }

    public static TranslationResult Success(string translatedText) =>
        new(true, translatedText, null);

    public static TranslationResult Failure(string errorMessage) =>
        new(false, string.Empty, errorMessage);
}
