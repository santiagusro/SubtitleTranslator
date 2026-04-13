# Prompt de Arquitectura — SubtitleTranslator

## Contexto del Proyecto

Construir una aplicación de escritorio en **C# con Windows Forms (.NET 8)** que:

1. Capture una región configurable de la pantalla donde aparecen los subtítulos
2. Extraiga el texto de esa región mediante OCR
3. Detecte cuándo el subtítulo cambia
4. Lo traduzca al idioma configurado por el usuario
5. Muestre las traducciones acumuladas línea por línea en una ventana secundaria (segundo monitor)

---

## Stack Tecnológico

- **Lenguaje:** C# (.NET 8)
- **UI:** Windows Forms
- **OCR:** `Windows.Media.Ocr` (API nativa de Windows 10+)
- **Captura de pantalla:** `Windows.Graphics.Capture` o `Graphics.CopyFromScreen`
- **Traducción:** DeepL API (con abstracción para poder cambiar de proveedor)
- **HTTP:** `HttpClient` con `IHttpClientFactory`
- **Configuración:** `System.Text.Json` + archivo `appsettings.json`
- **Tests:** xUnit

---

## Estructura de Directorios

```
SubtitleTranslator/
├── src/
│   ├── SubtitleTranslator.Core/              # Lógica de negocio pura, sin dependencias externas
│   │   ├── Interfaces/
│   │   │   ├── IScreenCapturer.cs
│   │   │   ├── IOcrEngine.cs
│   │   │   ├── ITranslationService.cs
│   │   │   ├── ISubtitleChangeDetector.cs
│   │   │   └── ITranslationHistoryRepository.cs
│   │   ├── Models/
│   │   │   ├── CaptureRegion.cs
│   │   │   ├── SubtitleLine.cs
│   │   │   ├── TranslationResult.cs
│   │   │   └── AppSettings.cs
│   │   ├── Services/
│   │   │   ├── SubtitleMonitor.cs             # Orquestador principal (loop de captura)
│   │   │   └── SubtitleChangeDetector.cs
│   │   └── Exceptions/
│   │       ├── OcrException.cs
│   │       └── TranslationException.cs
│   │
│   ├── SubtitleTranslator.Infrastructure/     # Implementaciones concretas de las interfaces
│   │   ├── Capture/
│   │   │   └── WindowsScreenCapturer.cs
│   │   ├── Ocr/
│   │   │   └── WindowsOcrEngine.cs
│   │   ├── Translation/
│   │   │   ├── DeepLTranslationService.cs
│   │   │   └── LibreTranslationService.cs     # Alternativa open source
│   │   ├── Persistence/
│   │   │   └── InMemoryTranslationHistoryRepository.cs
│   │   └── Configuration/
│   │       └── AppSettingsLoader.cs
│   │
│   └── SubtitleTranslator.UI/                 # Capa de presentación (Windows Forms)
│       ├── Forms/
│       │   ├── MainForm.cs                    # Ventana de control principal
│       │   ├── TranslationOverlayForm.cs      # Ventana del segundo monitor con traducciones
│       │   └── RegionSelectorForm.cs          # Selector visual de región de pantalla
│       ├── ViewModels/
│       │   └── TranslationLineViewModel.cs
│       ├── Presenters/
│       │   ├── MainPresenter.cs
│       │   └── OverlayPresenter.cs
│       ├── DependencyInjection/
│       │   └── ServiceLocator.cs              # Composición de dependencias (sin framework DI)
│       └── Program.cs
│
└── tests/
    ├── SubtitleTranslator.Core.Tests/
    │   ├── Services/
    │   │   ├── SubtitleMonitorTests.cs
    │   │   └── SubtitleChangeDetectorTests.cs
    │   └── Models/
    │       └── CaptureRegionTests.cs
    └── SubtitleTranslator.Infrastructure.Tests/
        └── Translation/
            └── DeepLTranslationServiceTests.cs
```

---

## Modelos de Dominio

### `CaptureRegion`
Representa la región de la pantalla configurada por el usuario.
- Propiedades: `X`, `Y`, `Width`, `Height`
- Debe ser **inmutable** (record o constructor privado con factory method)
- Debe validar que las dimensiones sean positivas en construcción
- Método: `bool IsValid()`

### `SubtitleLine`
Representa una línea de subtítulo con su traducción.
- Propiedades: `OriginalText`, `TranslatedText`, `DetectedAt` (DateTime), `SourceLanguage`, `TargetLanguage`
- Inmutable
- No puede tener `OriginalText` vacío

### `TranslationResult`
Wrapper de resultado para operaciones de traducción (patrón Result).
- Propiedades: `bool IsSuccess`, `string TranslatedText`, `string? ErrorMessage`
- Factory methods: `TranslationResult.Success(text)`, `TranslationResult.Failure(error)`

### `AppSettings`
- `CaptureRegion Region`
- `string TargetLanguage` (código ISO, ej: "ES", "FR")
- `int CaptureIntervalMs` (default: 500ms)
- `string TranslationProviderName`
- `Dictionary<string, string> ApiKeys`

---

## Interfaces Clave (Contratos)

### `IScreenCapturer`
```csharp
public interface IScreenCapturer
{
    Bitmap CaptureRegion(CaptureRegion region);
}
```

### `IOcrEngine`
```csharp
public interface IOcrEngine
{
    Task<string> ExtractTextAsync(Bitmap image);
}
```

### `ITranslationService`
```csharp
public interface ITranslationService
{
    Task<TranslationResult> TranslateAsync(string text, string targetLanguage, CancellationToken ct = default);
    string ProviderName { get; }
}
```

