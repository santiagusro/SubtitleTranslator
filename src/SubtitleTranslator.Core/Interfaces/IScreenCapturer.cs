using System.Drawing;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Core.Interfaces;

public interface IScreenCapturer
{
    Bitmap CaptureRegion(CaptureRegion region);
}
