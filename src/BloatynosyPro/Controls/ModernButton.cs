using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloatynosyPro.Controls;

public class ModernButton : Button
{
    public int BorderRadius { get; set; } = 8;
    public Color NormalColor { get; set; } = Color.FromArgb(99, 102, 241);
    public Color HoverColor { get; set; } = Color.FromArgb(129, 140, 248);
    public Color PressedColor { get; set; } = Color.FromArgb(79, 70, 229);

    private bool _isHovered;
    private bool _isPressed;

    public ModernButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        Cursor = Cursors.Hand;
        Size = new Size(160, 42);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _isPressed = true;
        Invalidate();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Color currentBg = _isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor);
        if (!Enabled)
            currentBg = Color.FromArgb(60, 64, 85);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreateRoundedRectanglePath(rect, BorderRadius);

        using var brush = new SolidBrush(currentBg);
        pevent.Graphics.FillPath(brush, path);

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            rect,
            Enabled ? ForeColor : Color.FromArgb(140, 145, 170),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak
        );
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
