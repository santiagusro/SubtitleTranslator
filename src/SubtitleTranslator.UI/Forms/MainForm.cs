using SubtitleTranslator.UI.DependencyInjection;
using SubtitleTranslator.UI.Presenters;

namespace SubtitleTranslator.UI;

public partial class MainForm : Form
{
    private readonly MainPresenter _presenter;
    private TranslationOverlayForm? _overlayForm;

    public MainForm()
    {
        InitializeComponent();

        _presenter = new MainPresenter(
            monitor:           ServiceLocator.SubtitleMonitor,
            settings:          ServiceLocator.Settings,
            settingsLoader:    ServiceLocator.AppSettingsLoader,
            historyRepository: ServiceLocator.HistoryRepository);

        _presenter.MonitorErrorOccurred += OnMonitorError;
        UpdateControls();
    }

    // ── Handlers de botones ──────────────────────────────────────────────────

    private async void btnStart_Click(object sender, EventArgs e)
    {
        await _presenter.StartAsync();

        if (_overlayForm is null || _overlayForm.IsDisposed)
        {
            _overlayForm = new TranslationOverlayForm();
            _overlayForm.Show(this);
        }

        UpdateControls();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _presenter.Stop();
        UpdateControls();
    }

    private void btnConfigureRegion_Click(object sender, EventArgs e)
    {
        _presenter.ConfigureRegion();
    }

    private void btnClearHistory_Click(object sender, EventArgs e)
    {
        _presenter.ClearHistory();
    }

    // ── Actualización de estado ──────────────────────────────────────────────

    private void UpdateControls()
    {
        bool running = _presenter.IsRunning;

        lblStatus.ForeColor = running ? Color.LimeGreen : SystemColors.ControlText;
        lblStatus.Text      = running ? "● Corriendo" : "○ Detenido";
        btnStart.Enabled    = !running;
        btnStop.Enabled     = running;
    }

    private void OnMonitorError(object? sender, Exception ex)
    {
        if (InvokeRequired)
            BeginInvoke(() => ShowError(ex));
        else
            ShowError(ex);
    }

    private void ShowError(Exception ex)
    {
        lblStatus.ForeColor = Color.Crimson;
        lblStatus.Text      = $"✕ Error: {ex.Message}";
        btnStart.Enabled    = true;
        btnStop.Enabled     = false;
    }
}
