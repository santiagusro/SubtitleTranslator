using System.Net.Http.Json;
using System.Text.Json;
using SubtitleTranslator.Core.Exceptions;
using SubtitleTranslator.Core.Interfaces;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Infrastructure.Translation;

/// <summary>
/// Implementación de ITranslationService usando la API de DeepL.
/// El HttpClient inyectado debe tener configurados:
///   - BaseAddress: https://api-free.deepl.com/v2/ (o https://api.deepl.com/v2/ para Pro)
///   - DefaultRequestHeaders["Authorization"]: "DeepL-Auth-Key {api_key}"
/// </summary>
public sealed class DeepLTranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;

    public string ProviderName => "DeepL";

    public DeepLTranslationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TranslationResult> TranslateAsync(
        string text, string targetLanguage, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            HttpResponseMessage response = await PostTranslationRequestAsync(text, targetLanguage, ct);
            response.EnsureSuccessStatusCode();
            return await ParseSuccessResponseAsync(response, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TranslationException(
                $"DeepL HTTP error ({(int?)ex.StatusCode}): {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new TranslationException($"DeepL response parse error: {ex.Message}", ex);
        }
        catch (TranslationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TranslationException($"DeepL unexpected error: {ex.Message}", ex);
        }
    }

    private Task<HttpResponseMessage> PostTranslationRequestAsync(
        string text, string targetLanguage, CancellationToken ct)
    {
        var body = new
        {
            text = new[] { text },
            target_lang = targetLanguage
        };

        return _httpClient.PostAsJsonAsync("translate", body, ct);
    }

    private static async Task<TranslationResult> ParseSuccessResponseAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        using Stream responseStream = await response.Content.ReadAsStreamAsync(ct);
        using JsonDocument document = await JsonDocument.ParseAsync(responseStream, cancellationToken: ct);

        if (!document.RootElement.TryGetProperty("translations", out JsonElement translations)
            || translations.GetArrayLength() == 0)
        {
            return TranslationResult.Failure("DeepL returned an empty translations array.");
        }

        string? translatedText = translations[0].GetProperty("text").GetString();

        if (string.IsNullOrWhiteSpace(translatedText))
            return TranslationResult.Failure("DeepL returned an empty translation.");

        return TranslationResult.Success(translatedText);
    }
}
