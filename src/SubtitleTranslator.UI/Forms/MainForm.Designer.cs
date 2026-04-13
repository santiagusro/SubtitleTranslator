namespace SubtitleTranslator.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        btnStart           = new Button();
        btnStop            = new Button();
        btnConfigureRegion = new Button();
        btnClearHistory    = new Button();
        lblStatus          = new Label();
        SuspendLayout();

        // btnStart
        btnStart.Location = new Point(12, 12);
        btnStart.Name     = "btnStart";
        btnStart.Size     = new Size(100, 35);
        btnStart.Text     = "▶  Iniciar";
        btnStart.Click   += btnStart_Click;

        // btnStop
        btnStop.Enabled  = false;
        btnStop.Location = new Point(120, 12);
        btnStop.Name     = "btnStop";
        btnStop.Size     = new Size(100, 35);
        btnStop.Text     = "■  Detener";
        btnStop.Click   += btnStop_Click;

        // btnConfigureRegion
        btnConfigureRegion.Location = new Point(228, 12);
        btnConfigureRegion.Name     = "btnConfigureRegion";
        btnConfigureRegion.Size     = new Size(130, 35);
        btnConfigureRegion.Text     = "Configurar Región";
        btnConfigureRegion.Click   += btnConfigureRegion_Click;

        // btnClearHistory
        btnClearHistory.Location = new Point(366, 12);
        btnClearHistory.Name     = "btnClearHistory";
        btnClearHistory.Size     = new Size(108, 35);
        btnClearHistory.Text     = "Limpiar Historial";
        btnClearHistory.Click   += btnClearHistory_Click;

        // lblStatus
        lblStatus.AutoSize = false;
        lblStatus.Font     = new Font("Segoe UI", 10F);
        lblStatus.Location = new Point(12, 60);
        lblStatus.Name     = "lblStatus";
        lblStatus.Size     = new Size(460, 23);
        lblStatus.Text     = "○ Detenido";

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(486, 96);
        Controls.Add(btnStart);
        Controls.Add(btnStop);
        Controls.Add(btnConfigureRegion);
        Controls.Add(btnClearHistory);
        Controls.Add(lblStatus);
        MinimumSize = new Size(502, 135);
        Name        = "MainForm";
        Text        = "SubtitleTranslator";
        ResumeLayout(false);
    }

    #endregion

    private Button btnStart;
    private Button btnStop;
    private Button btnConfigureRegion;
    private Button btnClearHistory;
    private Label  lblStatus;
}
