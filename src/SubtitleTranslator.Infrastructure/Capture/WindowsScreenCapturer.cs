using System.Drawing;
using SubtitleTranslator.Core.Exceptions;
using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Infrastructure.Capture;

public sealed class WindowsScreenCapturer : IScreenCapturer
{
    public Bitmap CaptureRegion(CaptureRegion region)
    {
        try
        {
            Bitmap bitmap = new(region.Width, region.Height);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(
                sourceX: region.X,
                sourceY: region.Y,
                destinationX: 0,
                destinationY: 0,
                blockRegionSize: new Size(region.Width, region.Height),
                copyPixelOperation: CopyPixelOperation.SourceCopy);

            return bitmap;
        }
        catch (Exception ex)
        {
            throw new OcrException(
                $"Screen capture failed for region ({region.X},{region.Y} {region.Width}x{region.Height}): {ex.Message}",
                ex);
        }
    }
}
