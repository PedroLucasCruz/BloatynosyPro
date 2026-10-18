using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloatynosyPro.Controls;

public class ModernProgressBar : Control
{
    private int _value = 0;
    private int _maximum = 100;

    public int Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0, _maximum);
            Invalidate();
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(1, value);
            Invalidate();
        }
    }

    public Color ProgressColor { get; set; } = Color.FromArgb(6, 182, 212); // Neon Cyan
    public Color TrackColor { get; set; } = Color.FromArgb(36, 41, 64);
    public int BorderRadius { get; set; } = 6;

    public ModernProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(200, 14);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Fundo / Track
        var trackRect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var trackPath = CreateRoundedPath(trackRect, BorderRadius);
        using var trackBrush = new SolidBrush(TrackColor);
        e.Graphics.FillPath(trackBrush, trackPath);

        // Barra de progresso preenchida
        if (Value > 0)
        {
            int fillWidth = (int)((float)Value / Maximum * Width);
            if (fillWidth > 4)
            {
                var fillRect = new Rectangle(0, 0, fillWidth - 1, Height - 1);
                using var fillPath = CreateRoundedPath(fillRect, BorderRadius);
                using var fillBrush = new SolidBrush(ProgressColor);
                e.Graphics.FillPath(fillBrush, fillPath);
            }
        }
    }

    private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = Math.Min(radius * 2, rect.Height);
        if (d <= 0) d = 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(Math.Max(rect.X, rect.Right - d), rect.Y, d, d, 270, 90);
        path.AddArc(Math.Max(rect.X, rect.Right - d), Math.Max(rect.Y, rect.Bottom - d), d, d, 0, 90);
        path.AddArc(rect.X, Math.Max(rect.Y, rect.Bottom - d), d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
