using System.Drawing.Drawing2D;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.UI;

/// <summary>
/// Overlay de pantalla completa que permite al usuario dibujar con el ratón
/// la región donde aparecen los subtítulos. No usa Designer: todo por código.
/// </summary>
public sealed class RegionSelectorForm : Form
{
    private Point _startPoint;
    private Point _currentPoint;
    private bool  _selecting;

    public CaptureRegion? SelectedRegion { get; private set; }

    public RegionSelectorForm()
    {
        WindowState     = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        TopMost         = true;
        Opacity         = 0.5;
        BackColor       = Color.Gray;
        Cursor          = Cursors.Cross;

        // Evita el parpadeo al redibujar el rectángulo en cada MouseMove
        DoubleBuffered = true;

        // Cancelar con Escape
        KeyPreview = true;
        KeyDown   += OnKeyDown;
    }

    // ── Teclado ──────────────────────────────────────────────────────────────

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    // ── Mouse ────────────────────────────────────────────────────────────────

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            base.OnMouseDown(e);
            return;
        }

        _startPoint   = e.Location;
        _currentPoint = e.Location;
        _selecting    = true;
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_selecting)
        {
            _currentPoint = e.Location;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !_selecting)
        {
            base.OnMouseUp(e);
            return;
        }

        _selecting    = false;
        _currentPoint = e.Location;

        Rectangle clientRect = GetSelectionRect();

        if (clientRect.Width > 0 && clientRect.Height > 0)
        {
            // Convertir a coordenadas de pantalla para que CaptureRegion sea
            // válida independientemente de la posición real del form
            Point screenOrigin = PointToScreen(new Point(clientRect.X, clientRect.Y));
            SelectedRegion = CaptureRegion.Create(
                screenOrigin.X, screenOrigin.Y,
                clientRect.Width, clientRect.Height);
            DialogResult = DialogResult.OK;
        }
        else
        {
            DialogResult = DialogResult.Cancel;
        }

        Close();
        base.OnMouseUp(e);
    }

    // ── Pintura ──────────────────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_selecting)
            return;

        Rectangle rect = GetSelectionRect();

        // Relleno semitransparente que diferencia el área seleccionada del overlay
        using SolidBrush fillBrush = new(Color.FromArgb(50, Color.White));
        e.Graphics.FillRectangle(fillBrush, rect);

        // Borde punteado blanco
        using Pen borderPen = new(Color.White, 2) { DashStyle = DashStyle.Dash };
        e.Graphics.DrawRectangle(borderPen, rect);

        // Dimensiones dentro del rectángulo
        if (rect.Width > 60 && rect.Height > 20)
        {
            string label = $"{rect.Width} × {rect.Height}";
            using Font  font      = new("Segoe UI", 9F, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.White);
            e.Graphics.DrawString(label, font, textBrush, rect.X + 4, rect.Y + 4);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private Rectangle GetSelectionRect() =>
        new(
            Math.Min(_startPoint.X, _currentPoint.X),
            Math.Min(_startPoint.Y, _currentPoint.Y),
            Math.Abs(_currentPoint.X - _startPoint.X),
            Math.Abs(_currentPoint.Y - _startPoint.Y));
}
