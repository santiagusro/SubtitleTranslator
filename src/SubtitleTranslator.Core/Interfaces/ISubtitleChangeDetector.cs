namespace SubtitleTranslator.Core.Interfaces;

public interface ISubtitleChangeDetector
{
    bool HasChanged(string previousText, string currentText);
    void Reset();
}
