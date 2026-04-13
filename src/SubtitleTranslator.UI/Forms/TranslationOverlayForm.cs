using SubtitleTranslator.Core.Models;
using SubtitleTranslator.UI.DependencyInjection;
using SubtitleTranslator.UI.Presenters;

namespace SubtitleTranslator.UI;

public partial class TranslationOverlayForm : Form
{
    private readonly OverlayPresenter _presenter;

    public TranslationOverlayForm()
    {
        InitializeComponent();

        // Propiedades visuales del overlay configuradas por código, no por el Designer
        TopMost         = true;
        FormBorderStyle = FormBorderStyle.None;
        Opacity         = 0.85;
        BackColor       = Color.Black;

        _presenter = new OverlayPresenter(
            monitor:           ServiceLocator.SubtitleMonitor,
            historyRepository: ServiceLocator.HistoryRepository);

        _presenter.TranslationReceived += OnTranslationReceived;

        // Cargar el historial existente al abrir el overlay
        foreach (SubtitleLine line in _presenter.GetHistory())
            AppendTranslation(line);
    }

    // ── Handlers de botones ──────────────────────────────────────────────────

    private void btnClear_Click(object sender, EventArgs e)
    {
        rtbTranslations.Clear();
    }

    private void btnCopy_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(rtbTranslations.Text))
            Clipboard.SetText(rtbTranslations.Text);
    }

    // ── Traducción recibida desde el presenter ───────────────────────────────

    private void OnTranslationReceived(object? sender, SubtitleLine line)
    {
        // Las traducciones llegan desde el background thread del SubtitleMonitor
        if (InvokeRequired)
            BeginInvoke(() => AppendTranslation(line));
        else
            AppendTranslation(line);
    }

    private void AppendTranslation(SubtitleLine line)
    {
        rtbTranslations.AppendText($"{line.OriginalText}  →  {line.TranslatedText}{Environment.NewLine}");
        rtbTranslations.ScrollToCaret();
    }

    // ── Limpieza al cerrar ───────────────────────────────────────────────────

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _presenter.TranslationReceived -= OnTranslationReceived;
        _presenter.Unsubscribe();
        base.OnFormClosed(e);
    }
}
