namespace SubtitleTranslator.Core.Exceptions;

public sealed class TranslationException : Exception
{
    public TranslationException(string message) : base(message) { }

    public TranslationException(string message, Exception innerException) : base(message, innerException) { }
}
