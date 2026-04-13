using System.Drawing;
using System.Drawing.Imaging;
using SubtitleTranslator.Core.Exceptions;
using SubtitleTranslator.Core.Interfaces;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using WinOcrEngine = Windows.Media.Ocr.OcrEngine;

namespace SubtitleTranslator.Infrastructure.Ocr;

public sealed class WindowsOcrEngine : IOcrEngine
{
    private readonly WinOcrEngine _ocrEngine;

    public WindowsOcrEngine()
    {
        _ocrEngine = WinOcrEngine.TryCreateFromUserProfileLanguages()
            ?? throw new OcrException(
                "No hay un idioma OCR disponible para el perfil de usuario actual. " +
                "Instalá un paquete de idioma en la configuración de Windows.");
    }

    public async Task<string> ExtractTextAsync(Bitmap image)
    {
        try
        {
            using Bitmap preprocessed = PreprocessForOcr(image);
            using SoftwareBitmap softwareBitmap = await ConvertToSoftwareBitmapAsync(preprocessed);
            OcrResult result = await _ocrEngine.RecognizeAsync(softwareBitmap);
            return result.Text;
        }
        catch (OcrException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OcrException($"OCR text extraction failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Convierte la imagen a escala de grises y aumenta el contraste para mejorar
    /// la precisión del OCR en subtítulos sobre fondos de video complejos.
    /// </summary>
    private static Bitmap PreprocessForOcr(Bitmap source)
    {
        Bitmap result = new(source.Width, source.Height, PixelFormat.Format32bppArgb);

        // Coeficientes ITU-R BT.601 para grayscale + boost de contraste (factor 1.3, offset -0.15)
        ColorMatrix colorMatrix = new(new float[][]
        {
            new[] { 0.3887f, 0.3887f, 0.3887f, 0f, 0f },  // R → gris contrastado
            new[] { 0.7631f, 0.7631f, 0.7631f, 0f, 0f },  // G → gris contrastado
            new[] { 0.1482f, 0.1482f, 0.1482f, 0f, 0f },  // B → gris contrastado
            new[] { 0f,      0f,      0f,      1f, 0f },  // A sin cambios
            new[] { -0.15f, -0.15f, -0.15f,   0f, 1f }   // offset de contraste
        });

        using ImageAttributes attributes = new();
        attributes.SetColorMatrix(colorMatrix);

        using Graphics g = Graphics.FromImage(result);
        g.DrawImage(
            source,
            new Rectangle(0, 0, source.Width, source.Height),
            0, 0, source.Width, source.Height,
            GraphicsUnit.Pixel,
            attributes);

        return result;
    }

    /*  Convierte un System.Drawing.Bitmap al SoftwareBitmap que requiere Windows.Media.Ocr.
        El pipeline: Bitmap → BMP en memoria → InMemoryRandomAccessStream → 
                                                                BitmapDecoder → SoftwareBitmap.*/
    private static async Task<SoftwareBitmap> ConvertToSoftwareBitmapAsync(Bitmap bitmap)
    {
        using MemoryStream memoryStream = new();
        bitmap.Save(memoryStream, ImageFormat.Bmp);
        byte[] imageBytes = memoryStream.ToArray();

        using InMemoryRandomAccessStream randomAccessStream = new();

        using (DataWriter writer = new(randomAccessStream))
        {
            writer.WriteBytes(imageBytes);
            await writer.StoreAsync();
            writer.DetachStream(); 
        }

        randomAccessStream.Seek(0);

        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        return await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);
    }
}
