# CLAUDE.md — SubtitleTranslator

Este archivo le indica a Claude Code cómo debe comportarse en este repositorio.
Leelo completo antes de tocar cualquier archivo.

---

## Identidad del Proyecto

**SubtitleTranslator** es una aplicación de escritorio en C# (.NET 8) con Windows Forms.
Captura subtítulos de pantalla, los procesa con OCR y muestra traducciones en tiempo real en un segundo monitor.

---

## Comandos Comunes

> El SDK instalado genera `SubtitleTranslator.slnx` (formato XML nuevo), no `.sln`.

```bash
# Build completo
dotnet build SubtitleTranslator.slnx

# Ejecutar la app
dotnet run --project src/SubtitleTranslator.UI/SubtitleTranslator.UI.csproj

# Todos los tests
dotnet test SubtitleTranslator.slnx

# Tests de un proyecto
dotnet test tests/SubtitleTranslator.Core.Tests/

# Tests de una clase específica
dotnet test tests/SubtitleTranslator.Core.Tests/ --filter "FullyQualifiedName~SubtitleChangeDetectorTests"
```

---

## Reglas de Comportamiento General

### Antes de escribir código
- Leer el archivo `ARCHITECTURE_PROMPT.md` si existe duda sobre dónde va algo
- Si una tarea requiere crear una clase nueva, primero identificar en qué capa (`Core`, `Infrastructure`, `UI`) corresponde
- Si hay ambigüedad sobre si algo es responsabilidad de `Core` o `Infrastructure`, preguntar antes de asumir

### Al escribir código
- **Siempre respetar el patrón de capas.** `Core` no referencia `Infrastructure`. `Infrastructure` no referencia `UI`.
- **No crear dependencias circulares** entre proyectos
- **No usar `var` cuando el tipo no es obvio** en la misma línea
- Preferir **expresiones** sobre **statements** cuando mejora la legibilidad
- Métodos de más de 30 líneas son una señal de que hay que refactorizar

### Nomenclatura
- Interfaces: prefijo `I` → `ITranslationService`
- Clases de implementación: nombre descriptivo sin sufijo genérico → `DeepLTranslationService`, no `TranslationServiceImpl`
- Métodos async: sufijo `Async` → `TranslateAsync`, `ExtractTextAsync`
- Eventos: nombre en pasado o presente continuo → `NewTranslationAvailable`, `ErrorOccurred`
- Variables privadas: prefijo `_` → `_captureRegion`, `_isRunning`

---

## Lo Que Claude Code PUEDE Hacer Sin Preguntar

- Crear nuevas clases dentro de la capa correcta
- Agregar tests unitarios para cualquier clase de `Core`
- Refactorizar métodos largos extrayendo métodos privados
- Agregar `using` statements que falten
- Corregir errores de compilación obvios
- Aplicar mejoras de legibilidad que no cambien comportamiento

---

## Lo Que Claude Code DEBE Preguntar Antes de Hacer

- Cambiar una interfaz pública (rompe implementaciones)
- Agregar una nueva dependencia NuGet
- Cambiar el proveedor de traducción por defecto
- Mover una clase entre capas
- Cambiar el esquema de `appsettings.json`
- Cualquier cambio que afecte a más de 2 archivos de forma coordinada

**Formato para preguntar:**
```
Quiero hacer X porque Y. ¿Procedo?
```
No redactar párrafos, una línea alcanza.

---

## Lo Que Claude Code NUNCA Debe Hacer

- Poner lógica de negocio en los `Form`
- Llamar a APIs externas desde la capa `Core`
- Hardcodear API keys, rutas, o valores de configuración
- Usar `Thread.Sleep` (usar `Task.Delay` con CancellationToken)
- Ignorar el `CancellationToken` en métodos async
- Crear clases `Helper`, `Utils`, o `Manager` sin justificación clara
- Mezclar inglés y español en nombres de variables/clases (el código va en inglés)

---

## Estructura de un Commit

```
tipo: descripción corta en infinitivo

feat: agregar soporte para LibreTranslate como proveedor alternativo
fix: corregir loop que no se detenía al cancelar el token
refactor: extraer lógica de debounce a SubtitleChangeDetector
test: agregar tests para CaptureRegion con dimensiones inválidas
```

Tipos válidos: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`

---

## Cómo Reportar lo que se Hizo

Al terminar una tarea, Claude Code debe responder con este formato:

```
✅ Hecho: [qué se hizo en una línea]

Archivos modificados:
- src/Core/Services/SubtitleMonitor.cs → [qué cambió]
- src/Infrastructure/Translation/DeepLTranslationService.cs → [qué cambió]

⚠️ Atención: [algo que el usuario debería saber, si aplica]
❓ Pendiente: [algo que requiere decisión del usuario, si aplica]
```

Si no hay advertencias ni pendientes, omitir esas líneas. Ser breve.

---

## Convenciones de Código

### Orden dentro de una clase
1. Campos privados (`_campo`)
2. Propiedades públicas
3. Constructor(es)
4. Métodos públicos
5. Métodos privados

### Manejo de errores
```csharp
// ✅ Correcto: excepciones tipadas con mensaje claro
throw new TranslationException($"DeepL API error: {statusCode}", innerException);

// ❌ Incorrecto: excepciones genéricas
throw new Exception("Error");
```

### Async
```csharp
// ✅ Correcto
public async Task<TranslationResult> TranslateAsync(string text, CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested();
    // ...
}

// ❌ Incorrecto: bloquear async
var result = TranslateAsync(text).Result;
```

### Null handling
```csharp
// ✅ Preferir null-coalescing y pattern matching
var text = result?.TranslatedText ?? string.Empty;

// ✅ Activar nullable reference types en el .csproj
<Nullable>enable</Nullable>
```

---

## Contexto de Negocio (para tomar mejores decisiones)

- El usuario final es **una persona viendo series/películas** mientras aprende un idioma
- La latencia de traducción es importante: si tarda más de 2 segundos se siente lento
- El OCR puede fallar en subtítulos con fondos complejos → los errores de OCR son esperados y no deben crashear la app
- El historial de traducciones **no se borra entre episodios** a menos que el usuario lo pida explícitamente
- La región de captura es probable que deba ajustarse según el reproductor de video que use el usuario (VLC, Netflix en Chrome, etc.)

---

## Preguntas Frecuentes

**¿Dónde va X?**
- Regla de negocio → `Core`
- Llamada HTTP / OCR / captura de pantalla → `Infrastructure`
- Cualquier cosa que involucre un control de Windows Forms → `UI`

**¿Testeo los Form?**
- No. Los Form son vistas tontas. Se testean los Presenters si contienen lógica.

**¿Puedo usar un framework de DI como Microsoft.Extensions.DependencyInjection?**
- Sí, es aceptable. Reemplazaría `ServiceLocator.cs`. Preguntar antes de agregar el NuGet.

**¿El código va en español o inglés?**
- Código (nombres de clases, métodos, variables) → **inglés**
- Comentarios → **español** si son para explicar decisiones de dominio, inglés si son técnicos
- Este archivo y los documentos de arquitectura → **español**