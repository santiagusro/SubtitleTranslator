using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Infrastructure.Persistence;

public sealed class InMemoryTranslationHistoryRepository : ITranslationHistoryRepository
{
    private readonly List<SubtitleLine> _lines = new();
    private readonly object _lock = new();

    public void Add(SubtitleLine line)
    {
        lock (_lock)
        {
            _lines.Add(line);
        }
    }

    public IReadOnlyList<SubtitleLine> GetAll()
    {
        lock (_lock)
        {
            return _lines.ToList().AsReadOnly();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _lines.Clear();
        }
    }
}
