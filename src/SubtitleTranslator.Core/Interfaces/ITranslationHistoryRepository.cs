using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Core.Interfaces;

public interface ITranslationHistoryRepository
{
    void Add(SubtitleLine line);
    IReadOnlyList<SubtitleLine> GetAll();
    void Clear();
}
