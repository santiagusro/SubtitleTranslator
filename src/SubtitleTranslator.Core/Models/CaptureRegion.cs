namespace SubtitleTranslator.Core.Models;

public sealed record CaptureRegion
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    private CaptureRegion() { }

    public static CaptureRegion Create(int x, int y, int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");

        return new CaptureRegion { X = x, Y = y, Width = width, Height = height };
    }

    public bool IsValid() => Width > 0 && Height > 0;
}