### `ISubtitleChangeDetector`
```csharp
public interface ISubtitleChangeDetector
{
    bool HasChanged(string previousText, string currentText);
    void Reset();
}
```

### `ITranslationHistoryRepository`
```csharp
public interface ITranslationHistoryRepository
{
    void Add(SubtitleLine line);
    IReadOnlyList<SubtitleLine> GetAll();
    void Clear();
}
```

---

## Servicio Principal: `SubtitleMonitor`

Es el orquestador del loop de captura. **No depende de ninguna implementación concreta**, solo de interfaces.

**Responsabilidades:**
1. Ejecutar el loop de polling cada `CaptureIntervalMs` milisegundos
2. Capturar la región → pasarla al OCR → comparar con el texto anterior
3. Si cambió: traducir → guardar en historial → notificar a la UI
4. Manejar errores de cada etapa sin romper el loop

**Eventos que expone:**
- `event EventHandler<SubtitleLine> NewTranslationAvailable`
- `event EventHandler<Exception> ErrorOccurred`

**Métodos públicos:**
- `Task StartAsync(CancellationToken ct)`
- `void Stop()`
- `bool IsRunning { get; }`

El loop interno debe correr en un hilo separado y **nunca bloquear el hilo de UI**.

---

## Capa de UI (Windows Forms)

### Patrón: MVP (Model-View-Presenter)
No usar lógica de negocio dentro de los Form. Los Form son vistas tontas.

### `MainForm`
- Botones: Iniciar / Detener / Configurar Región / Limpiar historial
- Label que muestra el estado actual (Corriendo / Detenido / Error)
- No contiene lógica, delega todo al `MainPresenter`

### `TranslationOverlayForm`
- Ventana `TopMost = true`, posicionable en el segundo monitor
- `RichTextBox` o `ListBox` donde se acumulan las líneas traducidas (la más nueva abajo)
- Fondo semitransparente opcional
- Nunca desaparece el texto anterior (la ventana acumula, no reemplaza)
- Botón de limpiar y de copiar al portapapeles

### `RegionSelectorForm`
- Overlay transparente sobre toda la pantalla
- El usuario arrastra para seleccionar la región rectangular
- Al soltar, retorna un `CaptureRegion` al presenter

---

## Principios SOLID a Respetar Estrictamente

### Single Responsibility
- Cada clase tiene **una única razón para cambiar**
- `SubtitleMonitor` no sabe nada de UI ni de HTTP
- `DeepLTranslationService` no sabe nada de OCR ni captura
- Los `Form` no hacen llamadas HTTP ni lógica de negocio

### Open/Closed
- Para agregar un nuevo proveedor de traducción: crear una nueva clase que implemente `ITranslationService`, **sin modificar ninguna clase existente**
- Para cambiar el motor de OCR: nueva clase que implemente `IOcrEngine`

### Liskov Substitution
- `DeepLTranslationService` y `LibreTranslationService` deben ser 100% intercambiables
- Si el código funciona con uno, debe funcionar con el otro sin cambios

### Interface Segregation
- Las interfaces deben ser pequeñas y específicas
- No crear una interfaz `ITranslationEngine` que mezcle captura + OCR + traducción

### Dependency Inversion
- `SubtitleMonitor` depende de `IScreenCapturer`, `IOcrEngine`, `ITranslationService`, nunca de las clases concretas
- La composición de dependencias ocurre únicamente en `ServiceLocator.cs` o `Program.cs`

---

## Manejo de Errores

- Cada capa tiene sus propias excepciones tipadas (`OcrException`, `TranslationException`)
- El loop de `SubtitleMonitor` **nunca lanza**, captura y emite el evento `ErrorOccurred`
- La UI muestra errores al usuario sin crashear la aplicación
- Los errores de traducción no deben detener el monitoreo
- Usar `CancellationToken` en todas las operaciones async para poder cancelar limpiamente

---

## Consideraciones de Rendimiento

- La captura de pantalla debe hacerse en el hilo de background, nunca en el UI thread
- El OCR con `Windows.Media.Ocr` es async nativamente, aprovechar eso
- Si el texto del frame actual es idéntico al anterior, **no llamar a la API de traducción** (comparación de string antes de la llamada HTTP)
- Preprocesar la imagen antes del OCR: convertir a escala de grises, aumentar contraste (mejora la precisión en subtítulos sobre fondos de video)
- Debounce: si el subtítulo cambia pero sigue cambiando en los próximos 200ms, esperar a que se estabilice antes de traducir (evita traducciones de texto parcial)

---

## Configuración y Persistencia

- `appsettings.json` en el directorio del ejecutable
- La `CaptureRegion` se persiste entre sesiones
- La API key nunca se loguea ni se expone en la UI
- El idioma destino es configurable sin reiniciar la app

---

## Testing

- Testear `SubtitleChangeDetector` con casos: texto igual, texto diferente, texto vacío, texto con espacios extra
- Testear `SubtitleMonitor` con mocks de todas las interfaces
- Testear `TranslationResult` (patrón Result)
- Testear `CaptureRegion` validaciones
- **No testear** la capa de UI ni las implementaciones de infraestructura (esas van a integración)
- Usar `Moq` o implementaciones stub manuales para los mocks

---

## Lo que NO hacer

- No poner lógica en los constructores
- No usar `static` para estado mutable compartido
- No llamar a APIs externas directamente desde los Form
- No mezclar `async void` salvo en event handlers de Windows Forms (y ahí manejar excepciones explícitamente)
- No hardcodear la API key en el código fuente