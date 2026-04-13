using System.Drawing;

namespace SubtitleTranslator.Core.Interfaces;

public interface IOcrEngine
{
    Task<string> ExtractTextAsync(Bitmap image);
}
