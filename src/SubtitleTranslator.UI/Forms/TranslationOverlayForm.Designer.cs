namespace SubtitleTranslator.UI;

partial class TranslationOverlayForm
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
        rtbTranslations = new RichTextBox();
        btnClear        = new Button();
        btnCopy         = new Button();
        SuspendLayout();

        // rtbTranslations
        rtbTranslations.Anchor      = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        rtbTranslations.BackColor   = Color.Black;
        rtbTranslations.BorderStyle = BorderStyle.None;
        rtbTranslations.Font        = new Font("Segoe UI", 14F);
        rtbTranslations.ForeColor   = Color.White;
        rtbTranslations.Location    = new Point(0, 0);
        rtbTranslations.Name        = "rtbTranslations";
        rtbTranslations.ReadOnly    = true;
        rtbTranslations.ScrollBars  = RichTextBoxScrollBars.Vertical;
        rtbTranslations.Size        = new Size(900, 265);
        rtbTranslations.TabStop     = false;

        // btnCopy
        btnCopy.Anchor    = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCopy.BackColor = Color.FromArgb(45, 45, 45);
        btnCopy.FlatStyle = FlatStyle.Flat;
        btnCopy.ForeColor = Color.White;
        btnCopy.Location  = new Point(712, 270);
        btnCopy.Name      = "btnCopy";
        btnCopy.Size      = new Size(85, 26);
        btnCopy.Text      = "Copiar";
        btnCopy.Click    += btnCopy_Click;

        // btnClear
        btnClear.Anchor    = AnchorStyles.Bottom | AnchorStyles.Right;
        btnClear.BackColor = Color.FromArgb(45, 45, 45);
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.ForeColor = Color.White;
        btnClear.Location  = new Point(805, 270);
        btnClear.Name      = "btnClear";
        btnClear.Size      = new Size(85, 26);
        btnClear.Text      = "Limpiar";
        btnClear.Click    += btnClear_Click;

        // TranslationOverlayForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(900, 300);
        Controls.Add(rtbTranslations);
        Controls.Add(btnCopy);
        Controls.Add(btnClear);
        Name          = "TranslationOverlayForm";
        StartPosition = FormStartPosition.Manual;
        Text          = "Traducciones";
        ResumeLayout(false);
    }

    #endregion

    private RichTextBox rtbTranslations;
    private Button      btnCopy;
    private Button      btnClear;
}
